namespace Fruitkha.Models
{
	public class Product
	{
		public int Id { get; set; }
		public int CategoryId { get; set; }
		public string Name { get; set; }
		public int Price { get; set; }
		public IFormFile Image { get; set; }
		public string? CategoryName { get; set; }
		public string? ImgUrl { get; set; }
		public int Stock { get; set; }
		public string Details { get; set; }
	}
	public class Category
	{
		public int Id { get; set; }

		public string CategoryName { get; set; }
	}
	public class Cart
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public int UserId { get; set; }	
        public string ProductName { get; set; }
		public int ProductPrice { get; set; }
		public int Quantity { get; set; }
		public int Subtotal { get; set; }
		public string ProductUrl { get; set; }
	}	
	public class CartItem
	{
        public List<Cart> CartUpdate { get; set; }	
    }
}
