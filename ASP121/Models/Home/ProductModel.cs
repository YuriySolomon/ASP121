namespace ASP121.Models.Home
{
    public class Product
    {
        public String Name { get; set; }
        public Double Price { get; set; }
        public String? Image { get; set; }
        public String? Description { get; set; }

    }
    public class ProductModel
    {
        public List<Product>? Products { get; set; }
    }
}
