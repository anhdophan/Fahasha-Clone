
using DoAn.Models;
using System;
using System.Collections.Generic;
using System.EnterpriseServices.CompensatingResourceManager;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.AspNetCore.Http;
using PagedList;
using PagedList.Mvc;
using System.Web.UI;
using System.Globalization;
using System.Text;

namespace DoAn.Controllers
{
    public class ProductController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        // GET: Product
 
        public ActionResult Index(string _name)
        {
            if (_name == null)
            {
                var productList = database.Products.OrderByDescending(x => x.NamePro);

                return View(productList);
            }
            else
            {
                var productList = database.Products.OrderByDescending(x => x.NamePro)
                    .Where(x => x.Category == _name);
                return View(productList);
            }

        }

        [HttpGet]
        public ActionResult Search(string _name)
        {
            if (string.IsNullOrEmpty(_name))
            {
                return View(new List<Product>());
            }

            // Chuẩn hóa chuỗi tìm kiếm
            string searchTerm = ConvertToUnSign(_name.ToLower());

            var productList = database.Products
                .AsEnumerable() // Chuyển sang xử lý trên C# thay vì SQL
                .Where(p => ConvertToUnSign(p.NamePro.ToLower()).Contains(searchTerm))
                .ToList();

            return View(productList);
        }

        // Hàm chuyển đổi chuỗi thành dạng không dấu
        public static string ConvertToUnSign(string text)
        {
            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }


        public ActionResult Pro_Cate(string category, string publisher, string supplier, int? page, double min = double.MinValue, double max = double.MaxValue)
        {
            int pageSize = 10;
            int pageNum = (page ?? 1);

            if (category == null && publisher == null && supplier == null)
            {
                var productList = database.Products.OrderByDescending(x => x.NamePro);
                return View(productList.ToPagedList(pageNum, pageSize));
            }
            if(category !=null && publisher == null && supplier == null)
            {
                var productList = database.Products.OrderByDescending(x => x.NamePro)
                    .Where(x => x.Category == category)
                    .ToPagedList(pageNum, pageSize);
                return View(productList);
            }
            if(category == null && publisher != null && supplier == null)
            {
                var productList = database.Products.OrderByDescending(x => x.NamePro)
                   .Where(x => x.Publisher == publisher)
                   .ToPagedList(pageNum, pageSize);
                return View(productList);
            }
            else
            {
                var productList = database.Products.OrderByDescending(x => x.NamePro)
                   .Where(x => x.Supplier == supplier)
                   .ToPagedList(pageNum, pageSize);
                return View(productList);
            }
        }

        public ActionResult QuanlySP(string category, string searchTerm, int? page, double min = double.MinValue, double max = double.MaxValue)
        {
            int pageSize = 4;
            int pageNum = (page ?? 1);

            var productList = database.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                productList = productList.Where(p => p.NamePro.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(category))
            {
                productList = productList.Where(p => p.Category == category);
            }

            return View(productList.OrderByDescending(x => x.NamePro).ToPagedList(pageNum, pageSize));
        }

        public ActionResult Create()
        {
            List<Category> list = database.Categories.ToList();
            List<Publisher> listpub = database.Publishers.ToList();
            List<Supplier> listsup = database.Suppliers.ToList();
            ViewBag.listCategory = new SelectList(list, "IDCate", "NameCate", "");
            ViewBag.listPublisher = new SelectList(listpub, "IDPub", "NamePub", "");
            ViewBag.listSupplier = new SelectList(listsup, "IDSup", "NameSup", "");
            Product pro = new Product();
            return View(pro);
        }
        [HttpPost]
        public ActionResult Create(Product pro)
        {
            // Retrieve lists to repopulate dropdowns in case of error
            List<Category> list = database.Categories.ToList();
            List<Publisher> listpub = database.Publishers.ToList();
            List<Supplier> listsup = database.Suppliers.ToList();

            ViewBag.listCategory = new SelectList(list, "IDCate", "NameCate", pro.Category);
            ViewBag.listPublisher = new SelectList(listpub, "IDPub", "NamePub", pro.Publisher);
            ViewBag.listSupplier = new SelectList(listsup, "IDSup", "NameSup", pro.Supplier);

            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                // Aggregate all error messages
                var errors = string.Join("<br/>", ModelState.Values
                                           .SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = errors;
                return RedirectToAction("QuanlySP");
            }

            try
            {
                if (pro.UploadImage != null)
                {
                    string filename = Path.GetFileNameWithoutExtension(pro.UploadImage.FileName);
                    string extent = Path.GetExtension(pro.UploadImage.FileName);
                    filename = filename + extent;
                    pro.ImagePro = "~/Content/images/" + filename;
                    pro.UploadImage.SaveAs(Path.Combine(Server.MapPath("~/Content/images/"), filename));
                }
                else
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn ảnh sản phẩm.";
                    return View(pro);
                }

                database.Products.Add(pro);
                database.SaveChanges();

                TempData["SuccessMessage"] = $"Đã tạo sản phẩm thành công";
                return RedirectToAction("QuanlySP");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo sản phẩm: " + ex.Message;
                return RedirectToAction("QuanlySP");
            }
        }



        public ActionResult Details(int id)
        {
            var relatedProducts = database.Products.Where(s => s.ProductID == id).FirstOrDefault();
            var category = database.Products
                 .Where(p => p.ProductID == id)
                 .Select(p => p.Category)
                 .FirstOrDefault();
            Session["category"]=category;
            return View(relatedProducts);
        }
        public ActionResult DetailsAdmin(int id)
        { 
            return View(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }
        public ActionResult Edit(int id)
        {
            List<Category> list = database.Categories.ToList();
            List<Publisher> listpub = database.Publishers.ToList();
            List<Supplier> listsup = database.Suppliers.ToList();
            ViewBag.listCategory = new SelectList(list, "IDCate", "NameCate", "");
            ViewBag.listPublisher = new SelectList(listpub, "IDPub", "NamePub", "");
            ViewBag.listSupplier = new SelectList(listsup, "IDSup", "NameSup", "");

            // Remove any existing ModelState entry for 'Supplier'
            ModelState.Remove("Publisher");
            ModelState.Remove("Supplier");
            ModelState.Remove("Category");

            return View(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }

        [HttpPost]
        public ActionResult Edit(Product pro, int id)
        {
            // Load the original product from the database
            var product = database.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            List<Category> list = database.Categories.ToList();
            List<Publisher> listpub = database.Publishers.ToList();
            List<Supplier> listsup = database.Suppliers.ToList();
            ViewBag.listCategory = new SelectList(database.Categories, "IDCate", "NameCate", product.Category);
            ViewBag.listPublisher = new SelectList(database.Publishers, "IDPub", "NamePub", product.Publisher);
            ViewBag.listSupplier = new SelectList(database.Suppliers, "IDSup", "NameSup", product.Supplier);

            // Update only the editable properties
            product.NamePro = pro.NamePro;
            product.DecriptionSmall = pro.DecriptionSmall;
            product.DescriptionBig = pro.DescriptionBig;
            product.Category = pro.Category;
            product.Price = pro.Price;
            product.Author = pro.Author;
            product.Supplier = pro.Supplier;
            product.Publisher = pro.Publisher;
            product.Publishingyear = pro.Publishingyear;
            product.C_Language = pro.C_Language;
            product.Series = pro.Series;
            product.Quantity = pro.Quantity;

            // Update image if a new image is uploaded
            if (pro.UploadImage != null)
            {
                string filename = Path.GetFileNameWithoutExtension(pro.UploadImage.FileName);
                string extension = Path.GetExtension(pro.UploadImage.FileName);
                filename = filename + extension;
                string filePath = Path.Combine(Server.MapPath("~/Content/images/"), filename);

                // Lưu ảnh vào thư mục
                pro.UploadImage.SaveAs(filePath);

                // Chỉ cập nhật nếu có ảnh mới
                product.ImagePro = "~/Content/images/" + filename;
            }

            try
            {
                database.SaveChanges();
                TempData["SuccessMessage"] = $"Đã cập nhật sản phẩm {product.NamePro} thành công!";
                return RedirectToAction("QuanlySP");
            }
            catch (Exception ex)
            {
                ViewBag.listCategory = new SelectList(database.Categories, "IDCate", "NameCate", product.Category);
                ViewBag.listPublisher = new SelectList(database.Publishers, "IDPub", "NamePub", product.Publisher);
                ViewBag.listSupplier = new SelectList(database.Suppliers, "IDSup", "NameSup", product.Supplier);

                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật sản phẩm: " + ex.Message;
                return RedirectToAction("QuanlySP");
            }

        }
        public ActionResult Delete(int id)
        {
            return View(database.Products.Where(s => s.ProductID == id).FirstOrDefault());
        }
        [HttpPost]
        public ActionResult Delete(int id, Product cate)
        {
            try
            {
                cate = database.Products.Where(s => s.ProductID == id).FirstOrDefault();
                database.Products.Remove(cate);
                database.SaveChanges();
                return RedirectToAction("QuanlySP");
            }
            catch
            {
                return Content("Sản phẩm này đang được đặt hàng. Xóa thất bại!");
            }
           
          
        }
        public PartialViewResult SuggestedProducts()
        {
            // kiểm tra đăng nhập
            if (Session["IdCustomer"] == null)
            {
                return PartialView(new List<Product>());
            }

            int userId = (int)Session["IdCustomer"];

            // lấy số lượng sản phẩm đã mua từ các đơn hàng để chọn loại sản phẩm dc mua nhiều nhất dựa theo đơn hàng của khách hàng
            var categoryCounts = (from op in database.OrderProes
                                  where op.IDCus == userId
                                  join od in database.OrderDetails on op.ID equals od.IDOrder
                                  join p in database.Products on od.IDProduct equals p.ProductID
                                  group od by p.Category into g
                                  select new { Category = g.Key, TotalQuantity = g.Sum(x => x.Quantity ?? 0) })
                                 .OrderByDescending(x => x.TotalQuantity)
                                 .ToList();

            // lấy 2 loại sản phẩm dc mua nhiều nhất
            var topCategories = categoryCounts.Take(2).Select(x => x.Category).ToList();

            if (topCategories.Count == 0)
            {
                //nếu người dùng chưa có đơn hàng nào thì sẽ lấy top 10 sản phẩm được mua nhiều nhất
                var topProducts = database.Products
                                    .OrderByDescending(p => p.SoldQuantity ?? 0)
                                    .Take(10)
                                    .ToList();
                return PartialView(topProducts);
            }
            else
            {
                // lấy loại sản phẩm dc mua nhiều nhất bởi tài khoản đăng nhập
                var suggestedProducts = database.Products
                                          .Where(p => topCategories.Contains(p.Category))
                                          .ToList();
                return PartialView(suggestedProducts);
            }
        }


        public ActionResult SelectCate()
        {
            Category se_cate = new Category();
            se_cate.ListCate = database.Categories.ToList<Category>();
            return PartialView(se_cate);
        }
        public ActionResult SelectSup()
        {
            Supplier se_sup = new Supplier();
            se_sup.ListSup = database.Suppliers.ToList<Supplier>();
            return PartialView(se_sup);
        }


        public PartialViewResult Recomment()
        {
            string cate = (string)Session["category"];
            var productList = database.Products.OrderByDescending(x => x.NamePro)
                   .Where(x => x.Category == cate);
            return PartialView(productList);
        }

    }
}