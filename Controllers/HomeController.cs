using LibraryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LibraryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }



        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Manager()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return RedirectToAction("Index", "Auth");
            }

            return View();
        }
        [HttpPost]
        public IActionResult SaveBooks([FromBody] List<Book> books)
        {
            foreach (var newBook in books)
            {
                var existingBook = _context.Books
                    .FirstOrDefault(b => b.BookId == newBook.BookId);

                if (existingBook != null)
                {
                    // 🔥 UPDATE QUANTITY
                    existingBook.Quantity += newBook.Quantity;
                }
                else
                {
                    // 🆕 NEW BOOK ADD
                    _context.Books.Add(newBook);
                }
            }

            _context.SaveChanges();

            return Json(new { message = "Books processed successfully ✅" });
        }
        public IActionResult Records()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Auth");

            return View();
        }
        [HttpGet]
        public IActionResult GetRecords()
        {
            var books = _context.Books.ToList();
            var issued = (from i in _context.IssuedBooks
                          join s in _context.Students on i.StudentId equals s.Id
                          join b in _context.Books on i.BookId equals b.BookId
                          select new
                          {
                              id = i.Id,                 // 🔥 ADD THIS
                              student = s.Name,
                              book = b.Name,
                              issueDate = i.IssueDate,
                              returnDate = i.ReturnDate,
                              fine = (DateTime.Now > i.ReturnDate)
                                     ? (DateTime.Now - i.ReturnDate).Days * 10
                                     : 0,
                              status = i.Status         // 🔥 ADD THIS
                          }).ToList();

            return Json(new { books, issued });
        }
        public IActionResult Issue()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Student" && role != "Admin")
            {
                return RedirectToAction("Index", "Auth");
            }

            return View();
        }
        [HttpPost]
        public IActionResult IssueBook([FromBody] IssueRequest data)
        {
            try
            {
                string studentName = data.StudentName.Trim();
                string bookName = data.BookName.Trim();

                string email = data.Email.Trim();
                DateTime returnDate = data.ReturnDate;
                

                // Book check
                var book = _context.Books
                    .FirstOrDefault(b => b.Name.ToLower() == bookName.ToLower());

                if (book == null)
                    return Json(new { message = "Book not found ❌" });

                if (book.Quantity <= 0)
                    return Json(new { message = "Out of stock ❌" });

                // Student check
                var student = _context.Students
                    .FirstOrDefault(s => s.Name.ToLower() == studentName.ToLower());

                if (student == null)
                {
                    // 🔥 NEW STUDENT → SAVE NAME + EMAIL
                    student = new Student
                    {
                        Name = studentName,
                        Email = email
                    };

                    _context.Students.Add(student);
                    _context.SaveChanges(); // ID generate hone ke liye
                }
                else
                {
                    // 🔥 EXISTING STUDENT → EMAIL UPDATE (agar missing hai)
                    if (string.IsNullOrEmpty(student.Email) && !string.IsNullOrEmpty(email))
                    {
                        student.Email = email;
                        _context.SaveChanges();
                    }
                }

                

                // Issue record (only request, not actual issue)
                var issue = new IssuedBook
                {
                    StudentId = student.Id,
                    BookId = book.BookId,
                    IssueDate = DateTime.Now,
                    ReturnDate = returnDate,
                    Status = "Pending"   // 🔥 IMPORTANT
                };

                _context.IssuedBooks.Add(issue);

                // ❌ REMOVE THIS LINE
                // book.Quantity -= 1;

                _context.SaveChanges();

                return Json(new { message = "Request sent to librarian ⏳" });
            }
            catch (Exception ex)
            {
                return Json(new { message = "Error ❌", error = ex.Message });
            }
        }
        public IActionResult Return()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Index", "Auth");

            return View();
        }
        [HttpPost]
        public IActionResult CheckFine(string studentName, string bookName, DateTime returnDate)
        {
            var record = (from i in _context.IssuedBooks
                          join s in _context.Students on i.StudentId equals s.Id
                          join b in _context.Books on i.BookId equals b.BookId
                          where s.Name == studentName && b.Name == bookName
                          select new
                          {
                              i.ReturnDate
                          }).FirstOrDefault();

            if (record == null)
            {
                return Json(new { fine = 0, message = "Record not found" });
            }

            int fine = 0;

            if (returnDate > record.ReturnDate)
            {
                fine = (returnDate - record.ReturnDate).Days * 10;
            }

            return Json(new { fine });
        }
        [HttpPost]
        public IActionResult ReturnBook(string studentName, string bookName)
        {
            var record = (from i in _context.IssuedBooks
                          join s in _context.Students on i.StudentId equals s.Id
                          join b in _context.Books on i.BookId equals b.BookId
                          where s.Name.ToLower() == studentName.ToLower()
                             && b.Name.ToLower() == bookName.ToLower()
                          select new
                          {
                              Issued = i,
                              Book = b
                          }).FirstOrDefault();

            if (record == null)
            {
                return Json(new { success = false, message = "Record not found" });
            }

            // Quantity +1
            record.Book.Quantity += 1;

            // Remove issued record
            _context.IssuedBooks.Remove(record.Issued);

            _context.SaveChanges();

            return Json(new { success = true, message = "Book Returned Successfully" });
        }
        [HttpGet]
        public IActionResult GetDashboardData()
        {
            var totalBooks = _context.Books.Count();
            var issuedBooks = _context.IssuedBooks
                             .Where(i => i.Status == "Approved")
                             .Count();
            var totalStudents = _context.Students.Count();
            var pendingRequests = _context.IssuedBooks
           .Where(i => i.Status == "Pending")
           .Count();

            return Json(new
            {
                totalBooks,
                issuedBooks,
                totalStudents,
                pendingRequests
            });
        }
        [HttpPost]
        public IActionResult SendReminderEmail(string student, string book, string returnDate)
        {
            try
            {
                var studentData = _context.Students
                    .FirstOrDefault(s => s.Name.ToLower() == student.ToLower());

                if (studentData == null || string.IsNullOrEmpty(studentData.Email))
                {
                    return Json(new { success = false, message = "Student email not found" });
                }

                string toEmail = studentData.Email;

                string subject = "📚 Library Book Return Reminder";

                DateTime parsedDate = DateTime.Parse(returnDate);
                int daysRemaining = (parsedDate - DateTime.Now).Days;

                string body = $@"
<div style='font-family: Arial; background:#0f172a; padding:20px; color:white;'>

    <h2 style='text-align:center;'>📚 Library Management System</h2>

    <div style='background:#1e293b; padding:20px; border-radius:10px;'>

        <h3 style='color:#38bdf8;'>📅 Book Return Reminder</h3>

        <p>Dear <b>{student}</b>,</p>

        <p>This is a friendly reminder that your issued book is due for return:</p>

        <table style='width:100%; border-collapse:collapse; margin-top:15px;'>

            <tr style='background:#111827;'>
                <td style='padding:12px;'>📖 Book Name</td>
                <td style='padding:12px;'><b>{book}</b></td>
            </tr>

            <tr style='background:#020617;'>
                <td style='padding:12px;'>📅 Due Date</td>
                <td style='padding:12px; color:#22d3ee;'>
                    <b>{parsedDate:dd MMM yyyy}</b>
                </td>
            </tr>

            <tr style='background:#064e3b;'>
                <td style='padding:12px;'>⏳ Days Remaining</td>
                <td style='padding:12px; color:#4ade80;'>
                    <b>{daysRemaining} day(s)</b>
                </td>
            </tr>

        </table>

        <div style='margin-top:15px; background:#78350f; padding:12px; border-radius:8px;'>
            ⚠️ Note: Please return the book on time to avoid a fine of 
            <b>₹10 per day</b> after the due date.
        </div>

        <p style='margin-top:20px;'>Thank you,<br/><b>Library Team</b></p>

    </div>

</div>
";

                var smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new System.Net.NetworkCredential(
                        "sardarbabbar2022@gmail.com",
                        "klosgvseginnkbcy"
                    ),
                    EnableSsl = true
                };

                var mail = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress("sardarbabbar2022@gmail.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true // 🔥 MOST IMPORTANT
                };

                mail.To.Add(toEmail);

                smtp.Send(mail);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult DeleteBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == id);

            if (book == null)
                return Json(new { success = false, message = "Book not found" });

            _context.Books.Remove(book);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult UpdateBook([FromBody] Book updatedBook)
        {
            var book = _context.Books.FirstOrDefault(b => b.BookId == updatedBook.BookId);

            if (book == null)
                return Json(new { message = "Book not found ❌" });

            book.Name = updatedBook.Name;
            book.Author = updatedBook.Author;
            book.Quantity = updatedBook.Quantity;

            _context.SaveChanges();

            return Json(new { message = "Book updated successfully ✅" });
        }
        [HttpPost]
        public IActionResult ApproveRequest(int id)
        {
            var issue = _context.IssuedBooks.FirstOrDefault(i => i.Id == id);

            if (issue == null)
                return Json(new { message = "Request not found ❌" });

            if (issue.Status != "Pending")
                return Json(new { message = "Already processed ⚠️" });

            var book = _context.Books.FirstOrDefault(b => b.BookId == issue.BookId);

            if (book == null || book.Quantity <= 0)
                return Json(new { message = "Book not available ❌" });

            // 🔥 STATUS UPDATE
            issue.Status = "Approved";

            // 🔥 QUANTITY DECREASE (yahi actual issue hai)
            book.Quantity -= 1;

            _context.SaveChanges();

            return Json(new { message = "Book Approved ✅" });
        }
        [HttpPost]
        public IActionResult DenyRequest(int id)
        {
            var issue = _context.IssuedBooks.FirstOrDefault(i => i.Id == id);

            if (issue == null)
                return Json(new { message = "Request not found ❌" });

            if (issue.Status != "Pending")
                return Json(new { message = "Already processed ⚠️" });

            // 🔥 STATUS UPDATE
            issue.Status = "Denied";

            _context.SaveChanges();

            return Json(new { message = "Request Denied ❌" });
        }
        public IActionResult StudentHistory()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
                return RedirectToAction("Index", "Auth");

            var history = _context.IssuedBooks
                .Where(i => i.StudentId == studentId)
                .Join(_context.Books,
                      i => i.BookId,
                      b => b.BookId,
                      (i, b) => new
                      {
                          Book = b.Name,
                          IssueDate = i.IssueDate,
                          ReturnDate = i.ReturnDate,
                          Status = i.Status
                      })
                .ToList();

            return View(history);
        }
        [HttpGet]
        public IActionResult GetStudentHistory()
        {
            int? studentId = HttpContext.Session.GetInt32("StudentId");

            if (studentId == null)
                return Json(new { success = false, message = "Not logged in" });

            var history = _context.IssuedBooks
                .Include(i => i.Book)   // 🔥 Important (Book join)
                .Where(i => i.StudentId == studentId)
                .Select(i => new
                {
                    book = i.Book.Name,
                    issueDate = i.IssueDate,
                    returnDate = i.ReturnDate,
                    status = i.Status,
                    fine = i.ReturnDate < DateTime.Now
                        ? (DateTime.Now - i.ReturnDate).Days * 10
                        : 0
                })
                .ToList();

            return Json(history);
        }
        public IActionResult AvailableBooks()
        {
            // Student aur Admin dono dekh sakte hain
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Student")
                return RedirectToAction("Index", "Auth");

            return View();
        }
        public IActionResult IssuedBooks()
        {
            // Sirf Admin
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return RedirectToAction("Index", "Auth");

            return View();
        }
        public IActionResult Profile()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Student")
                return RedirectToAction("Index", "Auth");

            int? studentId = HttpContext.Session.GetInt32("StudentId");
            var student = _context.Students.FirstOrDefault(s => s.Id == studentId);

            if (student == null)
                return RedirectToAction("Index", "Auth");

            var issuedBooks = _context.IssuedBooks
                .Where(i => i.StudentId == studentId)
                .Join(_context.Books,
                      i => i.BookId,
                      b => b.BookId,
                      (i, b) => new {
                          Book = b.Name,
                          IssueDate = i.IssueDate,
                          ReturnDate = i.ReturnDate,
                          Status = i.Status,
                          Fine = i.ReturnDate < DateTime.Now && i.Status != "Returned"
                                       ? (DateTime.Now - i.ReturnDate).Days * 10 : 0
                      })
                .ToList();

            ViewBag.Student = student;
            ViewBag.TotalBooks = issuedBooks.Count;
            ViewBag.Pending = issuedBooks.Count(x => x.Status == "Pending");
            ViewBag.Approved = issuedBooks.Count(x => x.Status == "Approved");
            ViewBag.TotalFine = issuedBooks.Sum(x => x.Fine);

            return View(issuedBooks);
        }
    }
}
