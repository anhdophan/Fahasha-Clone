using DoAn.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class SupplierController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        // GET: Categories

        public PartialViewResult SupplierPartial()
        {
            var supList = database.Suppliers.ToList();
            return PartialView(supList);
        }
        // GET: Supplier
        public ActionResult Index()
        {
            return View(database.Suppliers.ToList());
        }
        [HttpPost]
        public ActionResult Index(string _name)
        {
            if (_name == null)
                return View(database.Suppliers.ToList());
            else
                return View(database.Suppliers.Where(s => s.NameSup == _name).ToList());
        }
        public ActionResult Create()
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "admin")
                {
                    // Role Admin
                    Supplier sup = new Supplier();
                    return View(sup);
                }
                else
                {
                    // Sai role
                    return RedirectToAction("ErrorRole");
                }
            }
            else
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Index", "LoginUser");
            }

        }
        [HttpPost]
        public ActionResult Create(Supplier sup)
        {
            try
            {
               
                database.Suppliers.Add(sup);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return Content("Error Create New");
            }
        }
     
        public ActionResult Edit(int id)
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "admin")
                {
                    // Role Admin
                    return View(database.Suppliers.Where(s => s.Id == id).FirstOrDefault());
                }
                else
                {
                    // Sai role
                    return RedirectToAction("ErrorRole");
                }
            }
            else
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Index", "LoginUser");
            }

        }
        [HttpPost]
        public ActionResult Edit(Supplier sup, int id)
        {
            database.Entry(sup).State = System.Data.Entity.EntityState.Modified;
            database.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Delete(int id)
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "admin")
                {
                    return View(database.Suppliers.Where(s => s.Id == id).FirstOrDefault());
                }
                else
                {
                    // Sai role
                    return RedirectToAction("ErrorRole");
                }
            }
            else
            {
                // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Index", "LoginUser");
            }

        }
        [HttpPost]
        public ActionResult Delete(int id, Supplier sup)
        {
            try
            {
                sup = database.Suppliers.Where(s => s.Id == id).FirstOrDefault();
                database.Suppliers.Remove(sup);
                database.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return Content("This data is using in other table, Error Delete!");
            }
        }
    }
}