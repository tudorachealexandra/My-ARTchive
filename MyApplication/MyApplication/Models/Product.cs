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
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        [Required(ErrorMessage = "Please enter a name for the post.")]
        [StringLength(50, ErrorMessage ="Name is too long.")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please enter a description for the post.")]
        public string Description { get; set; }

        [NotMapped]
        public HttpPostedFileBase Image { get; set; }

        public string ImagePath { get; set; }

        public DateTime CreatedOn { get; set; }

        [Required(ErrorMessage = "Please choose a category.")]
        public int CategoryId { get; set; }

        public string UserId { get; set; }

        public bool ProductApprove { get; set; }

        public virtual Category Category { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

        public List<Comment> Comments { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int Likes { get; set; }

        public int Favorites { get; set; }

      
    }

    public class ViewModelComment

    {
        public ApplicationUser MyUser { get; set; }
        public Comment MyComment { get; set; }
    }


    public class ProductDBContext : DbContext
    {
        public ProductDBContext() : base("DefaultConnection") { }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<FavoriteList> FavoriteLists { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<LikeRating> Ratings { get; set; }
    }
    
}