using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class PublisherController : Controller
    {
        // GET: Publisher
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();

        public PartialViewResult PublisherPartial()
        {
            var pubList = database.Publishers.ToList();
            return PartialView(pubList);
        }
        public ActionResult Index()
        {
            return View(database.Publishers.ToList());
        }
        [HttpPost]
        public ActionResult Index(string _name)
        {
            if (_name == null)
                return View(database.Publishers.ToList());
            else
                return View(database.Publishers.Where(s => s.NamePub == _name).ToList());
        }
        public ActionResult Create()
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "admin")
                {
                    // Role Admin
                    Publisher pub = new Publisher();
                    return View(pub);
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
        public ActionResult Create(Publisher pub)
        {
            try
            {

                database.Publishers.Add(pub);
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
                    return View(database.Publishers.Where(s => s.ID == id).FirstOrDefault());
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
        public ActionResult Edit(Publisher pub, int id)
        {
            database.Entry(pub).State = System.Data.Entity.EntityState.Modified;
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
                    return View(database.Publishers.Where(s => s.ID == id).FirstOrDefault());
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
        public ActionResult Delete(int id, Publisher pub)
        {
            try
            {
                pub = database.Publishers.Where(s => s.ID == id).FirstOrDefault();
                database.Publishers.Remove(pub);
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