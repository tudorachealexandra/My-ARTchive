using MyApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyApplication.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static Random random = new Random();

        // GET: Category
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories;
            return View();
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id);

            var products = from prod in db.Products.Include("Category").Include("User")
                           where prod.CategoryId == id && prod.ProductApprove == true
                           select prod;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.Products = products;

            return View(category);
        }

        public ActionResult New()
        {
            Category category = new Category();
            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult New(Category cat)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    if (cat.Image != null)
                    {
                        var filename = string.Concat(RandomString(10), cat.Image.FileName);
                        var path = string.Concat("~/CategoryImages/", filename);
                        cat.Image.SaveAs(Server.MapPath("~/CategoryImages/") + filename);
                        cat.ImagePath = path;
                    }
                    db.Categories.Add(cat);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(cat);
                }
            }
            catch (Exception e)
            {
                return View(cat);
            }
        }

        [NonAction]
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

      
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            ViewBag.Category = category;
           
            if (User.IsInRole("Administrator"))
            {
                return View(category);
            }
            else
            {
                TempData["message"] = "You don't have the right to modify this!";
                return RedirectToAction("Index", "Category", new { id });
            }

        }


        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int id,Category requestCategory)
        {
            try
            {
                    if (ModelState.IsValid)
                    {
                        Category category = db.Categories.Find(id);
                    if (User.IsInRole("Administrator"))
                    { 
                        if (TryUpdateModel(category))
                        {
                            if (requestCategory.Image != null)
                            {
                                var filename = string.Concat(RandomString(10), requestCategory.Image.FileName);
                                var path = string.Concat("~/CategoryImages/", filename);
                                requestCategory.Image.SaveAs(Server.MapPath("~/CategoryImages/") + filename);
                                requestCategory.ImagePath = path;
                                category.ImagePath = requestCategory.ImagePath;
                            }

                            category.CategoryName = requestCategory.CategoryName;
                            category.Description = requestCategory.Description;
                            category.Image = requestCategory.Image;
                            db.SaveChanges();
                        }
                        return RedirectToAction("Show", "Category", new { id });
                    }
                    else
                    {
                        TempData["message"] = "You don't have the right to modify this!";
                        return RedirectToAction("Show", "Category", new { id });
                    }
                }
                else
                {
                    return View(requestCategory);
                }

            }
            catch (Exception e)
            {
                return View(requestCategory);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}