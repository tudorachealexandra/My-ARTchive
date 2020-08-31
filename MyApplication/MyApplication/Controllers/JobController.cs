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
    public class JobController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Job
        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Index()
        {
            var id = User.Identity.GetUserId();
            var jobs = from j in db.Jobs
                           where j.UserId == id
                           select j;
           
            ViewBag.Jobs = jobs;

            return View();
        }

        [Authorize(Roles = "User,Editor,Administrator")]
        public ActionResult Show(int id)
        {
            Job job = db.Jobs.Find(id);

            ViewBag.afisareButoane = false;
            if (User.IsInRole("Editor") || User.IsInRole("Administrator"))
            {
                ViewBag.afisareButoane = true;
            }

            if(job.JobTaken == true) { 
            ApplicationUser u = db.Users.Find(job.WinnerId);
            string WinnerName = u.UserName;
            ViewBag.WinnerName = WinnerName;
            }
            ViewBag.esteAdmin = User.IsInRole("Administrator");
            ViewBag.utilizatorCurent = User.Identity.GetUserId();
            ViewBag.status = job.Status;


            return View(job);

        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult New()
        {
            Job job = new Job();

            // preluam lista de categorii din metoda GetAllDifficulties()
            job.Difficulties = GetAllDifficulties();

            // Preluam ID-ul utilizatorului curent
            job.UserId = User.Identity.GetUserId();
           

            return View(job);

        }

        [HttpPost]
        [Authorize(Roles = "User,Administrator")]
        public ActionResult New(Job job)
        {
            job.Difficulties = GetAllDifficulties();
            try
            {
                if (ModelState.IsValid)
                {
                    job.Status = "In progress";
                    job.JobTaken = false;
                    db.Jobs.Add(job);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(job);
                }
            }
            catch (Exception e)
            {
               
                return View(job);

            }
        }

        [Authorize(Roles = "User,Administrator")]
        public ActionResult Edit(int id)
        {
            Job job = db.Jobs.Find(id);
            ViewBag.Job = job;
            job.Difficulties = GetAllDifficulties();

            if (job.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                return View(job);
            }
            else
            {
                TempData["message"] = "You can't edit this job!";
                return RedirectToAction("Sort");
            }

        }

        [HttpPut]
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Edit(int id, Job requestJob)
        {
            requestJob.Difficulties = GetAllDifficulties();

            try
            {
                if (ModelState.IsValid)
                {
                    Job job = db.Jobs.Find(id);
                    if (job.UserId == User.Identity.GetUserId() ||
                        User.IsInRole("Administrator"))
                    {
                        if (TryUpdateModel(job))
                        {
                            job.Title = requestJob.Title;
                            job.Content = requestJob.Content;
                            job.Date = requestJob.Date;
                            job.DifficultyId = requestJob.DifficultyId;
                            db.SaveChanges();
                           
                        }
                        return RedirectToAction("Show", new { id });
                    }
                    else
                    {
                        TempData["message"] = "You can't edit this job!";
                        return RedirectToAction("Sort");
                    }

                }
                else
                {
                    return View(requestJob);
                }

            }
            catch (Exception e)
            {
                return View(requestJob);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User,Administrator")]
        public ActionResult Delete(int id)
        {
            Job job = db.Jobs.Find(id);
            if (job.UserId == User.Identity.GetUserId() ||
                User.IsInRole("Administrator"))
            {
                db.Jobs.Remove(job);
                db.SaveChanges();

                return RedirectToAction("Sort");
            }
            else
            {
                TempData["message"] = "You can't delete this job!";
                return RedirectToAction("Sort");
            }

        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllDifficulties()
        {
            // generam o lista goala
            var selectList = new List<SelectListItem>();

            // Extragem toate categoriile din baza de date
            var difficulties = from dif in db.Difficulties
                             select dif;

            // iteram prin categorii
            foreach (var difficulty in difficulties)
            {
                // Adaugam in lista elementele necesare pentru dropdown
                selectList.Add(new SelectListItem
                {
                    Value = difficulty.DifficultyId.ToString(),
                    Text = difficulty.DifficultyName.ToString()
                });
            }

            // returnam lista de categorii
            return selectList;
        }

        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult TakeJob(int id)
        {
            string userid = User.Identity.GetUserId();
            Job job = db.Jobs.Find(id);
            JobList newJob = new JobList();

            

            //verifica daca jobul il are deja candidat pe user
            var openJobs = from tj in db.Commissions
                           where tj.JobId == id
                           where tj.CandidateId == userid
                           select tj;
            var check = 0;
            foreach (JobList j in openJobs)
            { check = 1; }
           
            if (check != 1)// nu exista participare de la user si trb creat
            {
               
                newJob.JobId = job.Id;
               
                newJob.CandidateId = userid;
               
                db.Commissions.Add(newJob);
              
                db.SaveChanges();
               

            }
            else//scoate cererea
            {
                foreach (JobList j in openJobs)
                {
                    db.Commissions.Remove(j);
                }
                db.SaveChanges();
            }

            return RedirectToAction("GetJobRequests", "Job", new { id = newJob.JobId });

        }

        
        public ActionResult GetJobRequests(int id)
        {
            
            // generam o lista goala
            List<JobList> commissionList = new List<JobList>();
            List<ApplicationUser> candidatesNames = new List<ApplicationUser>();
            Job job = db.Jobs.Find(id);
            if (job.UserId == User.Identity.GetUserId())
            { ViewBag.utilizatorAutor = true; }
            else
            { ViewBag.utilizatorAutor = false; }
            // Extragem toate cererile din baza de date
            var commissions = from co in db.Commissions
                            where co.JobId == job.Id
                            select co;

            // iteram prin categorii
            foreach (JobList co in commissions)
            {
                commissionList.Add(co);
            }

            ViewBag.requestCommissions = commissionList;

          foreach (JobList c in commissionList)
            {
                var commissionName = from co in db.Users
                                     where c.CandidateId == co.Id
                                     select co;

                foreach (ApplicationUser cn in commissionName)
                {
                    candidatesNames.Add(cn);
                }
            }

            ViewBag.candidateNamesList = candidatesNames;

            return View();
        }

        [Authorize(Roles = "User, Administrator")]
        public ActionResult AcceptJob(int id)
        {
            JobList jobl = db.Commissions.Find(id);
            var id_new = jobl.JobId;
            var winner_id = jobl.CandidateId;
            Job job = db.Jobs.Find(id_new);
            job.WinnerId = winner_id;
            job.JobTaken = true;
            db.SaveChanges();
           
            return RedirectToAction("Show", "Job", new { id = id_new });
        }

        public ActionResult Sort(string searchValue, string sortOrder)
        {


            if ((searchValue == "") || (searchValue == null))
            {
                switch (sortOrder)
                {

                    case "Ascending Date":
                        ViewBag.List = db.Jobs.ToList().OrderBy(p => p.Date).Where(p => p.JobTaken == false);
                        break;
                    case "Descending Date":
                        ViewBag.List = db.Jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.JobTaken == false); 
                        break;
                    case "Beginner":
                        ViewBag.List = db.Jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.DifficultyId == 1 && p.JobTaken == false);
                        break;
                    case "Intermediate":
                        ViewBag.List = db.Jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.DifficultyId == 2 && p.JobTaken == false);
                        break;
                    case "Professional":
                        ViewBag.List = db.Jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.DifficultyId == 3 && p.JobTaken == false);
                        break;
                    default:
                        ViewBag.List = db.Jobs.ToList().Where(p => p.JobTaken == false);
                        break;

                }
            }
            else
            {
                var jobs = db.Jobs.Where(p => p.Title.Contains(searchValue)).ToList();

                switch (sortOrder)
                {

                    case "Ascending Date":
                        ViewBag.List = jobs.ToList().OrderBy(p => p.Date).Where(p => p.JobTaken == false);
                        break;
                    case "Descending Date":
                        ViewBag.List = jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.JobTaken == false);
                        break;
                    case "Beginner":
                        ViewBag.List = jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.DifficultyId == 1 && p.JobTaken == false);
                        break;
                    case "Intermediate":
                        ViewBag.List = jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.DifficultyId == 2 && p.JobTaken == false);
                        break;
                    case "Professional":
                        ViewBag.List = jobs.ToList().OrderByDescending(p => p.Date).Where(p => p.DifficultyId == 3 && p.JobTaken == false);
                        break;
                    default:
                        ViewBag.List = jobs.ToList().Where(p => p.JobTaken == false);
                        break;
                }
            }

            return View();
        }


    }
}