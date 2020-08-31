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
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static Random random = new Random();

        // GET: Products
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Index()
        {
            var products = from prod in db.Products.Include("Category").Include("User")
                            where prod.ProductApprove == true
                            select prod;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            ViewBag.Products = products.OrderByDescending(p => p.CreatedOn).Take(9);

            ViewBag.auth = true;

            
            return View();
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int id)
        {
            Product product = db.Products.Find(id);

            ViewBag.afisareButoane = false;
            if (User.IsInRole("Editor") || User.IsInRole("Administrator"))
            {
                ViewBag.afisareButoane = true;
            }

            ViewBag.esteAdmin = User.IsInRole("Administrator");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
            ViewBag.Comments = GetAllComments(id);
            ViewBag.Photos = GetAllUserPhotos(GetAllComments(id));
            ViewBag.Likes = GetReview(id);
            ViewBag.Favorites = GetFavorite(id);

            return View(product);

        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New()
        {
            Product product = new Product();

            // preluam lista de categorii din metoda GetAllCategories()
            product.Categories = GetAllCategories();

            // Preluam ID-ul utilizatorului curent
            product.UserId = User.Identity.GetUserId();


            return View(product);

        }

        [HttpPost]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult New(Product product)
        {
            product.Categories = GetAllCategories();
            try
            {
                if (ModelState.IsValid)
                {
                    if (product.Image != null)
                    {
                        var filename = string.Concat(RandomString(10), product.Image.FileName);
                        var path = string.Concat("~/Images/", filename);
                        product.Image.SaveAs(Server.MapPath("~/Images/") + filename);
                        product.ImagePath = path;
                    }
                    product.CreatedOn = DateTime.Now;
                    product.ProductApprove = false;
                    db.Products.Add(product);
                    db.SaveChanges();
                   
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(product);
                }
            }
            catch (Exception e)
            {
                return View(product);
            }
        }

        [NonAction]
        [Authorize(Roles = "Editor,Administrator")]
        public bool DeleteReviews(int id)
        {
            Product product = db.Products.Find(id);
            var reviews = from rev in db.Ratings
                          where rev.ProductID == id
                          select rev;

            foreach (LikeRating rev in reviews)
            {
                db.Ratings.Remove(rev);
            }
            db.SaveChanges();
            return true;
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int id)
        {
            Product product = db.Products.Find(id);
            ViewBag.Product = product;
            product.Categories = GetAllCategories();

            if (product.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                return View(product);
            }
            else
            {
                TempData["message"] = "You don't have the right to modify this!";
                return RedirectToAction("Show", "Product", new {id});
            }

        }
        

        [HttpPut]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int id, Product requestProduct)
        {
            requestProduct.Categories = GetAllCategories();

            try
            {
                if (ModelState.IsValid)
                {
                    Product product = db.Products.Find(id);
                    if (product.UserId == User.Identity.GetUserId() ||
                        User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(product))
                        {
                            if (requestProduct.Image != null)
                            {
                                var filename = string.Concat(RandomString(10), requestProduct.Image.FileName);
                                var path = string.Concat("~/Images/", filename);
                                requestProduct.Image.SaveAs(Server.MapPath("~/Images/") + filename);
                                requestProduct.ImagePath = path;
                                product.ImagePath = requestProduct.ImagePath;
                            }
                            product.Title = requestProduct.Title;
                            product.Description = requestProduct.Description;
                            product.Image = requestProduct.Image;
                            product.CategoryId = requestProduct.CategoryId;
                            db.SaveChanges();
                           
                        }
                        return RedirectToAction("Show", "Product", new { id });
                    }
                    else
                    {
                        TempData["message"] = "You don't have the right to modify this!";
                        return RedirectToAction("Show", "Product", new { id });
                    }

                }
                else
                {
                    return View(requestProduct);
                }

            }
            catch (Exception e)
            {
                return View(requestProduct);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            if (product.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                db.Products.Remove(product);
                bool b = this.DeleteReviews(id);
                db.SaveChanges();
               
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "You don't have the right to delete this!";
                return RedirectToAction("Index");
            }

        }

        [NonAction]
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // Extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // Adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }
            // returnam lista de categorii
            return selectList;
        }

        public ActionResult OnPressFavorite(int id)
        {
            string userid = User.Identity.GetUserId();
            Product product = db.Products.Find(id);
            FavoriteList newFav = new FavoriteList();

            //verifica daca produsul are deja favorite de la user
            var favorite = from fav in db.Favorites
                         where fav.ProductID == id
                         where fav.UserId == userid
                         select fav;
            var check = 0;
            foreach (FavoriteList f in favorite)
            { check = 1; }

            if (check == 0)// nu exista fav de la user si trb creat
            {
                newFav.ProductID = product.ProductID;
                newFav.UserId = userid;
                db.Favorites.Add(newFav);
               
                db.SaveChanges();

            }
            else//scoate de la fav
            {
                foreach (FavoriteList fav in favorite)
                {
                    db.Favorites.Remove(fav);
                }
                db.SaveChanges();
            }

            return RedirectToAction("Show", "Product", new { id = product.ProductID });
           
        }

        [NonAction]
        public int GetFavorite(int id)
        {
            string userid = User.Identity.GetUserId();
            // generam o lista goala
            int favs = 0;
            Product product = db.Products.Find(id);
            // Extragem toate categoriile din baza de date
            var favorites = from fav in db.Favorites
                          where fav.ProductID == product.ProductID
                          select fav;

            // iteram prin categorii
            foreach (FavoriteList f in favorites)
            {
                favs++;
            }
            product.Favorites = favs;
            db.SaveChanges();
            // returnam lista de categorii
            return favs;
        }

        [NonAction] //returneaza true daca a gasit produsul
        public bool CheckProduct(List<Product> l, string numeProdus)
        {
            foreach(var prod in l)
            {
                if (prod.Title == numeProdus)
                    return true;
            }
            return false;
        }

        public ActionResult OnPressAddComment(int id)
        {
            Product product = db.Products.Find(id);
            return RedirectToAction("New", "Comment", new { id });
        }

        
        
        [NonAction]
        public List<Comment> GetAllComments(int productId)
        {
            List<Comment> commentsList = new List<Comment>();
            var comments = from comm in db.Comments.Include("User").Include("Product")
                           where comm.CommentProductId == productId
                           select comm;

            foreach (var comm in comments)
            {
                commentsList.Add(comm);
            }
            return commentsList;
        }

        [NonAction]
        public List<string> GetAllUserPhotos(List<Comment> lista)
        {
            List<string> userPhotos = new List<string>();
           foreach(Comment com in lista)
            {
                ApplicationUser user = db.Users.Find(com.UserId);
                string userPhoto = user.ImagePath;
                userPhotos.Add(userPhoto);
            }
            return userPhotos;
        }


        public ActionResult OnPressLike(int id)
        {
            
            Product product = db.Products.Find(id);
            string userid = User.Identity.GetUserId();
            LikeRating revieww = new LikeRating();

            //verifica daca produsul are deja like de la user
            var review = from rev in db.Ratings
                          where rev.ProductID == id
                          where rev.UserId == userid
                          select rev;
            var check = 0;
            foreach (LikeRating r in review)
            { check = 1; }
 
            if (check == 0)// nu exista review de la user si trb creat
            {
                revieww.ProductID = product.ProductID;
                revieww.UserId = userid;

                db.Ratings.Add(revieww);
               
                db.SaveChanges();

            }
            else//unlike
            {
                foreach (LikeRating rev in review)
                {
                    db.Ratings.Remove(rev);
                }
                db.SaveChanges();
            }

            return RedirectToAction("Show", "Product", new { id = product.ProductID });
        }

        [NonAction]
        public int GetReview(int id)
        {
            string userid = User.Identity.GetUserId();
            // generam o lista goala
            int likes = 0;
            Product product = db.Products.Find(id);
            // Extragem toate categoriile din baza de date
            var reviews = from rev in db.Ratings
                          where rev.ProductID == product.ProductID
                          select rev;

            // iteram prin categorii
            foreach (LikeRating r in reviews)
            {
                likes++;
            }
            product.Likes = likes;
            db.SaveChanges();
            // returnam nr de likeuri
            return likes;
        }

        public ActionResult Sort(string searchValue, string sortOrder)
        {
            if ((searchValue =="") || (searchValue == null))
            {
                switch (sortOrder)
                {
                    case "Ascending Date":
                        ViewBag.List = db.Products.ToList().OrderBy(p => p.CreatedOn).Where(p => p.ProductApprove == true);
                        break;
                    case "Descending Date":
                        ViewBag.List = db.Products.ToList().OrderByDescending(p => p.CreatedOn).Where(p => p.ProductApprove == true);
                        break;
                    case "Most Likes":
                        ViewBag.List = db.Products.ToList().OrderByDescending(p => p.Likes).Where(p => p.ProductApprove == true);
                        break;
                    case "Least Likes":
                        ViewBag.List = db.Products.ToList().OrderBy(p => p.Likes).Where(p => p.ProductApprove == true);
                        break;
                    case "Most Popular":
                        ViewBag.List = db.Products.ToList().OrderByDescending(p => p.Favorites).Where(p => p.ProductApprove == true);
                        break;
                    case "Least Popular":
                        ViewBag.List = db.Products.ToList().OrderBy(p => p.Favorites).Where(p => p.ProductApprove == true);
                        break;
                    default:
                        ViewBag.List = db.Products.ToList().Where(p => p.ProductApprove == true);
                        break;
                }
            }
            else
            {            
                var products = db.Products.Where(p => p.Title.Contains(searchValue)).ToList();
                switch (sortOrder)
                {

                    case "Ascending Date":
                        ViewBag.List = products.ToList().OrderBy(p => p.CreatedOn).Where(p => p.ProductApprove == true);
                        break;
                    case "Descending Date":
                        ViewBag.List = products.ToList().OrderByDescending(p => p.CreatedOn).Where(p => p.ProductApprove == true);
                        break;
                    case "Most Likes":
                        ViewBag.List = products.ToList().OrderByDescending(p => p.Likes).Where(p => p.ProductApprove == true);
                        break;
                    case "Least Likes":
                        ViewBag.List = products.ToList().OrderBy(p => p.Likes).Where(p => p.ProductApprove == true);
                        break;
                    case "Most Popular":
                        ViewBag.List = products.ToList().OrderByDescending(p => p.Favorites).Where(p => p.ProductApprove == true);
                        break;
                    case "Least Popular":
                        ViewBag.List = products.ToList().OrderBy(p => p.Favorites).Where(p => p.ProductApprove == true);
                        break;
                    default:
                        ViewBag.List = products.ToList().Where(p => p.ProductApprove == true);
                        break;
                }
            }
            return View();
        }

        public ActionResult Approve()
        {
            var products = from prod in db.Products
                           where prod.ProductApprove == false
                           select prod;

            ViewBag.Products = products;
            return View();
        }

        public ActionResult ApproveProduct(int id)
        {
            Product product = db.Products.Find(id);
            product.ProductApprove = true;
            db.SaveChanges();
           
            return RedirectToAction("Approve", "Product");
        }

    }

}