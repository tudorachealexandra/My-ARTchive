using Microsoft.AspNet.Identity;
using MyApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyApplication.Controllers
{
    [Authorize(Roles = "User,Editor,Administrator")]
    public class FavoriteListController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: FavoriteList
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Index()
        {
            var userid = User.Identity.GetUserId();
            List<FavoriteList> favList = new List<FavoriteList>();
            var favorites = from fav in db.Favorites
                            where (fav.UserId == userid)
                            select fav;
            foreach(var fav in favorites)
            {
                favList.Add(fav);
            }

            List<Product> favProducts = new List<Product>();
            foreach (var fprod in favList)
            {
                var favoriteProducts = from fp in db.Products
                                       where fprod.ProductID == fp.ProductID
                                       select fp;

                foreach (var fp in favoriteProducts)
                {
                    favProducts.Add(fp);
                }
            }

            ViewBag.FavoriteProductsList = favProducts;
            return View();
        }


        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult OnPressRemoveProduct(int id)
        {
            string userid = User.Identity.GetUserId();
            Product product = db.Products.Find(id);
            var prod = from fp in db.Favorites
                       where fp.ProductID == product.ProductID &&
                       fp.UserId == userid
                       select fp;
            foreach (var p in prod)
            {
                db.Favorites.Remove(p);
            }
            TempData["message"] = "Post removed from Favorites!";

            db.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}