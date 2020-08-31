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
    public class StarRating
    {
        [Key]
        public string StarRatingId { get; set; }

        public string selectedValue { get; set; }

        public IEnumerable<SelectListItem> Values { get; set; }

        public string ReceiverId { get; set; }

        public string GiverId { get; set; }

        public virtual ApplicationUser Receiver { get; set; }

        public virtual ApplicationUser Giver { get; set; }

    }

    
}