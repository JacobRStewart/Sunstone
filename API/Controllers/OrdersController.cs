using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Entities.OrderAggregate;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sunstone;

namespace API.Controllers
{
    [Authorize]
    public class OrdersController : BaseAPIController
    {
        private readonly StoreContext _context;
        public OrdersController(StoreContext context)
        {
            _context = context;
            
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDTO>>> GetOrders()
        {
            return await _context.Orders
                .ProjectOrderToOrderDTO()
                .Where(x => x.BuyerId == User.Identity.Name)
                .ToListAsync();
                
        }

        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<ActionResult<OrderDTO>> GetOrder(int id) 
        {
            return await _context.Orders
                .ProjectOrderToOrderDTO()
                .Where(x => x.BuyerId == User.Identity.Name && x.Id == id)
                .FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder(CreateOrderDTO orderDto)
        {
            var cart = await _context.Carts
                .RetrieveCartWithItems(User.Identity.Name)
                .FirstOrDefaultAsync();

            if (cart == null) return BadRequest(new ProblemDetails{Title = "Could not locate cart"});

            var items = new List<OrderItem>();

            foreach (var item in cart.Items)
            {
                var productItem = await _context.Products.FindAsync(item.ProductId);
                var itemOrdered = new ProductItemOrdered
                {
                    ProductId = productItem.Id,
                    Name = productItem.Name,
                    PictureURL = productItem.PictureURL
                };

                var orderItem = new OrderItem
                {
                    ItemOrdered = itemOrdered,
                    Price = productItem.Price,
                    Quantity = item.Quantity
                };

                items.Add(orderItem);
                productItem.QuantityInStock -= item.Quantity;
            }

            var subtotal = items.Sum(item => item.Price * item.Quantity);
            var deliveryFee = subtotal > 10000 ? 0 : 500;

            var order = new Order
            {
                OrderItems = items,
                BuyerId = User.Identity.Name,
                ShippingAddress = orderDto.ShippingAddress,
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                PaymentIntentId = cart.PaymentIntentId
            };

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);

            if (orderDto.SaveAddress)
            {
                var user = await _context.Users
                .Include(a => a.Address)
                .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);


                var address = new UserAddress
                {
                    FullName = orderDto.ShippingAddress.FullName,
                    Address1 = orderDto.ShippingAddress.Address1,
                    Address2 = orderDto.ShippingAddress.Address2,
                    City = orderDto.ShippingAddress.City,
                    State = orderDto.ShippingAddress.State,
                    Zip = orderDto.ShippingAddress.Zip,
                    Country = orderDto.ShippingAddress.Country
                };

                user.Address = address;

            } 

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return CreatedAtRoute("GetOrder", new {id = order.Id}, order.Id);

            return BadRequest("Problem creating order");
        }

    }
}