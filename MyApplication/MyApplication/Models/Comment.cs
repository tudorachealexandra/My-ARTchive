using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyApplication.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public int CommentProductId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        [Required(ErrorMessage = "Please add a content for the comment!")]
        public string CommentTxt { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Product Product { get; set; }
    }
}