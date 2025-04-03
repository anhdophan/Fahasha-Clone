using DoAn.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace DoAn.Controllers
{
    public class ProductDetailController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        // GET: ProductDetail
        public ActionResult DetailUp(int id)
        {
            return PartialView(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }
        public ActionResult DetailDown(int id)
        {
            return PartialView(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }

      

    }
}