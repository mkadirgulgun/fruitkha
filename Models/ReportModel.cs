namespace Fruitkha.Models
{
    public class ProductReport
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int SalesQuantity{ get; set; }
        public int TotalSales { get; set; }
    }   
    public class UserReport
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public int ProductsNumber { get; set; }
        public int TotalAmount { get; set; }
    }
    public class StatusReport
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int StatusId { get; set; }
        public string Email { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerZipCode { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public string? StatusName { get; set; }
        public int? Rating { get; set; }
        public string? CommentDetail { get; set; }
    }
    public class StatusModel
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }
    public class ChangeStatus
    {
        public int StatusId { get; set; }
    }
}   
