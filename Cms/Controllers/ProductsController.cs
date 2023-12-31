﻿using Shared.InfraStructure;
using Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cms.Controllers
{
    //[Authorize(Roles = "admin")]
    //[Area("Admin")]
    public class ProductsController : Controller
    {
        
        private readonly CmsShoppingCartContext context;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ProductsController(CmsShoppingCartContext context, IWebHostEnvironment webHostEnvironment)
        {
            this.context = context;
            this.webHostEnvironment = webHostEnvironment;
        }
        //GET /admin/products
        public async Task<IActionResult> Index(int p = 1)
        {
            int pageSize = 4;
            var products = context.Products.OrderByDescending(x => x.Id)
                                         .Include(x => x.Category)
                                         .Skip((p - 1) * pageSize)
                                         .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)context.Products.Count() / pageSize);

             return View(await products.ToListAsync());
           
        }
        //GET /admin/products/create/id=5
        public IActionResult Create() {

            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name");

            return View(); 
        }


        //POST /admin/products/create
        [HttpPost] //bt5aleha tbayen bara bas 23mela create
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)

        {

            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name");

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-"); //replace space with -
               

                var slug = await context.Products.FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The product already exists");
                    return View(product);
                }
                string imageName = "noimage.png";

                if(product.Imageupload !=null)
                {
                    string uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "media/products");
                    imageName = Guid.NewGuid().ToString() + "_" + product.Imageupload.FileName;
                    string filepath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filepath, FileMode.Create);
                    await product.Imageupload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }


                context.Add(product);
                await context.SaveChangesAsync();

                TempData["Success"] = "The product has been added";

                return RedirectToAction("Index");
            }
            return View(product);
        }

        //GET /admin/products/details/id=5
        public async Task<IActionResult> Details(int id)
        {
            Product product = await context.Products.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //GET /admin/product/delete/id=5
        public async Task<IActionResult> Delete(int id)
        {
            Product product = await context.Products.FindAsync(id);
            if (product == null)
            {

                TempData["Error"] = "The product does not exists!";
            }
            else
            {
                if (!string.Equals(product.Image, "noimage.png"))
                {
                    string uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "media/products");
                    string oldImagePath = Path.Combine(uploadsDir, product.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }



                context.Products.Remove(product);
                await context.SaveChangesAsync();

                TempData["Success"] = "The product has been deleted!";

            }
            return RedirectToAction("Index");
        }
        //GET /admin/products/edit/id=5
        public async Task<IActionResult> Edit(int id)
        {
            Product product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name" ,product.CategoryId);
            return View(product);
        }

        //POST /admin/products/edit
        [HttpPost] //bt5aleha tbayen bara bas 23mela create
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)

        {

            ViewBag.CategoryId = new SelectList(context.Categories.OrderBy(x => x.Sorting), "Id", "Name", product.CategoryId);

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.ToLower().Replace(" ", "-"); //replace space with -


                var slug = await context.Products.Where(x => x.Id != id).FirstOrDefaultAsync(x => x.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "The product already exists");
                    return View(product);
                }
               

                if (product.Imageupload != null)
                {
                    string uploadsDir = Path.Combine(webHostEnvironment.WebRootPath, "media/products");
                    
                    if(!string.Equals(product.Image, "noimage.png"))
                    {
                        string oldImagePath = Path.Combine(uploadsDir, product.Image);
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    
                   string imageName = Guid.NewGuid().ToString() + "_" + product.Imageupload.FileName;
                    string filepath = Path.Combine(uploadsDir, imageName);
                    FileStream fs = new FileStream(filepath, FileMode.Create);
                    await product.Imageupload.CopyToAsync(fs);
                    fs.Close();
                    product.Image = imageName;
                }


                context.Update(product);
                await context.SaveChangesAsync();

                TempData["Success"] = "The product has been edited";

                return RedirectToAction("Index");
            }
            return View(product);
        }

        public IActionResult ManageSales()
        {
            var products = context.Products.ToList();
            return View(products);
        }

        [HttpPost]
        public IActionResult UpdateSaleStatus(int id, bool isOnSale, decimal salePrice, DateTime? saleEndDate)
        {
            var product = context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsOnSale = isOnSale;
            product.SalePrice = salePrice;
            product.SaleEndDate = saleEndDate;

            context.SaveChanges();

            return RedirectToAction("ManageSales");
        }
    }

}

