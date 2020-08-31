using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyApplication.Models
{
    public class Difficulty
    {
        [Key]
        public int DifficultyId { get; set; }

        [Required(ErrorMessage = "Please provide a name!")]
        public string DifficultyName { get; set; }

        public virtual ICollection<Job> Job { get; set; }
    }
}