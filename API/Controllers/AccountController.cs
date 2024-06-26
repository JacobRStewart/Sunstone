using System.Runtime.CompilerServices;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.VisualBasic;
using Sunstone;

namespace API.Controllers
{
    public class AccountController : BaseAPIController
    {
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        private StoreContext _context;

        public AccountController(UserManager<User> userManager, TokenService tokenService, StoreContext context)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto) 
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return Unauthorized();

            var userCart = await RetrieveCart(loginDto.Username);
            var anonCart = await RetrieveCart(Request.Cookies["buyerId"]); 

            if (userCart != null) Console.WriteLine(userCart);
            if (anonCart != null) Console.WriteLine(anonCart);


            if (anonCart != null) 
            {
                if (userCart != null) {
                    _context.Carts.Remove(userCart);
                }
                anonCart.BuyerId = user.UserName;
                Response.Cookies.Delete("buyerId");
                await _context.SaveChangesAsync();
            }

            return new UserDTO
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Cart = anonCart != null ? anonCart.MapCartToDTO() : userCart?.MapCartToDTO()
            };


        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO registerDto)
        {
            var user = new User{UserName = registerDto.Username, Email = registerDto.Email};
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) 
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user, "Member");

            return StatusCode(201);
        }

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userCart = await RetrieveCart(User.Identity.Name);

            return new UserDTO
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Cart =  userCart?.MapCartToDTO()
            };
        }

        [Authorize]
        [HttpGet("savedAddress")]
        public async Task<ActionResult<UserAddress>> GetSavedAddress() 
        {
            return await _userManager.Users
                .Where(x => x.UserName == User.Identity.Name)
                .Select(user => user.Address)
                .FirstOrDefaultAsync();
        }


        private async Task<Cart> RetrieveCart(string buyerId)
        {
            if (string.IsNullOrEmpty(buyerId))
            {
                Console.WriteLine("Deleting Buyer Id cookie, since buyer Id is empty.");
                Response.Cookies.Delete("buyerId");
            }
            Console.WriteLine("Returning cart context and looping through items and products");
            return await _context.Carts
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x => x.BuyerId == buyerId);
        }
    }
}