﻿using CmsShoppingCart.InfraStructure;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CmsShoppingCart.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, MinLength(2, ErrorMessage = "Minimum length is 2")]
        public string Name { get; set; }
        public string Slug { get; set; }
        [Required, MinLength(4, ErrorMessage = "Minimum length is 4")]
        public string Description { get; set; }
        [Column(TypeName ="decimal(10,2)")]
        public decimal Price { get; set; }
        [Display(Name ="Category")]
        [Range(1, int.MaxValue, ErrorMessage ="You must choose a category")]
        public int CategoryId { get; set; }
        
        public string Image { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [NotMapped] //To do not do anything to the table of database
        [FileExtension]
        public IFormFile Imageupload{ get; set; }
    }
}
