using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pustok.DAL;
using Pustok.Models;
using Pustok.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustok.Controllers
{
    public class BookDetailController : Controller
    {
        private readonly AppDbContext _context;

        public BookDetailController(AppDbContext context)
        {
            this._context = context;
        }

        public IActionResult Detail(int Id) 
        {

            BookDetailViewModel bookVM = GetBookDetailVM(Id);
            if (bookVM == null)
            {
                return Content("This book is not exits");
            }
            return View(bookVM);

        }

        public IActionResult GetBookModal(int id)
        {
            Books book = _context.Book.Include(x => x.Author).Include(x => x.Genre).Include(x => x.BookImages).FirstOrDefault(x=>x.Id == id);
            if (book == null)
            {
                return NotFound();
            }
            return PartialView("_BookModalPartial",book);
        }
        private BookDetailViewModel GetBookDetailVM(int Id)
        {
            Books book = _context.Book
                .Include(x => x.BookImages)
                .Include(x => x.Genre)
                .Include(x => x.BookComments).ThenInclude(bc => bc.User)
                .Include(x => x.Author)
                .Include(x => x.BookTags).ThenInclude(x => x.Tags)
                .FirstOrDefault(x => x.Id == Id);
            if (book ==null)
            {
                return null;
            }
            BookDetailViewModel bookVM = new BookDetailViewModel
            {
                Book = book,
                RelatedBooks = _context.Book
               .Include(x => x.Author)
               .Include(x => x.BookImages)
               .Where(x => x.GenreId == book.GenreId).Take(6).ToList(),
                BookComment = new BookCommentViewModel { BookId = Id },

            };
            return bookVM;
        }
        [HttpPost]
        public async Task<IActionResult> Comment(BookCommentViewModel commentVM)
        {
            if (!ModelState.IsValid)
            {
                var model = GetBookDetailVM(commentVM.BookId);
                if (model == null)  
                {
                    return Content("This book is not exists");
                }
                else
                {
                    model.BookComment = commentVM;
                    return View("detail", model);
                }
            }
            Books book = _context.Book.Include(x => x.BookComments).FirstOrDefault(x => x.Id == commentVM.BookId);
            if (book == null)
            {
                return Content("This book is not exists");
            }
            AppUser user = await _context.Users.FirstOrDefaultAsync(x => !x.IsAdmin && x.NormalizedUserName == User.Identity.Name.ToUpper());
            BookComment comment = new BookComment
            {
                BookId = commentVM.BookId,
                AppUserId = user.Id,
                Rate = commentVM.Rate,
                CreatedAt = DateTime.UtcNow.AddHours(4),
                Text = commentVM.Text
            };
            book.BookComments.Add(comment);
            book.Rate = (byte)Math.Round(book.BookComments.Average(x => x.Rate));
            await _context.SaveChangesAsync();
            return RedirectToAction("detail", new { Id = commentVM.BookId });
        }
       /* public IActionResult SetSession()
        {
            HttpContext.Session.SetString("name", "dilqem");
            return Content("");

        }
        public IActionResult GetSession()
        {
            var value = HttpContext.Session.GetString("name");
            return Content(value);
        }

        public IActionResult SetCookie()
        {
            HttpContext.Response.Cookies.Append("name","dilqem");
            return Content("");
        }
        public IActionResult GetCookie()
        {
            var value = HttpContext.Request.Cookies["name"];
            return Content(value);
        }*/
        public IActionResult AddToBasket(int Id)
        {
            List<BasketItemViewModel> basketItems = null;
            BasketItemViewModel basketItem = null;
            string basketJson = Request.Cookies["Basket"];
            if (basketJson == null)
            {
                basketItems = new List<BasketItemViewModel>();
            }
            else
            {
                basketItems = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketJson);

                basketItem = basketItems.FirstOrDefault(x=>x.BookId == Id);
               
            }
            if (basketItem == null)
            {
                basketItem = new BasketItemViewModel
                {
                    BookId = Id,
                    Count = 1,
                };
                basketItems.Add(basketItem);
            }
            else
            {
                basketItem.Count++;
            }

            string newBasketJson = JsonConvert.SerializeObject(basketItems);
            Response.Cookies.Append("Basket",newBasketJson);
            return RedirectToAction("index", "home");
        }

        public IActionResult ShowBasket()
        {
            List<BasketItemViewModel> ids = null;
            string basketJson = Request.Cookies["Basket"];
            if (basketJson == null)
            {
                ids = new List<BasketItemViewModel>();
            }
            else
            {
                ids = JsonConvert.DeserializeObject<List<BasketItemViewModel>>(basketJson);
            }
            return Ok(ids);
        }



    }
}
