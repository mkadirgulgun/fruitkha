using Dapper;
using Fruitkha.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;

namespace Fruitkha.Controllers
{
    public class CommentController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddComment(string id)
        {
            using var connection = new SqlConnection(connectionString);
            var comment = connection.QueryFirstOrDefault<StatusReport>("SELECT * FROM sales WHERE CommentBar = @CommentBar", new { CommentBar = id });
            return View(comment);
        }
        [HttpPost]
        public IActionResult AddComment(Comment model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }

            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO comments (CommentDetail, CustomerName, Email,Rating,ProductId) VALUES (@CommentDetail, @CustomerName, @Email,@Rating,@ProductId)";
            try
            {
                var data = new
                {
                    model.CommentDetail,
                    model.CustomerName,
                    model.Email,
                    model.Rating,
                    model.ProductId
                };
                var affectedRows = connection.Execute(sql, data);


                
                return RedirectToAction("Detail","Home", new { id = model.ProductId });
            }
            catch
            {
                return RedirectToAction("Index");

            }
        }
    }
}
