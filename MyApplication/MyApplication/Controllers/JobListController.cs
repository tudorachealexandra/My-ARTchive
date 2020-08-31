using Microsoft.AspNet.Identity;
using MyApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyApplication.Controllers
{
    public class JobListController : Controller
    {
        private ApplicationDbContext db = ApplicationDbContext.Create();

        // GET: JobList
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Index()
        {
            var userid = User.Identity.GetUserId();
            List<JobList> jobList = new List<JobList>();
            var jobs = from j in db.Commissions
                            where (j.CandidateId == userid)
                            select j;
            foreach (var j in jobs)
            {
                jobList.Add(j);
            }

            List<Job> actualJobs = new List<Job>();
            foreach (var jo in jobList)
            {
                var actJobs = from j in db.Jobs
                                       where jo.JobId == j.Id
                                       select j;

                foreach (var j in actJobs)
                {
                    actualJobs.Add(j);
                }
            }

            ViewBag.CommissionList = actualJobs;
            return View();
        }


        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult OnPressRemoveJob(int id)
        {
            string userid = User.Identity.GetUserId();
            Job job = db.Jobs.Find(id);
            var jo = from j in db.Commissions
                       where j.JobId == job.Id &&
                       j.CandidateId == userid
                       select j;
            foreach (var j in jo)
            {
                db.Commissions.Remove(j);
            }
            TempData["message"] = "Job request removed from commissions!";

            db.SaveChanges();
            return RedirectToAction("Index");
        }


       
    }
}
