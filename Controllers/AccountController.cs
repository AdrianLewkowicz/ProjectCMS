using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Account;
using CmsShop.Models.ViewModels.Shop;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~~/account/Login");
        }
        // GET: Account/Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            // sprawdzanie czy użytkownik nie jest już zalogowany
            string UserName = User.Identity.Name;
            if (!string.IsNullOrEmpty(UserName))
                return RedirectToAction("user-profile");

            // zwracamy widok logowania
            return View();
        }
        // POST: Account/Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // sprawdzamy czy model state jest prawidołwy
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            // sprawdzamy użytkownika
            bool isValid = false;
            using (DB db = new DB())
            {
                if(db.Users.Any(x => x.UserName.Equals(model.UserName) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if(!isValid)
            {
                ModelState.AddModelError("", "Nieprawidłowa nazwa użytkowniak lub hasło");
                return View(model);
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.UserName, model.RememberMe));
            }
                
        }
        // GET: Account/Account/Create-account
        [ActionName("Create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }
        [ActionName("Create-account")]
        [HttpPost]
        //POST: Account/Account/Create-account
        public ActionResult CreateAccount(UserVM model)
        {
            // sprawdzamy model State
            if(!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }
            
            // sprawdzenie hasła
            if(!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Hasła nie pasują do siebie");
                return View("CreateAccount", model);
            }

            using(DB db = new DB())
            {
                // sprawdzenie czy nazwa użytkowniak jest unikalna
                if(db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", "Zawza użytkownika już jest zajęta" + model.UserName + "jest już zajęta");
                    model.UserName = "";
                    return View("CreateAccount", model);
                }

                // utworzenie użytkownika
                UserDTO usserDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    UserName = model.UserName,
                    Password = model.Password,
                };

                // dodanie Użytkownia
                db.Users.Add(usserDTO);
                //zapis na bazie
                db.SaveChanges();

                // dodanie roli dla użytkownika
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = usserDTO.Id,
                    RolesId = 2
                };

                // Dodanie roli
                db.UserRoles.Add(userRoleDTO);
                // Zapis na bazie
                db.SaveChanges();
            }
            // TembData komunikat 
            TempData["SM"] = "Jesteś teraz zarejestrowany i możesz się zalogować ";

            return Redirect("~/account/login");
        }
        // GET: Account/Account/Logout
        [HttpGet]
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }
        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Pobieramy username

            string usernname = User.Identity.Name;

            // deklrarujemy model
            UserNavPartialVM model;

            using (DB db = new DB())
            {
                // Pobieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == usernname);
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                };
            }
                return PartialView(model);
        }
        // GET: Account/Account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // pobieramy nazwę użytkownika
            string username = User.Identity.Name;

            // deklradujemy model state
            UserProfileVM model;

            using (DB db = new DB())
            {
                // pobieramy użytkownika
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == username);

                model = new UserProfileVM(dto);
            }
                return View("UserProfile",model);
        }
        // POST: Account/Account/user-profile
        [HttpPost]
        public ActionResult UserProfile(UserProfileVM model)
        {
            // sprawdzanie model state
            if(!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }
            
            // sprawdzamy hasła
            if(!string.IsNullOrWhiteSpace(model.Password))
            {
                if(!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Hasła nie pasują do siebie.");
                    return View("UserProfile", model);
                }
            }

            using (DB db = new DB())
            {
                // pobieramy nazwę uzytkowniak
                string username = User.Identity.Name;

                // sprawdzenie czy nazwa uzytkowniak jest unikalna
                if(db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == username))
                {
                    ModelState.AddModelError("", "Nazwa uzytkownika "+ model.UserName + "  zajęta");
                    model.UserName = "";
                    return View("UserProfile", model);
                }

                // Edycja DTO
                UserDTO dto = db.Users.Find(model.Id);
                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.UserName = model.UserName;
                
                if(!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
              
                // zapis
                db.SaveChanges();
            }

            // ustawienie komunikatu zmiennej 
            TempData["SM"] = "Edytowałeś swój profil";
                return Redirect("~/account/user-rofile");
        }

        // GET: Account/Account/Orders
        [HttpGet]
        [Authorize(Roles ="User")]
        public ActionResult Orders()
        {
            // inicjalizcaja lity zamowieniem dla uzytkownika
            List<OrderForUserVM> ordersForUser = new List<OrderForUserVM>();

            using (DB db = new DB())
            {
                // pobieramy id uzytkownika
                UserDTO user = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                // pobieramy zamowienia dla uzytkownika
                List<OrderVM> orders = db.Orders.Where(x => x.OrderId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                foreach(var order in orders)
                {
                    // inicjalizacja słowknika produktów
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    decimal total = 0;

                    // pobieramy szczegóły zamówienia
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    foreach(var orderDetails in orderDetailsDTO)
                    {
                        // pobieramy produkt
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // pobieramy cene
                        decimal price = product.Price;

                        // pobieramy nazwe 
                        string productname = product.Name;

                        // dodajemy produkt do słownika
                        productsAndQty.Add(productname, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    ordersForUser.Add(new OrderForUserVM() {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
                return View(ordersForUser);
        }
    }
}