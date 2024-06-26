using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sunstone;

namespace API.Extensions
{
    public static class CartExtensions
    {
        public static CartDTO MapCartToDTO(this Cart cart)
        {
            return new CartDTO
            {
                Id = cart.Id,
                BuyerId = cart.BuyerId,
                Items = cart.Items.Select(item => new CartItemDTO
                {
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    Price = item.Product.Price,
                    PictureURL = item.Product.PictureURL,
                    Type = item.Product.Type,
                    Brand = item.Product.Brand,
                    Quantity = item.Quantity
                }).ToList()
            };
        }

        public static IQueryable<Cart> RetrieveCartWithItems(this IQueryable<Cart> query, string buyerId) 
        {
            return query.Include(i => i.Items)
                .ThenInclude(p => p.Product).Where(b => b.BuyerId == buyerId);

        }
    }
}