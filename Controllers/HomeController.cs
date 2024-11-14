using Dapper;
using Fruitkha.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace Fruitkha.Controllers
{
    public class HomeController : Controller
    {
        
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT * FROM products WHERE Stock > 0").ToList();
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            ViewData["SessionId"] = HttpContext.Session.GetInt32("SessionId");
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");

            ViewData["Location"] = HttpContext.Session.GetString("Location");
            HttpContext.Session.SetString("Location", "Index");

            if (ViewData["UserId"] == null)
            {

                Random rnd = new Random();
                HttpContext.Session.SetInt32("UserId", rnd.Next());

            }

            return View(products);
        }
        public IActionResult Detail(int id)
        {
            using var connection = new SqlConnection(connectionString);
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            HttpContext.Session.SetString("Location", "Cart");
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");

            var sql = "SELECT * FROM products WHERE Id = @Id";
            var detail = connection.QuerySingleOrDefault<Product>(sql, new { Id = id });


            var comments = connection.Query<Comment>("SELECT * FROM comments WHERE ProductId = @Id ORDER BY Rating DESC", new { Id = id }).ToList();




            ViewBag.Comments = comments;
            ViewBag.Control = ViewData["Id"];

            return View(detail);
        }
        public IActionResult AboutUs()
        {
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");

            return View();
        }
        public IActionResult Contact()
        {
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");

            return View();
        }
        public IActionResult Products()
        {
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT products.*, CategoryName FROM products LEFT JOIN categories ON categories.Id = products.CategoryId WHERE products.Stock > 0 ").ToList();
            var categories = connection.Query<Category>("SELECT * FROM categories ").ToList();
            ViewBag.Categories = categories;
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");

            HttpContext.Session.SetString("Location", "Products");
            return View(products);
        }
        public IActionResult Cart()
        {
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            var userId = ViewData["UserId"];
            using var connection = new SqlConnection(connectionString);
            var cart = connection.Query<Cart>("SELECT * FROM cart WHERE UserId = @userId", new { userId }).ToList();

            var subTotalSql = "SELECT SUM(ProductPrice * Quantity) AS Subtotal\r\nFROM cart\r\nWHERE UserId = @userId";
            var subTotal = connection.QueryFirstOrDefault<Cart>(subTotalSql, new { userId });
            ViewBag.SubTotal = subTotal.Subtotal;
            return View(cart);
        }
        public IActionResult CheckOut()
        {
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            var userId = ViewData["UserId"];
            using var connection = new SqlConnection(connectionString);
            var cart = connection.Query<Cart>("SELECT * FROM cart WHERE UserId = @userId", new { userId }).ToList();
            ViewBag.CheckOut = cart;
            var subTotalSql = "SELECT SUM(ProductPrice * Quantity) AS Subtotal\r\nFROM cart\r\nWHERE UserId = @userId";
            var subTotal = connection.QueryFirstOrDefault<Cart>(subTotalSql, new { userId });
            ViewBag.SubTotal = subTotal.Subtotal;
            return View(new User());
        }
        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index", "Home");
            }
            using var connection = new SqlConnection(connectionString);


            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");

            var sql = "SELECT * FROM Products WHERE Name LIKE @Query";
            var results = connection.Query<Product>(sql, new { Query = "%" + query + "%" }).ToList();
            return View(results);

        }
    }
}
