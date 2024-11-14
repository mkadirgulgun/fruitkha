using Dapper;
using Fruitkha.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;

namespace Fruitkha.Controllers
{
	public class LoginController : Controller
	{
		
		public IActionResult Index()
		{

			return View();
		}
		public IActionResult Login()
		{
			ViewData["email"] = HttpContext.Session.GetString("email");
			ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");
            return View(new Login());
		}
		[HttpPost]
		public IActionResult Login(Login model)
		{
			if (model.Email == null || model.Password == null)
			{
				ViewData["Error"] = "Form eksik veya hatalı!";
				return View("Login", "Login");
			}
			model.Password = Helper.Hash(model.Password);
			using var connection = new SqlConnection(connectionString);
			var login = connection.Query<Login>("SELECT * FROM Users").ToList();

			foreach (var user in login)
			{
				if (user.Email == model.Email && user.Password == model.Password )
				{
					if(user.Autharization == 1)
					{
                        HttpContext.Session.SetInt32("Autharization", 1);
                    }
                    ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
                    var userId = ViewData["UserId"];
                    ViewData["Msg"] = "Giriş Başarılı";
					HttpContext.Session.SetString("email", user.Email);
					HttpContext.Session.SetInt32("UserId", user.Id);
                    ViewData["UserId"] = HttpContext.Session.GetInt32("UserId");
                    var newId = ViewData["UserId"];
                    var sql = "UPDATE cart SET UserId = @newId WHERE UserId=@userId";

                    var parameters = new
                    {
                        newId,
                        userId
                    };

                    var affectedRows = connection.Execute(sql, parameters);



                    ViewBag.IdUser = user.Id;
					return RedirectToAction("Index", "Home");

				}
				else
				{
					ViewData["Msg"] = "Kullanıcı adı veya şifre yanlış";
				}

			}
			return View("Login", model);
		}

		public IActionResult Exit()
		{
			HttpContext.Session.Clear();
			return RedirectToAction("Login", "Login");
		}
		public IActionResult SignUp()
		{
            ViewData["Autharization"] = HttpContext.Session.GetInt32("Autharization");
            return View();
		}
		[HttpPost]
		public IActionResult SignUp(Login model)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.MessageCssClass = "alert-danger";
				ViewBag.Message = "Eksik veya hatalı işlem yaptın";
				return View("Msg");
			}
			using var connection = new SqlConnection(connectionString);
			var login = connection.QueryFirstOrDefault<Login>("SELECT * FROM Users WHERE Email = @Email", new { model.Email});

			if (model.Password != model.PasswordRepeat)
			{
				ViewData["Message"] = "Sifreler uyusmuyor";
				return View("Index", model);
			}
			else if (login?.Email == model.Email)
			{
				ViewData["Message"] = "Bu mail kayıtlı";
				return View("Index", model);
			}
			
			else
			{
				model.Password = Helper.Hash(model.Password);

				var client = new SmtpClient("smtp.eu.mailgun.org", 587)
				{
					Credentials = new NetworkCredential("postmaster@bildirim.mkadirgulgun.com.tr", "cb5edda1ad0913ef5144e9fc0f8484a2-fe9cf0a8-3d53c1ae"),
					EnableSsl = true
				};
				

				var signup = "INSERT INTO users (Name, Password, Email) VALUES (@Name, @Password, @Email)";

				var data = new
				{
					model.Name,
					model.Password,
					model.Email,
				};

				var rowsAffected = connection.Execute(signup, data);

				ViewBag.Subject = "Hoş Geldiniz! Kayıt İşleminiz Başarıyla Tamamlandı";
				ViewBag.Body = $"<h1>Hoş Geldiniz, {model.Name}!</h1>\r\n            <p>Web sitemize kayıt olduğunuz için teşekkür ederiz. Kayıt işleminiz başarıyla tamamlandı.</p>\r\n            <p>Aşağıdaki bilgileri gözden geçirebilirsiniz:</p>\r\n            <ul>\r\n                <li><strong>Kullanıcı Adı:</strong> </li>\r\n                <li><strong>E-posta:</strong> {model.Email}</li>\r\n            </ul>\r\n            <p>Hesabınızı doğrulamak ve hizmetlerimizden yararlanmaya başlamak için <a href=>buraya tıklayın</a>.</p>\r\n            <p>İyi günler dileriz!</p>";
				ViewBag.MessageCssClass = "alert-success";
				ViewBag.Message = "Başarıyla kayıt olundu. Onaylamak için mail kutunuza gidin";
				SendMail(model);
				return View("Message");
			}
		}
		public IActionResult SendMail(Login model)
		{
			
			var mailMessage = new MailMessage
			{
				From = new MailAddress("bildirim@fruitkha.com.tr", "Fruitkha.com"),
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
