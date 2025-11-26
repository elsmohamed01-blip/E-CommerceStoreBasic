using E_Commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspSooQcom.Controllers
{

    /// <summary>
    /// الكنترولر الرئيسي للموقع – يعالج جميع عمليات المتجر مثل عرض المنتجات، الأقسام، السلة، والطلبات.
    /// Main storefront controller – handles product listing, categories, shopping cart and orders.
    /// </summary>
    public class HomeController : Controller
    {
        ECommerceStoreContext db = new ECommerceStoreContext();


        // ============================================================
        //                        Home / Index
        // ============================================================

        /// <summary>
        /// الصفحة الرئيسية – تحميل الأقسام، المنتجات، أحدث المنتجات، والمراجعات.
        /// Home page – loads categories, products, latest items, and customer reviews.
        /// </summary>
        public IActionResult Index()
        {

            IndexVM result = new IndexVM();

            result.Cateogries = db.Catoegries.ToList();
            result.Products = db.Products.ToList();
            result.Reviews = db.Reviews.ToList();
            result.LatesProducts = db.Products.OrderByDescending(x => x.Price).Take(8).ToList();

            return View(result);
        }
        // ============================================================
        //                        Product Search
        // ============================================================

        /// <summary>
        /// البحث عن منتج باستخدام الاسم المدخل من المستخدم.
        /// Search for products by a name string entered by the user.
        /// </summary>
        [HttpGet]
        public IActionResult ProductSearch(string xname)
        {
            var Products = new List<Product>();

            if (string.IsNullOrEmpty(xname))
            {
                Products = db.Products.ToList();
            }
            else
                Products = db.Products.Where(x => x.Name.Contains(xname)).ToList();

            return View(Products);
        }

        // ============================================================
        //                    Categories & Products
        // ============================================================

        /// <summary>
        /// عرض جميع الأقسام المتاحة.
        /// Displays all available categories.
        /// </summary>
        public IActionResult Cateogry()
        {
            var cato = db.Catoegries.ToList();


            return View(cato);
        }

        /// <summary>
        /// عرض المنتجات الخاصة بقسم معيّن.
        /// Displays products that belong to a specific category.
        /// </summary>
        public IActionResult Product(int id)
        {
            var cato = db.Products.Where(x => x.CatId == id).ToList();

            return View(cato);
        }

        // ============================================================
        //                          Reviews
        // ============================================================

        /// <summary>
        /// إرسال تقييم من العميل وحفظه في قاعدة البيانات.
        /// Submit customer review and save it into the database.
        /// </summary>

        [HttpGet]
        public IActionResult Sendreview(string Name, string email, string adress, string mobill, string description)
        {
            {
                db.Reviews.Add(new Review { Name = Name, Email = email, Adress = adress, Mobill = mobill, Description = description });
                db.SaveChanges();

                return RedirectToAction("Index");
            }
        }
        /// <summary>
        /// عرض تفاصيل منتج واحد بما يشمل الصور والقسم.
        /// Shows full details of a product including images and category.
        /// </summary>
        public IActionResult ProductShow(int id)
        {
            var cato = db.Products.Include(x => x.Cat).Include(x => x.ProductImages).FirstOrDefault(x => x.Id == id);

            return View(cato);
        }

        // ============================================================
        //                          Cart
        // ============================================================

        /// <summary>
        /// عرض سلة المشتريات الخاصة بالمستخدم الحالي.
        /// Displays the shopping cart for the currently logged-in user.
        /// </summary>
        [Authorize]
        public IActionResult Cart()
        {
            var result = db.Carts.Include(x => x.Product).Where(x => x.UserId == User.Identity.Name).ToList();
            return View(result);
        }

        /// <summary>
        /// إضافة منتج للسلة أو زيادة كميته إذا كان موجودًا بالفعل.
        /// Adds a product to the cart or increases its quantity if it already exists.
        /// </summary>
        [Authorize]
        public IActionResult AddCart(int id)
        {
            var price = db.Products.Find(id).Price;


            var item = db.Carts.FirstOrDefault(x => x.ProductId == id && x.UserId == User.Identity.Name);
            if (item != null)

                item.Qty += 1;

            else
                db.Carts.Add(new Cart { ProductId = id, UserId = User.Identity.Name, Qty = 1, Price = price });
            db.SaveChanges();

            return RedirectToAction("Index");


        }

        // ============================================================
        //                          Orders
        // ============================================================

        /// <summary>
        /// إنشاء طلب جديد من محتويات السلة ثم إرسال إشعار للواتساب.
        /// Creates a new order from cart items and sends a WhatsApp notification.
        /// </summary>  /// 
        
        [HttpPost]
        [Authorize]
        public IActionResult AddOrder(Order model)
        {

            var order = new Order
            {
                Name = model.Name,
                Aderss = model.Aderss,
                Email = model.Email,
                Mobile = model.Mobile,
                DataTime = model.DataTime,
                IsonlineParid = model.IsonlineParid,
                UserId = User.Identity.Name
            };

            var cartItems = db.Carts.Where(x => x.UserId == User.Identity.Name).ToList();

            if (!cartItems.Any())
                return RedirectToAction(nameof(Cart));

            int total = 0;

            foreach (var item in cartItems)
            {
                order.AddorderDeatils.Add(new AddorderDeatil
                {
                    Qty = item.Qty,
                    Price = item.Price,
                    Productid = item.ProductId,
                    Totalprice = item.Price * item.Qty
                });

                total += (int)(item.Qty * item.Price);
            }

            db.Orders.Add(order);
            db.Carts.RemoveRange(cartItems);
            db.SaveChanges();

            // Build WhatsApp message
            StringBuilder sb = new();
            sb.AppendLine($"طلب جديد من عميل: {order.Name}");
            sb.AppendLine($"رقم الهاتف: {order.Mobile}");
            sb.AppendLine($"البريد الالكتروني: {order.Email}");
            sb.AppendLine($"العنوان: {order.Aderss}");
            sb.AppendLine($"تاريخ الطلب: {order.DataTime}");
            sb.AppendLine("\n*تفاصيل الطلب*\n--------------------");

            foreach (var item in cartItems)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                string name = product?.Name ?? "منتج غير معروف";
                int subtotal = (int)(item.Qty * item.Price);

                sb.AppendLine($"- {name} × {item.Qty} = {subtotal} جنيه");
            }

            sb.AppendLine("--------------------");
            sb.AppendLine($"الإجمالي الكلي: {total} جنيه");

            string encodedMessage = Uri.EscapeDataString(sb.ToString());
            string sellerPhone = "201029638961";
            string whatsappUrl = $"https://api.whatsapp.com/send?phone={sellerPhone}&text={encodedMessage}";

            return Redirect(whatsappUrl);
        }

        /// <summary>
        /// إزالة منتج واحد من السلة.
        /// Removes a specific product from the user's shopping cart.
        /// </summary> /// 

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var item = db.Carts.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                db.Carts.Remove(item);
                db.SaveChanges();
            }

            return RedirectToAction("Cart"); // إعادة توجيه إلى صفحة السلة بعد الحذف
        }

        /// <summary>
        /// عرض كل الطلبات الخاصة بالمستخدم.
        /// Displays all orders that belong to the logged-in user.
        /// </summary> /// 
      
        [Authorize]
        public IActionResult Orders()
        {
            var order = db.Orders.Include(x => x.AddorderDeatils).ThenInclude(x => x.Product).Where(x => x.UserId == User.Identity.Name).ToList();

            return View(order);
        }
        public IActionResult EndOrsers()
        {
            return View();
        }
        /// <summary>
        /// إضافة منتج بكمية محددة للسلة.
        /// Adds a product to the cart with a specific quantity.
        /// </summary>
        [HttpPost]
        [Authorize]
        public IActionResult AddToCart(int id, int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest("يجب أن تكون الكمية أكبر من صفر");
            }

            var product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound("المنتج غير موجود.");
            }

            var item = db.Carts.FirstOrDefault(x => x.ProductId == id && x.UserId == User.Identity.Name);
            if (item != null)
            {
                // تحديث الكمية إذا كان المنتج موجوداً في السلة
                item.Qty += quantity;
            }
            else
            {
                // إضافة المنتج الجديد إلى السلة
                db.Carts.Add(new Cart
                {
                    ProductId = id,
                    UserId = User.Identity.Name,
                    Qty = quantity,
                    Price = product.Price
                });
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
