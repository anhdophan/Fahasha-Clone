    using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class LoginUserController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        // GET: LoginUser : Form Login

        public ActionResult CheckLoginStatus()
        {
            // Kiểm tra xem Session "isAdminLoggedIn" có tồn tại hay không
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                // Người dùng đã đăng nhập, chuyển hướng đến trang danh sách người dùng
                return RedirectToAction("Userlist", "LoginUser");
            }
            else
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Index", "LoginUser");
            }
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoginAcount(AdminUser _user)
        {
            var check = database.AdminUsers.Where(s => s.ID == _user.ID && s.PasswordUser == _user.PasswordUser).FirstOrDefault();
            if (check == null) //login sai thong tin
            {
                ViewBag.ErrorInfo = "SaiInfo";
                return View("Index");
            }
            else
            {
                database.Configuration.ValidateOnSaveEnabled = false;
                int _userId = check.ID;
                Session["IdAdmin"] = _userId;
                Session["Role"] = check.RoleUser;
                Session["isAdminLoggedIn"] = true;
                Session["PasswordUser"] = _user.PasswordUser;
                return RedirectToAction("QuanLySP", "Product");
            }
        }
        
        public ActionResult LogOutUser()
        {
            Session.Abandon();
            return RedirectToAction("Index", "LoginUser");
        }

        public ActionResult RegisterUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisterUser(AdminUser _user)
        {
            if (ModelState.IsValid)
            {
                var check_ID = database.AdminUsers.Where(s => s.ID == _user.ID).FirstOrDefault();
                if (check_ID == null)   //chua co ID
                {
                    database.Configuration.ValidateOnSaveEnabled = false;
                    database.AdminUsers.Add(_user);
                    database.SaveChanges();
                    return RedirectToAction("QuanLySP", "Product");
                }
                else
                {
                    ViewBag.ErrorRegister = "This ID is exist";
                    return View();
                }
            }
            return View();
        }
        public ActionResult ErrorRole()
        {
            return View();
        }
        public ActionResult History(string paymentMethod, string status)
        {
            
                    var orders = database.OrderProes.AsQueryable();

                    // Lọc theo phương thức thanh toán dựa trên thuộc tính Status
                    // Nếu paymentMethod = "COD" thì Status phải true
                    // Nếu paymentMethod = "MoMo" thì Status phải false
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

                    // Lưu giá trị lọc vào ViewBag để hiển thị trong dropdown ở view
                    ViewBag.SelectedPaymentMethod = paymentMethod;
                    ViewBag.SelectedStatus = status;

                    return View(orders.ToList());
               
        }

        public ActionResult UpdateTinhTrang(int id, string status)
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "Admin")
                {
                    var order = database.OrderProes.Find(id);
                    if (order != null)
                    {
                        // Update the status
                        order.TinhTrang = status;
                        database.SaveChanges(); // Save changes to the database
                    }

                    // After updating, redirect back to the History view
                    return RedirectToAction("History");
                }
                else
                {
                    return RedirectToAction("ErrorRole");
                }
            }
            else
            {
                return RedirectToAction("Index", "LoginUser");
            }
        }

        public ActionResult DetailPro(int id)
        {
           
               
                    return View(database.OrderDetails.Where(s => s.IDOrder == id).FirstOrDefault());
                
            
           
        }
        public ActionResult SetRole(int id)
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "Admin")
                {
                    var user = database.AdminUsers.Find(id);
                    if (user != null)
                    {
                        ViewBag.UserId = id;
                        ViewBag.CurrentRole = user.RoleUser;  // Pass current role to the view
                        return View(user);  // Pass the user object to the view
                    }
                    else
                    {
                        return RedirectToAction("Userlist");  // Redirect to user list if user not found
                    }
                }
                else
                {
                    return RedirectToAction("ErrorRole");
                }
            }
            else
            {
                return RedirectToAction("Index", "LoginUser");
            }
        }
        [HttpPost]
        public ActionResult UpdateRole(int id, string roleUser)
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "Admin")
                {
                    var user = database.AdminUsers.Find(id);
                    if (user != null)
                    {
                        user.RoleUser = roleUser;  // Update the role
                        database.SaveChanges();    // Save changes to the database
                    }

                    // After updating, redirect back to the Userlist view
                    return RedirectToAction("Userlist");
                }
                else
                {
                    return RedirectToAction("ErrorRole");
                }
            }
            else
            {
                return RedirectToAction("Index", "LoginUser");
            }
        }



        public ActionResult Customer()
        {
            return View(database.Customers.ToList());
        }
        public ActionResult OrderCus(int id)
        {
            return View(database.OrderProes.Where(s => s.IDCus == id).ToList());
        }

        public ActionResult Userlist()
        {
            return View(database.AdminUsers.ToList());
        }

      
        public ActionResult Product()
        {
            return View(database.Products.ToList());
        }

    }
}