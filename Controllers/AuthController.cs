using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Net.Mail;


namespace LibraryManagement.Controllers
{
    public class AuthController : Controller
    {
        private static string generatedOtp = "";
        private static string tempName = "";
        private static string tempEmail = "";
        private static string tempPassword = "";

        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AdminLogin()
        {
            return View();
        }
        private bool IsValidPassword(string password)
        {
            return password.Any(char.IsUpper) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        [HttpPost]
        public IActionResult AdminLogin(string userId, string password)
        {
            if (userId == "admin" && password == "admin@123")
            {
                HttpContext.Session.SetString("Role", "Admin");
                return RedirectToAction("Manager", "Home");
            }

            ViewBag.Error = "Invalid Credentials";
            return View();
        }
        public IActionResult StudentAuth()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string name, string email, string password)
        {
            if (!IsValidPassword(password))
            {
                ViewBag.Error = "Password must have 1 capital, 1 number, 1 special character";
                return View("StudentAuth");
            }

            tempName = name;
            tempEmail = email;
            tempPassword = password;

            Random rnd = new Random();
            generatedOtp = rnd.Next(100000, 999999).ToString();

            SendOtpEmail(email, generatedOtp); // 🔥 yeh line

            return RedirectToAction("VerifyOtp");
        }
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyOtp(string otp)
        {
            if (otp == generatedOtp)
            {
                var student = new Student
                {
                    Name = tempName,
                    Email = tempEmail,
                    Password = tempPassword
                };

                _context.Students.Add(student);
                _context.SaveChanges();

                return RedirectToAction("Issue", "Home");
            }

            ViewBag.Error = "Invalid OTP";
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var student = _context.Students
                .FirstOrDefault(s => s.Email == email && s.Password == password);

            if (student != null)
            {
                HttpContext.Session.SetString("Role", "Student");
                HttpContext.Session.SetInt32("StudentId", student.Id);
                HttpContext.Session.SetString("StudentName", student.Name);
                HttpContext.Session.SetString("StudentEmail", student.Email ?? "");
                return RedirectToAction("Issue", "Home");
            }

            ViewBag.Error = "Invalid login";
            return View("StudentAuth");
        }
        private void SendOtpEmail(string toEmail, string otp)
        {
            var fromEmail = "sardarbabbar2022@gmail.com";
            var password = "klosgvseginnkbcy";

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Your OTP Code",
                Body = "Your OTP is: " + otp
            };

            smtp.Send(message);
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SendResetOtp(string email)
        {
            var student = _context.Students.FirstOrDefault(s => s.Email == email);

            if (student == null)
            {
                ViewBag.Error = "Email not found";
                return View("ForgotPassword");
            }

            tempEmail = email;

            Random rnd = new Random();
            generatedOtp = rnd.Next(100000, 999999).ToString();

            SendOtpEmail(email, generatedOtp);

            return RedirectToAction("ResetPassword");
        }
        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string otp, string newPassword)
        {
            if (otp != generatedOtp)
            {
                ViewBag.Error = "Invalid OTP";
                return View();
            }

            var student = _context.Students.FirstOrDefault(s => s.Email == tempEmail);

            if (student != null)
            {
                student.Password = newPassword;
                _context.SaveChanges();
            }

            return RedirectToAction("StudentAuth");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // 🔥 pura session clear
            return RedirectToAction("Index", "Auth");
        }


    }
}