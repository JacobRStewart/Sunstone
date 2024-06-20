using System.Reflection.Metadata.Ecma335;
using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sunstone;

public class CartController : BaseAPIController
{
    private readonly StoreContext _context;
    public CartController(StoreContext context)
    {
        _context = context;
    }

    [HttpGet(Name = "GetCart")]
    public async Task<ActionResult<CartDTO>> GetCart()
    {
        var cart = await RetrieveCart(GetBuyerId());

        if (cart == null) return NotFound();

        return cart.MapCartToDTO();
    }


    [HttpPost]
    public async Task<ActionResult<CartDTO>> AddItemToBasket(int productId, int quantity) 
    {
        var cart = await RetrieveCart(GetBuyerId());

        if (cart == null) cart = CreateCart();

        var product = await _context.Products.FindAsync(productId);

        if (product == null) return BadRequest(new ProblemDetails{Title = "Product Not Found"});

        cart.AddItem(product, quantity);
        
        var result = await _context.SaveChangesAsync() > 0;

        if (result) return CreatedAtRoute("GetCart", cart.MapCartToDTO());

        return BadRequest(new ProblemDetails{Title = "Problem saving item to basket"});
    }

    [HttpDelete]

    public async Task<ActionResult> RemoveCartItem(int productId, int quantity)
    {
        var cart = await RetrieveCart(GetBuyerId());

        if (cart == null) return NotFound();

        cart.RemoveItem(productId, quantity);

        var result = await _context.SaveChangesAsync() > 0;

        if (result) return Ok();

        return BadRequest(new ProblemDetails{Title = "Problem removing item from cart"});
    }

    private async Task<Cart> RetrieveCart(string buyerId)
    {
        if (string.IsNullOrEmpty(buyerId))
        {
            Response.Cookies.Delete("buyerId");
        }
        return await _context.Carts
            .Include(i => i.Items)
            .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(x => x.BuyerId == buyerId);
    }

    private string GetBuyerId()
    {
        return User.Identity?.Name ?? Request.Cookies["buyerId"];
    }
    private Cart CreateCart()
    {
        var buyerId = User.Identity?.Name;
        if (string.IsNullOrEmpty(buyerId))
        {
            buyerId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions{IsEssential = true, Expires = DateTime.Now.AddDays(30)};
            Response.Cookies.Append("buyerId", buyerId, cookieOptions);
        }
 
        var cart = new Cart{BuyerId = buyerId};
        _context.Carts.Add(cart);
        return cart;
    } 

}
