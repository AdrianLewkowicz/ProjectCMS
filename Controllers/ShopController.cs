using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index","Pages");
        }
        public ActionResult CategoryMenuPartial()
        {
            // Deklarujemy CategoryVM List

            List<CategoryVM> CategoryVMList;

            // Inicjalizacja listy
            using (DB db = new DB())
            {
                CategoryVMList = db.Categories
                                   .ToArray()
                                   .OrderBy(x => x.Sorting)
                                   .Select(x => new CategoryVM(x))
                                   .ToList();
            }
            // Zwracamy Partial z lista
                return View(CategoryVMList);
            
        }
        //GET: shop/category/name
        public ActionResult Category(string name)
        {
            // deklaracja productVMList
            List<ProductVM> productVMList;

            using (DB db = new DB())
            {
                // pobranie id kategorii
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                // inicjalizacja list produktów
                productVMList = db.Products
                                  .ToArray()
                                  .Where(x => x.CategoryId == catId)
                                  .Select(x => new ProductVM(x)).ToList();

                // pobieramy nazwe kategorii
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }
            // Zwracamy widok z listą  produktów z danej kategorii

                return View(productVMList);
        }
        // GET: /Shop/Product-szczegoly/name
        [ActionName("product-szczegoly")]
        [HttpGet]
        public ActionResult ProductDetails(string name)
        {
            // deklracja productVM i productDTO
            ProductVM model;
            ProductDTO dto;

            // inicjalizacja prodctId
            int id = 0;

            using (DB db = new DB())
            {
                // sprawdzamy czy produkt istnieje
                if(!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index","Shop");
                }

                // Inicjalizacja productDTO
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                // pobranie id
                id = dto.Id;

                // inicjalizacja modelu 

                model = new ProductVM(dto);
            }
            // pobieramy galerie zdjęć dla wybranego produktu
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(fu => Path.GetFileName(fu));

            // Zwracamy widok z modelem
                return View("ProductDetails", model);
        }
    }
}