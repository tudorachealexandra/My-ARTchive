using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyApplication.Models
{
    public class JobList
    {
        [Key]
        public int Id { get; set; }

        public int JobId { get; set; }

        public string CandidateId { get; set; }

        public virtual Job Job { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}