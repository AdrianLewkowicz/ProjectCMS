using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{pages}
        public ActionResult Index(string page = "")
        {
            // Ustawiamy adres naszej strony
            if (page == "")
            {
                page = "home";
            }
            // Deklaraujemy pageVM i PageDTO

            PageVM model;
            PageDTO dto;
            // Sprawdzaimy czy strona istniej
            using (DB db = new DB())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });

            }

            //pobieramy PageDTO
            using (DB db = new DB())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }
            // ustawiamy tytuł naszej strony 
            ViewBag.PageTitle = dto.Title;
            // Sprawdzamy czy strona ma pasek boczny
            if (dto.HasSidebar == true)
                ViewBag.Sidebar = "Tak";

            else
                ViewBag.Sidebar = "Nie";

            //inicjalizujemy pageVM
            model = new PageVM(dto);
            // zwracamy widok z modelem 
            return View(model);
        }
         
        public ActionResult PagesMenuPartial()
        {
            // Deklaracja PageVM

            List<PageVM> pageVMList;

            // Pobranie stron
            using (DB db = new DB())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home").Select(x => new PageVM(x)).ToList();
            }
            
            // Zwracamy PageVMList
            return PartialView(pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            // Deklarujemy model
            SidebarVM model;

            // Inicjalizujemy model
            using (DB db = new DB())
            {
                SidebarDTO dto = db.Sidebar.Find(1);
                model = new SidebarVM(dto);
            }
                // zwracamy Patrial z modelem
                return PartialView(model);
        }
    }
}