namespace Fruitkha.Models
{
	public class Login
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Password { get; set; }
		public string PasswordRepeat { get; set; }
		public string Email { get; set; }
        public int? Autharization { get; set; }
    }
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string? Password { get; set; }
		public string Address { get; set; }
		public string City { get; set; }
		public string ZipCode { get; set; }	
	}
}
