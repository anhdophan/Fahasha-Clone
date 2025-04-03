using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class HomeController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        

        public PartialViewResult Test()
        {
            return PartialView();
        }

        public PartialViewResult Cate()
        {
            var cateList = database.Categories.ToList();
            return PartialView(cateList);
        }
    }
}