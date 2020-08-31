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
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter a name for the category.")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Please enter a description for the category.")]
        public string Description { get; set; }

        [NotMapped]
        public HttpPostedFileBase Image { get; set; }

        public string ImagePath { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }

}