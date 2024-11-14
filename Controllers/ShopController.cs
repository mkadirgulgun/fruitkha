using Dapper;
using Fruitkha.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace Fruitkha.Controllers
{
    public class ShopController : Controller
    {
        

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddtoCart(int id, int Quantity)
        {
            using var connection = new SqlConnection(connectionString);
            var product = connection.QueryFirstOrDefault<Product>("SELECT * FROM products WHERE Id = @Id", new { Id = id });

            if (product == null)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Böyle bir ürün yok.";
                return View("Message");
            }else if(product.Stock < Quantity)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Bu ürünün stoğu yok.. ";
                return View("Message");
            }
            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            var userId = ViewData["UserId"];

            var existingItem = connection.QueryFirstOrDefault<Cart>("SELECT * FROM Cart WHERE ProductId = @Id AND UserId = @userId", new { Id = id, userId });
            if (existingItem != null)
            {
                existingItem.Quantity += Quantity;
                var sqlUpdate = "UPDATE Cart SET Quantity = @Quantity WHERE ProductId = @ProductId AND UserId = @userId";
                var data = new
                {
                    existingItem.Quantity,
                    ProductId = id,
                    userId
                };
                connection.Execute(sqlUpdate, data);
            }
            else
            {

                var sqlInsert = "INSERT INTO Cart (ProductId, ProductName, ProductPrice, Quantity, ProductUrl,UserId) VALUES (@ProductId, @ProductName, @ProductPrice, @Quantity, @ProductUrl,@userId)";
                var data = new
                {
                    ProductId = id,
                    ProductName = product.Name,
                    ProductPrice = product.Price,
                    ProductUrl = product.ImgUrl,
                    Quantity,
                    userId
                };
                connection.Execute(sqlInsert, data);
            }

            //product.Stock -= Quantity;
            //var sqlStock = "UPDATE products SET Stock = @Stock WHERE Id = @Id";
            //connection.Execute(sqlStock, new { Id = id, product.Stock });
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Ürün başarıyla eklendi.";
            ViewData["Location"] = HttpContext.Session.GetString("Location");
            return RedirectToAction(ViewData["Location"].ToString(), "Home");
        }

        [HttpPost]
        public IActionResult UpdateCart(CartItem model)
        {
            using var connection = new SqlConnection(connectionString);

            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            var userId = ViewData["UserId"];

            foreach (var item in model.CartUpdate)
            {
                if (item.Quantity > 0)
                {
                    var productSql = "SELECT * FROM products WHERE Id = @ProductId";
                    var product = connection.QuerySingleOrDefault<Product>(productSql, new { item.ProductId });

                    if (product.Stock < item.Quantity)
                    {
                        ViewBag.MessageCssClass = "alert-danger";
                        ViewBag.Message = $"'{product.Name}' ürününden stok fazlası girdiniz. Mevcut stok: {product.Stock}.";
                        return RedirectToAction("Cart", "Home");
                    }
                }
            }

            connection.Execute("DELETE FROM Cart WHERE UserId = @userId", new { userId });

            foreach (var item in model.CartUpdate)
            {
                if (item.Quantity > 0)
                {
                    var productSql = "SELECT * FROM products WHERE Id = @ProductId";
                    var product = connection.QuerySingleOrDefault<Product>(productSql, new { item.ProductId });

                    var sqlInsert = "INSERT INTO Cart (ProductName, ProductPrice, ProductUrl, ProductId, Quantity, UserId) VALUES (@ProductName, @ProductPrice, @ProductUrl, @ProductId, @Quantity, @userId)";
                    var data = new
                    {
                        ProductName = product.Name,
                        ProductPrice = product.Price,
                        ProductUrl = product.ImgUrl,
                        item.ProductId,
                        item.Quantity,
                        userId
                    };
                    connection.Execute(sqlInsert, data);
                }
            }

            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Sepet başarıyla güncellendi.";
            return RedirectToAction("Cart", "Home");
        }

        public IActionResult TakePayment(User model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);
            ViewData["email"] = HttpContext.Session.GetString("email");
            var login = connection.QueryFirstOrDefault<Login>("SELECT * FROM users WHERE Email = @Email", new { model.Email });
            if (ViewData["email"] == null)
            {
                if (login?.Email == model.Email)
                {
                    ViewData["Message"] = $"Bu mail kayıtlı. Lutfen giris <a href=\"/login/login\" > yapin <a/>";
                    return RedirectToAction("Checkout", "Home", model);
                }

            }

            ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            var userId = ViewData["UserId"];
            var client = new SmtpClient("smtp.eu.mailgun.org", 587)
            {
                Credentials = new NetworkCredential("postmaster@bildirim.mkadirgulgun.com.tr", "cb5edda1ad0913ef5144e9fc0f8484a2-fe9cf0a8-3d53c1ae"),
                EnableSsl = true
            };

            var sale = "SELECT cart.*, products.Stock FROM cart LEFT JOIN products ON products.Id = cart.ProductId WHERE UserId = @userId ";
            var saleSql = connection.Query<Cart>(sale, new { userId }).ToList();

            
            foreach (var item in saleSql)
            {
                var sqlInsert = "INSERT INTO sales (UserId,ProductId,Email,CustomerName,CustomerAddress,CustomerCity,CustomerZipCode,Quantity) VALUES (@UserId,@ProductId,@Email,@CustomerName,@CustomerAddress,@CustomerCity,@CustomerZipCode,@Quantity)";
                var data = new
                {
                    item.ProductId,
                    item.UserId,
                    model.Email,
                    CustomerName = model.Name,
                    CustomerAddress = model.Address,
                    CustomerCity = model.City,
                    CustomerZipCode = model.ZipCode,
                    item.Quantity
                };
                connection.Execute(sqlInsert, data);
            }
            var subTotalSql = "SELECT SUM(ProductPrice * Quantity) AS Subtotal FROM cart WHERE UserId = @userId";
            var subTotal = connection.QueryFirstOrDefault<Cart>(subTotalSql, new { userId });
            ViewBag.SubTotal = subTotal.Subtotal;

            ViewBag.Subject = "Siparişiniz Başarıyla Alındı";
            ViewBag.Body = $"<p>Merhaba <strong>{model.Name}</strong>,</p>\r\n        <p>Fruitkha'dan alışveriş yaptığınız için teşekkür ederiz! Siparişiniz başarıyla alındı ve en kısa sürede işleme konulacaktır.</p>\r\n        \r\n        <h3>Sipariş Detayları:</h3>\r\n        <ul>\r\n<li><strong>Toplam Tutar:</strong> {ViewBag.SubTotal} TL</li>\r\n        </ul>\r\n\r\n        <h3>Teslimat Bilgileri:</h3>\r\n        <ul>\r\n            <li><strong>Alıcı Adı:</strong> {model.Name}</li>\r\n            <li><strong>Teslimat Adresi:</strong> {model.Address}</li>\r\n <p>Siparişinizin durumu hakkında sizi bilgilendirmek için e-posta göndermeye devam edeceğiz. Siparişiniz gönderildiğinde, takip numarası ve tahmini teslimat süresi hakkında bilgi alacaksınız.</p>\r\n\r\n            <p>Tekrar teşekkür ederiz ve siparişinizin keyfini çıkarmanızı dileriz!</p>\r\n\r\n            <p>Saygılarımızla,<br>Fruitkha Ekibi</p>";
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Başarıyla kayıt olundu. Onaylamak için mail kutunuza gidin";
            SendMail(model);
            connection.Execute("DELETE FROM Cart Where UserId =@userId", new { userId });
            return View("ThankYou");

        }
        public IActionResult SendMail(User model)
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
    }
}
