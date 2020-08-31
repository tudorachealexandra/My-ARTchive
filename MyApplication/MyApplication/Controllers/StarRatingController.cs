using Microsoft.AspNet.Identity;
using MyApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyApplication.Controllers
{
    [Authorize]
    public class StarRatingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: StarRating
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(string id)
        {
            ApplicationUser Receiver = db.Users.Find(id);
            string ReceiverId = Receiver.Id;
            string GiverId = User.Identity.GetUserId();
            StarRating rating = new StarRating();
            rating.ReceiverId = ReceiverId;
            ViewBag.ReceiverName = Receiver.UserName;
            rating.GiverId = GiverId;
            rating.Values = GetAllValues();
            rating.StarRatingId = CountRatings().ToString();
            return View(rating);
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult New(StarRating rating)
        {
            rating.Values = GetAllValues();
            try
            {
                if (ModelState.IsValid)
                {

                    db.StarRatings.Add(rating);
                    db.SaveChanges();

                    return RedirectToAction("OnPressUser", "Users", new { id = rating.ReceiverId });
                }
                else
                {
                    TempData["Message"] = "Failed to create new rating";
                    return View(rating);
                }
            }
            catch (Exception e)
            {
                return View(rating);
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllValues()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // iteram prin categorii
            for (var i = 1; i<=5;i++)
            {
                // Adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = i.ToString()
                });
            }
            // returnam lista de categorii
            return selectList;
        }

        [NonAction]
        public int CountRatings()
        {
            var count = 0;
            foreach(StarRating sr in db.StarRatings)
            {
                count++;
            }
            return count;
        }

        [NonAction]
        public double CalculateRating(string id)
        {
            var result = 0.0;
            var total = 0.0;
            List<float> srList = new List<float>();
            var starRatings = from rev in db.StarRatings
                              where (rev.ReceiverId == id)
                              select rev;
            foreach (var rev in starRatings)
            {
                var r = float.Parse(rev.selectedValue);
                srList.Add(r);
            }

            foreach(var r in srList)
            {
                total = total + r;
                
            }
            result = total / srList.Count;
            return result;
        }
        
    }
}