namespace ShopApi.Models;

public class Shop
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Category { get; set; }

    public string Description { get; set; } 

    public int Price { get; set; }

    public int Cost { get; set; }

    public DateTime ExpirationDate { get; set; }

    public int Stock { get; set; }
}
