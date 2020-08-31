using MyApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace MyApplication.Controllers
{
    public class CommentController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();
        // GET: CommentList
        public ActionResult Index(int id)
        {
            return RedirectToAction("Index", "Product");
        }

        public ActionResult New(int id)
        {
            string userid = User.Identity.GetUserId();
            var user = db.Users.Find(userid);
            ViewBag.img = user.ImagePath;
            Comment comment = new Comment();
            comment.CommentProductId = id;
            comment.UserId = userid;
            comment.UserName = user.UserName;
            
            return View(comment);
        }

        [HttpPost]
        public ActionResult New(Comment comment)
        {
            
                if (ModelState.IsValid)
                {
                    
                    db.Comments.Add(comment);

                    db.SaveChanges();
                   
                    return RedirectToAction("Show", "Product", new { id = comment.CommentProductId });
                }
                else
                {
                    return View(comment);
                }
            
        }

        
        public ActionResult Show(int id)
        {
            Comment c = db.Comments.Find(id);
          
            var user = db.Users.Find(c.UserId);
           
            ViewBag.img = user.ImagePath;
           
            List<Comment> commentsList = new List<Comment>();
            var comments = from comm in db.Comments
                           where comm.CommentProductId == id
                           select comm;

            foreach (var comm in comments)
            {
                commentsList.Add(comm);

            }

            ViewBag.Comments = commentsList;
            return View();

        }

        public ActionResult ShowAll(int id)
        {
            
            List<Comment> commentsList = new List<Comment>();
            var comments = from comm in db.Comments
                           where comm.CommentProductId == id
                           select comm;

            foreach (var comm in comments)
            {
                commentsList.Add(comm);
               
            }

            ViewBag.Comments = commentsList;
           
          
            return View();
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int id)
        {

            Comment c = db.Comments.Find(id);
            var prodId = c.CommentProductId;
            var comment = from comm in db.Comments
                          where comm.CommentId == id
                          select comm;

            foreach (Comment comm in comment)
            {
                db.Comments.Remove(comm);
            }

            db.SaveChanges();
            
            return RedirectToAction("Show", "Product", new { id = prodId });

        }
    }
}