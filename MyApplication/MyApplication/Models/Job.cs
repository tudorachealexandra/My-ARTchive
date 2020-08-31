using MyApplication.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyApplication.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please provide a title for the Job")]
        [StringLength(20, ErrorMessage = "Title too long!(Max. 20 characters)")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please provide a content for the Job")]
        public string Content { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Date and time are a must!")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Please provide a difficulty level!")]
        public int DifficultyId { get; set; }

        public string Status { get; set; }

        public bool JobTaken { get; set; }

        public string UserId { get; set; }

        public string WinnerId { get; set; }

        public virtual Difficulty Difficulty { get; set; }

        public IEnumerable<SelectListItem> Difficulties { get; set; }

        public virtual ApplicationUser User { get; set; }
    }

    public class JobDBContext : DbContext
    {
        public JobDBContext() : base("DefaultConnection") { }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Difficulty> Difficulties { get; set; }
        public DbSet<JobList> JobLists { get; set; }

    }
}