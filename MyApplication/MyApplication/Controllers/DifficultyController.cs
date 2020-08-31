using MyApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyApplication.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DifficultyController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Category
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var difficulties = from difficulty in db.Difficulties
                               orderby difficulty.DifficultyName
                               select difficulty;
            ViewBag.Difficulties = difficulties;
            return View();
        }

        public ActionResult Show(int id)
        {
            Difficulty difficulty = db.Difficulties.Find(id);
            var jobs = from j in db.Jobs.Include("Difficulty").Include("User")
                           where j.DifficultyId == id
                           select j;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.Jobs = jobs;
            return View(difficulty);
        }
  
    }
}