using DoAn.Controllers.MoMoPayment.model;
using DoAn.Controllers.MoMoPayment.Order;
using DoAn.Controllers.MoMoPayment.Services;
using DoAn.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DoAn.Controllers
{
    public class ShoppingCartController : Controller
    {
        DoAnWebSachEntities2 database = new DoAnWebSachEntities2();
        private readonly IMomoService _momoService;

        // Constructor của controller, khởi tạo _momoService với đối số cần thiết
        public ShoppingCartController()
        {
            // Tạo đối tượng cấu hình cho MoMo
            var momoOptions = new MomoOptionModel
            {
                PartnerCode = "MOMO",
                AccessKey = "F8BBA842ECF85",
                RequestType = "captureMoMoWallet",
                NotifyUrl = "http://localhost:51529/MoMoHome/MomoNotify",
                ReturnUrl = "http://localhost:51529/ShoppingCart/PaymentSuccess",
                MomoApiUrl = "https://test-payment.momo.vn/gw_payment/transactionProcessor",
                SecretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz"
            };

            // Tạo IOptions từ momoOptions
            var options = Options.Create(momoOptions);

            // Khởi tạo MomoService với đối tượng options
            _momoService = new MomoService(options);
        }

        // GET: ShoppingCart
        public ActionResult ShowCart()
        {
            if (Session["Cart"] == null)
                return View("EmptyCart");
            Cart _cart = Session["Cart"] as Cart;
            return View(_cart);
        }

        // Action tạo giỏ hàng
        public Cart GetCart()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null || Session["Cart"] == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // Action thêm product vào giỏ hàng
        public ActionResult AddToCart(int id)
        {
            var _pro = database.Products.SingleOrDefault(s => s.ProductID == id);
            if (_pro != null)
            {
                GetCart().Add_Product_Cart(_pro);
            }
            return RedirectToAction("ShowCart", "ShoppingCart");
        }

        public ActionResult Update_Cart_Quantity(FormCollection form)
        {
            Cart cart = Session["Cart"] as Cart;
            int id_pro = int.Parse(form["idPro"]);
            int _quantity = int.Parse(form["cartQuantity"]);
            cart.Update_quantity(id_pro, _quantity);
            return RedirectToAction("ShowCart", "ShoppingCart");
        }

        public ActionResult RemoveCart(int id)
        {
            Cart cart = Session["Cart"] as Cart;
            cart.Remove_CartItem(id);
            return RedirectToAction("ShowCart", "ShoppingCart");
        }

        public ActionResult ThanhToan()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ThanhToan(FormCollection form)
        {
            if (Session["isLoggedIn"] != null && (bool)Session["isLoggedIn"])
            {
                Cart cart = Session["Cart"] as Cart;
                OrderPro _order = new OrderPro(); // Đơn hàng cho COD
                _order.DateOrder = DateTime.Now;
                Customer cus = Session["Customer"] as Customer;
                _order.NameCus = cus.NameCus;
                _order.PhoneCus = cus.PhoneCus;
                _order.AddressDeliverry = cus.Address;
                _order.IDCus = (int?)Session["IdCustomer"];
                _order.Status = true; // COD
                _order.TinhTrang = "Đang Xác Nhận"; // Ghi nhận trạng thái
                database.OrderProes.Add(_order);

                foreach (var item in cart.Items)
                {
                    OrderDetail _order_detail = new OrderDetail();
                    _order_detail.IDOrder = _order.ID;
                    _order_detail.IDProduct = item._product.ProductID;
                    _order_detail.UnitPrice = (double)cart.Total_money();
                    _order_detail.Quantity = item._quantity;
                    _order_detail.ImgPro = item._product.ImagePro;
                    _order_detail.NamePro = item._product.NamePro;
                    database.OrderDetails.Add(_order_detail);
                    foreach (var p in database.Products.Where(s => s.ProductID == _order_detail.IDProduct))
                    {
                        if (p.RemainQuantity == null)
                        {
                            var update_quan_pro = p.Quantity - item._quantity;
                            if (p.SoldQuantity == null)
                            {
                                p.SoldQuantity = 0;
                                p.SoldQuantity += item._quantity;
                            }
                            else
                            {
                                p.SoldQuantity += item._quantity;
                            }
                            p.RemainQuantity = update_quan_pro;
                        }
                        else
                        {
                            var update_quan_pro = p.RemainQuantity - item._quantity;
                            if (p.SoldQuantity == null)
                            {
                                p.SoldQuantity = 0;
                                p.SoldQuantity += item._quantity;
                            }
                            else
                            {
                                p.SoldQuantity += item._quantity;
                            }
                            p.RemainQuantity = update_quan_pro;
                        }
                    }
                }
                database.SaveChanges();
                cart.ClearCart();
                return RedirectToAction("CheckOut_Suscess", "ShoppingCart");
            }
            else
            {
                return RedirectToAction("Index", "LoginCustomer");
            }
        }
        public ActionResult ThanhToan_MoMo()
        {
            return View();
        }

        // Action POST ThanhToan: Lưu đơn hàng và chuyển sang bước thanh toán MoMo
        [HttpPost]
        public async Task<ActionResult> ThanhToan_MoMo(FormCollection form)
        {
            if (Session["isLoggedIn"] != null && (bool)Session["isLoggedIn"])
            {
                Cart cart = Session["Cart"] as Cart;
                Customer cus = Session["Customer"] as Customer;

                OrderPro order = new OrderPro(); // Đơn hàng cho MoMo
                order.DateOrder = DateTime.Now;
                order.NameCus = cus.NameCus;
                order.PhoneCus = cus.PhoneCus;
                order.AddressDeliverry = cus.Address;
                order.IDCus = (int?)Session["IdCustomer"];
                order.Status = false; // Thanh toán qua MoMo
                order.TinhTrang = "Đang Xác Nhận";
                database.OrderProes.Add(order);
                database.SaveChanges();

                foreach (var item in cart.Items)
                {
                    OrderDetail orderDetail = new OrderDetail();
                    orderDetail.IDOrder = order.ID;
                    orderDetail.IDProduct = item._product.ProductID;
                    orderDetail.UnitPrice = (double)cart.Total_money();
                    orderDetail.Quantity = item._quantity;
                    orderDetail.ImgPro = item._product.ImagePro;
                    orderDetail.NamePro = item._product.NamePro;
                    database.OrderDetails.Add(orderDetail);
                    foreach (var p in database.Products.Where(s => s.ProductID == orderDetail.IDProduct))
                    {
                        if (p.RemainQuantity == null)
                        {
                            var updateQuantity = p.Quantity - item._quantity;
                            p.SoldQuantity = (p.SoldQuantity ?? 0) + item._quantity;
                            p.RemainQuantity = updateQuantity;
                        }
                        else
                        {
                            var updateQuantity = p.RemainQuantity - item._quantity;
                            p.SoldQuantity = (p.SoldQuantity ?? 0) + item._quantity;
                            p.RemainQuantity = updateQuantity;
                        }
                    }
                }
                try
                {
                    database.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                    var fullErrorMessage = string.Join("; ", errorMessages);
                    var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);
                    throw new System.Data.Entity.Validation.DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                }

                // Chuẩn bị thông tin đơn hàng cho thanh toán MoMo
                OrderInfoModel momoOrder = new OrderInfoModel
                {
                    OrderId = order.ID.ToString(),
                    Amount = (double)cart.Total_money(),
                    FullName = order.NameCus,
                    OrderInfo = "Thanh toán cho đơn hàng " + order.ID
                };

                TempData["MomoOrder"] = JsonConvert.SerializeObject(momoOrder);
                TempData["OrderId"] = order.ID;
                return RedirectToAction("ThanhToanMoMo");
            }
            else
            {
                return RedirectToAction("Index", "LoginCustomer");
            }
        }

        // Action xử lý thanh toán qua MoMo
        public async Task<ActionResult> ThanhToanMoMo()
        {
            if (TempData["MomoOrder"] == null)
            {
                return RedirectToAction("ErrorPage");
            }

            var momoOrder = JsonConvert.DeserializeObject<OrderInfoModel>(TempData["MomoOrder"].ToString());
            string payUrl = await ProcessMomoPayment(momoOrder);

            if (!string.IsNullOrEmpty(payUrl))
            {
                return Redirect(payUrl);
            }
            else
            {
                // Nếu không có URL thanh toán, chuyển hướng đến trang CheckOut_Suscess
                return RedirectToAction("CheckOut_Suscess", "ShoppingCart");
            }
        }

        // Phương thức hỗ trợ để gọi API thanh toán MoMo
        private async Task<string> ProcessMomoPayment(OrderInfoModel momoOrder)
        {
            var paymentResponse = await _momoService.CreatePaymentAsync(momoOrder);
            return paymentResponse?.PayUrl;
        }

        // Action hiển thị trang thanh toán thành công, gửi email xác nhận
        public async Task<ActionResult> PaymentSuccess(MomoExecuteResponseModel model)
        {
            // Nếu có OrderId được lưu trong TempData, lấy đơn hàng từ DB và gửi email
            if (TempData["OrderId"] != null)
            {
                int orderId = Convert.ToInt32(TempData["OrderId"]);
                OrderPro order = database.OrderProes.Find(orderId);
                if (order != null)
                {
                    await SendConfirmationEmail(order);
                }
            }
            return View("PaymentSuccess", model);
        }

        // Phương thức gửi email xác nhận đơn hàng thành công
        private async Task SendConfirmationEmail(OrderPro order)
        {
            // Giả sử đối tượng order.Customer có thuộc tính Email
            string toEmail = order.Customer != null ? order.Customer.EmailCus : "";
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            string subject = "Xác nhận đơn hàng đã đặt thành công";
            var body = new StringBuilder();
            body.AppendLine($"Xin chào {order.NameCus},");
            body.AppendLine();
            body.AppendLine("Cảm ơn bạn đã đặt hàng tại cửa hàng của chúng tôi. Dưới đây là thông tin đơn hàng của bạn:");
            body.AppendLine($"Mã đơn hàng: {order.ID}");
            body.AppendLine($"Ngày đặt hàng: {order.DateOrder?.ToString("dd/MM/yyyy HH:mm:ss")}");
            body.AppendLine($"Địa chỉ giao hàng: {order.AddressDeliverry}");
            body.AppendLine($"Tình trạng: {order.TinhTrang}");
            body.AppendLine();
            body.AppendLine("Chi tiết đơn hàng:");
            foreach (var detail in order.OrderDetails)
            {
                body.AppendLine($"- Sản phẩm: {detail.NamePro} (ID: {detail.IDProduct})");
                body.AppendLine($"  Số lượng: {detail.Quantity}, Đơn giá: {detail.UnitPrice}");
            }
            body.AppendLine();
            body.AppendLine("Chúng tôi sẽ liên hệ với bạn trong thời gian sớm nhất để xác nhận đơn hàng.");
            body.AppendLine();
            body.AppendLine("Trân trọng,");
            body.AppendLine("Cửa hàng của chúng tôi");

            // Cấu hình SMTP; ví dụ sử dụng Gmail
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("07phananhdo@gmail.com", "rogt cbcd epil swsg"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("07phananhdo@gmail.com"),
                Subject = subject,
                Body = body.ToString(),
                IsBodyHtml = false,
            };
            mailMessage.To.Add(toEmail);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi gửi email: " + ex.Message);
            }
        }

        public PartialViewResult Check_Again()
        {
            if (Session["Cart"] == null)
                return PartialView("EmptyCart");
            Cart _cart = Session["Cart"] as Cart;
            return PartialView(_cart);
        }

        public PartialViewResult BagCart()
        {
            int total_quantity_item = 0;
            Cart cart = Session["Cart"] as Cart;
            if (cart != null)
                total_quantity_item = cart.Total_quantity();
            ViewBag.QuantityCart = total_quantity_item;
            return PartialView("BagCart");
        }

        public ActionResult CheckOut_Suscess()
        {
            return View();
        }
    }
}
