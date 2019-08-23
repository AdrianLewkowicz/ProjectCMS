using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace CmsShop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Inicjalizcaja koszyka
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // sprawdzamy czy nasz koszyk jest pusty
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Twój koszyk jest pusty";
                return View();
            }
            // obliczenie wartosci podsumowania koszyka i przekazanie do ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // Inicjalizacja CartVM
            CartVM model = new CartVM();

            // Inicjalizcja Ilosci i cena
            int Qty = 0;
            decimal Price = 0;

            // sprawdzamy czy mamy dane koszyka zapisane w sensi
            if (Session["cart"] != null)
            {
                // pobieranie wartosci z sesii
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    Qty += item.Quantity;
                    Price += item.Quantity * item.Price;
                }
                model.Quantity = Qty;
                model.Price = Price;
            }
            else
            {
                // Ustawiamy ilość i cena na 0
                Qty = 0;
                Price = 0m;
            }
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            // Inicjalizcaja CartVM list
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Inicjalizcja CartVM
            CartVM model = new CartVM();

            using (DB db = new DB())
            {
                // pobieramy produkt
                ProductDTO product = db.Products.Find(id);

                // Sprawdzamy czy ten produktów jest już w koszytku
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                // w zaleznosci od tego czy produkt jest w koszyku go dodajemy lub zwiększamy ilość
                if (productInCart == null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                else
                {
                    productInCart.Quantity++;
                }
            }
            // Pobieramy całkowite wartość ilosci i ceny i dodajemy do modelu
            int Qty = 0;
            decimal Price = 0;

            foreach (var item in cart)
            {
                Qty += item.Quantity;
                Price += item.Quantity * item.Price;
            }
            model.Quantity = Qty;
            model.Price = Price;

            // zapis w sesii
            Session["cart"] = cart;


            return PartialView(model);
        }

        public JsonResult IncrementProduct(int productId)
        {
            // Inicjalizacja list CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Pobieramy CartVm
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // zwiększamy ilość produku
            model.Quantity++;

            // Przygotowanie danych do JSONA
            var result = new
            {
                qty = model.Quantity,
                price = model.Price
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DecrementProduct(int productid)
        {
            // Inicjalizacja list CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Pobieramy CartVm
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productid);

            // zmiejszamyilość produku
            if(model.Quantity > 1)
            {
                model.Quantity--;
            }
            else
            {
                model.Quantity = 0;
                cart.Remove(model);
            }

            // Przygotowanie danych do JSONA
            var result = new
            {
                qty = model.Quantity,
                price = model.Price
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Removeproduct(int productId)
        {

            // Inicjalizacja list CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM>;


            // Pobieramy CartVm
            CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

            // usuwamy produkt
            cart.Remove(model);
            return View();
        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            return PartialView(cart);
        }
        [HttpPost]
        public void PlaceOrder()
        {
            // pobieramy zawarosc koszyka ze zmiennje session
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // pobieramy nazwe uzytkownika
            string username = User.Identity.Name;

            // deklaracja OrderId
            int orderId = 0;

            using (DB db = new DB())
            {
                // Inicjalizacaja OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // pobieramy UserID
                var user = db.Users.FirstOrDefault(x => x.UserName == username);
                int userId = user.Id;
                // ustawienie orderDTO i zapis
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                // pobieramy id zapisanego zamowienia 
                orderId = orderDTO.OrderId;

                // inicjalizacja OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();
                
                foreach(var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }

            // wysylanie emaila do admina
            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("", ""),
                EnableSsl = true
            };
            client.Send("", "", "", "" + orderId);
            // reset session
            Session["cart"] = null;
        }
    }
}