﻿using CmsShoppingCart.InfraStructure;
using CmsShoppingCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CmsShoppingCart.Controllers
{
  //  [Authorize]
    public class ProductsController : Controller
    {
        private readonly CmsShoppingCartContext context;

        public ProductsController(CmsShoppingCartContext context)
        {
            this.context = context;
        }

        //GET /products
        public async Task<IActionResult> Index(string categorySlug = "", int p = 1, string sortOrder = "", string search = "")
        {
            int pageSize = 4;
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.CategorySlug = categorySlug;

            var productsQuery = context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(categorySlug))
            {
                Category category = await context.Categories.FirstOrDefaultAsync(c => c.Slug == categorySlug);
                if (category == null)
                {
                    return RedirectToAction("Index");
                }


                productsQuery = productsQuery.Where(p => p.CategoryId == category.Id);
            }

            if (!string.IsNullOrEmpty(sortOrder))
            {
                switch (sortOrder)
                {
                    case "price_asc":
                        productsQuery = productsQuery.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        productsQuery = productsQuery.OrderByDescending(p => p.Price);
                        break;
                    default:

                        productsQuery = productsQuery.OrderByDescending(p => p.Id);
                        break;
                }
            }
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)productsQuery.Count() / pageSize);
            if (!string.IsNullOrEmpty(search))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }
            return View(await productsQuery.Skip((p - 1) * pageSize).Take(pageSize).ToListAsync());
        }
    

            //GET /products/category

            public async Task<IActionResult> ProductByCategory(string categorySlug, int p = 1)
        {
            Category category = await context.Categories.Where(x => x.Slug == categorySlug).FirstOrDefaultAsync();
            if (category == null)
            {
                return RedirectToAction("Index");
            }


            int pageSize = 4;
            var products = context.Products.OrderByDescending(x => x.Id)
                                        .Where(x => x.CategoryId == category.Id)
                                        .Skip((p - 1) * pageSize)
                                         .Take(pageSize);

            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)context.Products.Where(x => x.CategoryId == category.Id).Count() / pageSize);
            ViewBag.CategoryName = category.Name;
            @ViewBag.CategorySlug = categorySlug;

            return View(await products.ToListAsync());
        }

        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
           
            var results = context.Products.Where(x => x.Name.Contains(searchTerm)).ToList();
            return View( results);

        }




    }
}
