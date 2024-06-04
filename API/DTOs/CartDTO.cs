namespace Sunstone;

public class CartDTO
{
    public int Id { get; set; }

    public string BuyerId { get; set; }

    public List<CartItemDTO> Items {get; set; }
}
