namespace Fruitkha.Models
{
    public class Comment
    {
        public int? Id { get; set; }
        public int ProductId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; }
        public string CommentDetail { get; set; }
        public string? ProductName { get; set; }

    }
}
