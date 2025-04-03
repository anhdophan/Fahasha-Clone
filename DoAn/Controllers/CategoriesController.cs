using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAn.Models;

namespace DoAn.Controllers
{
    public class CategoriesController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        // GET: Categories
      
       
        public PartialViewResult CategoryPartial2()
        {
            var cateList = database.Categories.ToList();
            return PartialView(cateList);
        }
        public ActionResult Index()
        {
            return View(database.Categories.ToList());
        }
        [HttpPost]
        public ActionResult Index(string _name)
        {
            if (_name == null)
                return View(database.Categories.ToList());
            else
                return View(database.Categories.Where(s => s.NameCate == _name).ToList());
        }
        public ActionResult Create()
        {
            if (Session["isAdminLoggedIn"] != null && (bool)Session["isAdminLoggedIn"])
            {
                string roleAdmin = (string)Session["Role"];
                if (roleAdmin == "admin")
                {
                    // Role Admin
                    Category cate = new Category();
                    return View(cate);
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
        public ActionResult Create(Category cate)
        {
            try
            {
                if (cate.UploadImage != null)
                {
                    string filename = Path.GetFileNameWithoutExtension(cate.UploadImage.FileName);
                    string extent = Path.GetExtension(cate.UploadImage.FileName);
                    filename = filename + extent;
                    cate.ImgCate = "~/Content/images/" + filename;
                    cate.UploadImage.SaveAs(Path.Combine(Server.MapPath("~/Content/images/"), filename));
                }
                database.Categories.Add(cate);
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
                    return View(database.Categories.Where(s => s.Id == id).FirstOrDefault());
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
        public ActionResult Edit(Category cate, int id)
        {
            database.Entry(cate).State = System.Data.Entity.EntityState.Modified;
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
                    return View(database.Categories.Where(s => s.Id == id).FirstOrDefault());
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
        public ActionResult Delete(int id, Category cate)
        {

            try
            {
                cate = database.Categories.Where(s => s.Id == id).FirstOrDefault();
                database.Categories.Remove(cate);
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