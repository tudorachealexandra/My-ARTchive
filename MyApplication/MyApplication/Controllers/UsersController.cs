using MyApplication.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyApplication.Controllers
{

    public class UsersController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        [Authorize(Roles = "User,Editor,Administrator")]
        // GET: Users
        public ActionResult Index()
        {
            List<ApplicationUser> uList = new List<ApplicationUser>();
            var users = from user in db.Users
                        orderby user.UserName
                        select user;

            foreach(var u in users)
            {
                uList.Add(u);
            }

            ViewBag.UsersList = uList;
           
            return View();
        }

  

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            ViewBag.utilizatorCurent = User.Identity.GetUserId();

            var roles = db.Roles.ToList();

            var roleName = roles.Where(j => j.Id ==
               user.Roles.FirstOrDefault().RoleId).
               Select(a => a.Name).FirstOrDefault();

            ViewBag.roleName = roleName;


            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            ViewBag.userId = user.Id;
            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            // var userRole = user.Roles.FirstOrDefault();
            // ViewBag.userRole = userRole.RoleId;

            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;

                    var roles = from role in db.Roles select role;
                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(id, role.Name);
                    }

                    var selectedRole = db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(id, selectedRole.Name);

                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }
        }


        [Authorize(Roles = "Administrator")]
        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete]
        public ActionResult Delete(string id)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);

            var products = db.Products.Where(a => a.UserId == id);
            foreach (var product in products)
            {
                db.Products.Remove(product);

            }
            // Commit pe articles
            db.SaveChanges();
            UserManager.Delete(user);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult OnPressUser(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            ViewBag.user = user;
            ViewBag.name = user.UserName;
            var counter = 0;
  
            var products = from prod in db.Products.Include("Category").Include("User")
                           where prod.ProductApprove == true && prod.UserId == id
                           select prod;

            var jobs = from j in db.Jobs.Include("Difficulty").Include("User")
                           where j.UserId == id
                           select j;
            foreach (var p in products)
            {
                counter++; 
            }
            ViewBag.Counter = counter;
            ViewBag.Jobs = jobs;
            ViewBag.Products = products;
            
            var result = new StarRatingController().CalculateRating(id);
            var finalResult = result.ToString("F2");
            ViewBag.RatingFinal = finalResult;
            return View(user);

        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult OnPressAddRating(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            return RedirectToAction("New", "StarRating", new { id = id });
        }

    }
}