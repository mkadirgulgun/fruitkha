using Dapper;
using Fruitkha.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Reflection;

namespace Fruitkha.Controllers
{
    public class AdminController : Controller
    {
        
        public bool CheckLogin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Autharization")))
            {
                return false;
            }

            return true;
        }
        public IActionResult Index()
        {
            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);
            var products = connection.Query<Product>("SELECT products.*, CategoryName FROM products LEFT JOIN categories ON products.CategoryId = categories.Id").ToList();
            return View(products);
        }
        public IActionResult EditProduct(int id)
        {

            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);
            var product = connection.QuerySingleOrDefault<Product>("SELECT * FROM products WHERE Id = @Id", new { Id = id });

            var category = connection.Query<Product>("SELECT * FROM categories").ToList();
            ViewBag.Categories = category;
            return View(product);

        }
        [HttpPost]
        public IActionResult EditProduct(Product model)
        {
            using var connection = new SqlConnection(connectionString);
            var imageName = model.ImgUrl;
            if (model.Image != null)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
                using var stream = new FileStream(path, FileMode.Create);
                model.Image.CopyTo(stream);
            }
            var sql = "UPDATE products SET Name = @Name,Price = @Price, ImgUrl = @ImgUrl, Stock = @Stock, CategoryId = @CategoryId WHERE Id=@Id";

            //var parameters = new
            //{
            //    model.Name,
            //    model.Price,
            //    model.Stock,
            //    model.CategoryId,
            //    model.Id,
            //    ImgUrl = imageName
            //};

            var affectedRows = connection.Execute(sql, model);
            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }
        public IActionResult Delete(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM products WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Index");
        }
        public IActionResult AddProduct()
        {

            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);
            var category = connection.Query<Product>("SELECT * FROM categories").ToList();
            return View(category);

        }
        [HttpPost]
        public IActionResult AddProduct(Product model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);
            var menus = "INSERT INTO products (Name, Price, ImgUrl, Stock,CategoryId) VALUES (@Name, @Price, @ImgUrl, @Stock,@CategoryId)";

            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
            using var stream = new FileStream(path, FileMode.Create);
            model.Image.CopyTo(stream);
            model.ImgUrl = imageName;
            var data = new
            {
                model.Name,
                model.Price,
                model.Stock,
                model.ImgUrl,
                model.CategoryId
            };

            var rowsAffected = connection.Execute(menus, data);
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Ürün başarıyla eklendi.";
            return View("Message");
        }

        public IActionResult Categories()
        {
            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);
            var category = connection.Query<Product>("SELECT * FROM categories").ToList();

            return View(category);
        }
        public IActionResult AddCategory()
        {

            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            return View();

        }
        [HttpPost]
        public IActionResult AddCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO categories (CategoryName) VALUES (@CategoryName)";
            var data = new
            {
                model.CategoryName
            };
            var rowsAffected = connection.Execute(sql, data);
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Eklendi.";
            return View("Message");
        }
        public IActionResult DeleteCategory(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM categories WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Categories");
        }
        public IActionResult EditCategory(int id)
        {

            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);
            var category = connection.QuerySingleOrDefault<Category>("SELECT * FROM categories WHERE Id = @Id", new { Id = id });
            return View(category);

        }
        [HttpPost]
        public IActionResult EditCategory(Category model)
        {
            using var connection = new SqlConnection(connectionString);

            var sql = "UPDATE categories SET CategoryName = @CategoryName WHERE Id=@Id";

            var parameters = new
            {
                model.CategoryName,
                model.Id,
            };

            var affectedRows = connection.Execute(sql, parameters);
            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }
        public IActionResult Reports()
        {
            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);

            var userSql = connection.Query<UserReport>("SELECT sales.UserId,sales.Email,\r\nCOUNT(products.Id) AS ProductsNumber,\r\n SUM(products.Price) AS TotalAmount\r\nFROM sales \r\nLEFT JOIN products ON sales.ProductId = products.Id\r\nGROUP BY sales.UserId, sales.Email\r\nORDER BY sales.UserId").ToList();

            var productSql = connection.Query<ProductReport>("SELECT products.Id ,products.Name, products.Price,\r\nCOUNT(sales.ProductId) AS SalesQuantity,\r\nCOUNT(sales.ProductId) * products.Price AS TotalSales\r\nFROM products\r\nLEFT JOIN sales ON products.Id = sales.ProductId\r\nGROUP BY  products.Id, products.Name, products.Price\r\nORDER BY products.Id ASC").ToList();

            ViewBag.Products = productSql;
            return View(userSql);
        }

        public IActionResult Status()
        {
            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);

            var statusSql = connection.Query<StatusReport>("SELECT sales.*, products.Name, status.Status FROM sales LEFT JOIN products ON products.Id = sales.ProductId LEFT JOIN status ON status.Id = sales.StatusId").ToList();
            var status = connection.Query<StatusModel>("SELECT * FROM status").ToList();
            ViewBag.Status = status;
            return View(statusSql);
        }
        public IActionResult ChangeStatus(int id, int StatusId)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "UPDATE sales SET StatusId = @StatusId WHERE Id = @Id";
            var parameters = new
            {
                StatusId,
                Id = id,
            };

            var user = connection.QueryFirstOrDefault<StatusReport>("SELECT sales.*, products.Name,status.Status as StatusName FROM sales LEFT JOIN products ON products.Id = sales.ProductId LEFT JOIN status ON status.Id = sales.StatusId WHERE sales.Id = @Id", new { Id = id });
            var status = connection.QueryFirstOrDefault<StatusReport>("SELECT Status as StatusName FROM status WHERE Id = @StatusId", new { StatusId });
            var affectedRows = connection.Execute(sql, parameters);
            if (StatusId == 2 || StatusId == 4)
            {
                ViewBag.Subject = "Sipariş Durumunuz Güncellendi";
                ViewBag.Body = $" <h1>Merhaba {user.CustomerName},</h1>\r\n        <p>Siparişinizin durumu güncellenmiştir. Aşağıdaki bilgiler siparişinizle ilgilidir:</p>\r\n        <ul>\r\n            <li><span class=\"important\">Sipariş Numarası:</span> {user.Id}</li>\r\n            <li><span class=\"important\">Ürün Adı:</span> {user.Name}</li>\r\n            <li><span class=\"important\">Miktar:</span> {user.Quantity}</li>\r\n            <li><span class=\"important\">Yeni Durum:</span> {status.StatusName}</li>\r\n         <p>Siparişinizle ilgili daha fazla bilgiye ihtiyacınız olursa, lütfen bizimle iletişime geçin.</p>\r\n        <p>İyi günler dileriz.</p>\r\n            <p>Saygılarımızla,<br>";
            }
            else if (StatusId == 3)
            {
                var Key = Guid.NewGuid().ToString();

                var comments = "UPDATE sales SET CommentBar = @CommentBar WHERE Id=@Id";
                var data = new
                {
                    CommentBar = Key,
                    Id = id
                };
                var rowsAffected = connection.Execute(comments, data);
                ViewBag.Subject = "Sipariş Teslim Edildi";
                ViewBag.Body = $" <h1>Merhaba {user.CustomerName},</h1>\r\n        <p>Siparişiniz başarıyla teslim edilmiştir. Aşağıdaki bilgiler siparişinizle ilgilidir:</p>\r\n        <ul>\r\n            <li><span class=\"important\">Sipariş Numarası:</span> {user.Id}</li>\r\n            <li><span class=\"important\">Ürün Adı:</span> {user.Name}</li>\r\n            <li><span class=\"important\">Miktar:</span> {user.Quantity}</li>\r\n            <li><span class=\"important\">Durum:</span> Teslim Edildi</li>\r\n        </ul>\r\n        <p>Ürününüzü teslim aldıktan sonra, deneyiminizi bizimle paylaşmak için lütfen yorum yapma sayfamıza gidin.</p>\r\n        <a href=\"https://localhost:7231/Comment/AddComment/{Key}\" class=\"btn\">Yorum Yap</a>\r\n        <p>Siparişinizle ilgili daha fazla bilgiye ihtiyacınız olursa, lütfen bizimle iletişime geçin.</p>\r\n        <p>İyi günler dileriz.</p>\r\n            <p>Saygılarımızla,<br>";
            }
            SendMail(user);
            return RedirectToAction("Status", "Admin");
        }
        public IActionResult SendMail(StatusReport? model)
        {
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress("bildirim@fruitkha.com.tr", "Fruitkha"),
                //ReplyTo = new MailAddress("info@mkadirgulgun.com.tr", "Mehmet Kadir Gülgün"),
                Subject = ViewBag.Subject,
                Body = ViewBag.Body,
                IsBodyHtml = true,
            };
            mailMessage.ReplyToList.Add(model.Email);
            //mailMessage.To.Add("mkadirgulgun@gmail.com");
            mailMessage.To.Add(new MailAddress($"{model.Email}"));
            client.Send(mailMessage);
            return View();

        }

        public IActionResult CommentControl()
        {
            if (!CheckLogin())
            {

                return RedirectToAction("Login", "Login");
            }
            using var connection = new SqlConnection(connectionString);

            var sql = "SELECT comments.*, products.Name as ProductName FROM comments LEFT JOIN products ON products.Id = comments.ProductId";
            var comments = connection.Query<Comment>(sql).ToList();

            return View(comments);

        }
        public IActionResult DeleteComment(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM comments WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            
            return RedirectToAction("CommentControl");
        }
    }
}
