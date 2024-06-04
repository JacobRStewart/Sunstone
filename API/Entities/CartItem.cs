using System.ComponentModel.DataAnnotations.Schema;
using API.Entities;

namespace Sunstone;

[Table("CartItems")]
public class CartItem
{
    public int Id { get; set; }

    public int Quantity { get; set; }

    // Navigation properties
    public int ProductId { get; set; }

    public Product Product { get; set; }

    public int CartId { get; set; }

    public Cart Cart { get; set; }
}
