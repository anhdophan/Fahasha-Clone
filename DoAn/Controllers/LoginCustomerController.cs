using DoAn.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices.CompensatingResourceManager;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class LoginCustomerController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        // GET: LoginUser : Form Login
        public ActionResult CheckLoginStatus()
        {
            // Kiểm tra xem Session "isLoggedIn" có tồn tại hay không
            if (Session["isLoggedIn"] != null && (bool)Session["isLoggedIn"])
            {
                // Người dùng đã đăng nhập, chuyển hướng đến trang thông tin cá nhân
                return RedirectToAction("ThongTinCaNhan", "LoginCustomer");
            }
            else
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Index", "LoginCustomer");
            }
        }

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoginAcount(Customer _user)
        {
            var check = database.Customers.Where(s => s.PhoneCus == _user.PhoneCus && s.PasswordUser == _user.PasswordUser).FirstOrDefault();
           
            if (check == null) //login sai thong tin
            {
                ViewBag.ErrorInfo = "SaiInfo";
                return View("Index");
            }
            if (check.PhoneCus == "admin" && check.PasswordUser == "admin123")
            {
                return RedirectToAction("Index", "LoginUser");
            }
            else 
            {
                database.Configuration.ValidateOnSaveEnabled = false;
               int _userId = check.IDCus;
                Session["Customer"] = check;
                Session["IdCustomer"] = _userId;
                Session["isLoggedIn"] = true;
                Session["PasswordUser"] = _user.PasswordUser;
                return RedirectToAction("Index", "Product");
            }      
        }
        public ActionResult LogOutUser()
        {
            Session.Abandon();
            return RedirectToAction("Index", "LoginCustomer");
        }

        public ActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterUser(Customer _user)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem số điện thoại đã được đăng ký chưa
                var check = database.Customers.FirstOrDefault(s => s.PhoneCus == _user.PhoneCus);
                if (check == null) // Số điện thoại chưa được đăng ký
                {
                    database.Configuration.ValidateOnSaveEnabled = false;
                    database.Customers.Add(_user);
                    database.SaveChanges();

                    // Lưu thông báo thành công vào TempData
                    TempData["SuccessMessage"] = $"Đã đăng ký tài khoản với số điện thoại {_user.PhoneCus} thành công";
                    return RedirectToAction("RegisterUser");
                }
                else
                {
                    // Lưu thông báo lỗi vào TempData
                    TempData["ErrorMessage"] = "Số điện thoại đã được đăng ký.";
                    return RedirectToAction("RegisterUser");
                }
            }
            // Nếu ModelState không hợp lệ, trả về view với _user để hiển thị các lỗi của ModelState
            return View(_user);
        }




        public ActionResult ThongTinCaNhan()
        {
            int id = (int)Session["IdCustomer"];
            Customer customer = database?.Customers?.FirstOrDefault(c => c.IDCus == id);
            return View(customer);
        }


        public ActionResult History(string paymentMethod, string status)
        {
            int id = (int)Session["IdCustomer"];
            var orders = database.OrderProes.Where(s => s.IDCus == id).AsQueryable();

            // Lọc theo phương thức thanh toán dựa trên thuộc tính Status
            // Nếu paymentMethod = "COD" thì chỉ chọn đơn hàng có Status == true
            // Nếu paymentMethod = "MoMo" thì chỉ chọn đơn hàng có Status == false
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                if (paymentMethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.Where(o => o.Status == true);
                }
                else if (paymentMethod.Equals("MoMo", StringComparison.OrdinalIgnoreCase))
                {
                    orders = orders.Where(o => o.Status == false);
                }
            }

            // Lọc theo tình trạng đơn hàng (TinhTrang)
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.TinhTrang == status);
            }

            // Lưu giá trị lọc vào ViewBag để view hiển thị giá trị đã chọn
            ViewBag.SelectedPaymentMethod = paymentMethod;
            ViewBag.SelectedStatus = status;

            return View(orders.ToList());
        }

        public ActionResult UpdateOrderStatus(int id, string status)
        {
            // Lấy IdCustomer từ session
            int customerId = (int)Session["IdCustomer"];
            // Chỉ cho phép cập nhật đơn hàng của chính khách hàng đó
            var order = database.OrderProes.FirstOrDefault(o => o.ID == id && o.IDCus == customerId);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Cập nhật trạng thái đơn hàng
            order.TinhTrang = status;
            database.SaveChanges();

            // Sau khi cập nhật, chuyển hướng về trang History
            return RedirectToAction("History");
        }

        public ActionResult DetailPro(int id)
        {
            return View(database.OrderDetails.Where(s => s.IDOrder == id).FirstOrDefault());
        }
        public ActionResult UpdateOrder(int id)
        {
            var order = database.OrderProes.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        [HttpPost]
        public ActionResult UpdateOrder(OrderPro updatedOrder)
        {
            var order = database.OrderProes.Find(updatedOrder.ID);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Update fields
            order.NameCus = updatedOrder.NameCus;
            order.PhoneCus = updatedOrder.PhoneCus;
            order.AddressDeliverry = updatedOrder.AddressDeliverry;

            database.SaveChanges();
            return RedirectToAction("History");
        }

        public ActionResult RequestCancelOrder(int id)
        {
            var order = database.OrderProes.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            order.TinhTrang = "Yêu Cầu Hủy";
            database.SaveChanges();

            return RedirectToAction("History");
        }

        public ActionResult RequestCancelOrder2(int id)
        {
            var order = database.OrderProes.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            order.TinhTrang = "Đang Xác Nhận";
            database.SaveChanges();

            return RedirectToAction("History");
        }






        //        public ActionResult EditPersonalInfo()
        //        {
        //            int id = (int)Session["IdCustomer"];
        //            if (id == 0) // or another appropriate check to ensure session is valid
        //            {
        //                return RedirectToAction("Login"); // Redirect if session is invalid
        //            }

        //            var customer = database.Customers.FirstOrDefault(c => c.IDCus == id);
        //            if (customer == null)
        //            {
        //                return HttpNotFound(); // Return 404 if customer is not found
        //            }

        //            return View(customer);
        //        }


        //        // POST: LoginCustomer/EditPersonalInfo
        //        [HttpPost]
        //        [ValidateAntiForgeryToken]
        //        public ActionResult EditPersonalInfo(Customer updatedCustomer)
        //        {
        //            if (ModelState.IsValid)
        //            {
        //                var customer = database.Customers.Find(updatedCustomer.IDCus);
        //                if (customer == null)
        //                {
        //                    return HttpNotFound();
        //                }

        //                // Update fields
        //                customer.NameCus = updatedCustomer.NameCus;
        //                customer.PhoneCus = updatedCustomer.PhoneCus;
        //                customer.EmailCus = updatedCustomer.EmailCus;
        //                customer.Address = updatedCustomer.Address;

        //                database.SaveChanges();
        //                return RedirectToAction("ThongTinCaNhan"); // Redirect to personal info page
        //            }

        //            return View(updatedCustomer); // Return to the form if validation fails
        //        }


    }
}