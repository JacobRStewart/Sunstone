using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sunstone;

namespace API.Controllers
{
    public class ProductsController : BaseAPIController
    {
       private readonly StoreContext context;
       
       public ProductsController(StoreContext context)
       {
        System.Diagnostics.Debug.WriteLine("This is a context");
            this.context = context;
        
       }

       [HttpGet]
       public async Task <ActionResult<List<Product>>> GetProducts()
       {
            return await context.Products.ToListAsync();
       }

       [HttpGet("{id}")]
       public async Task <ActionResult<Product>> GetProducts(int id)
       {
            var product = await context.Products.FindAsync(id);

            if (product == null) return NotFound();

            return product;
       }
    }
}