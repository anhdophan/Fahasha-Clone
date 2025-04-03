using DoAn.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DoAn.Controllers
{
    public class SendConfirmationEmail
    {
        static void Main(string[] args)
        {
            // Tạo đối tượng order mẫu để demo (thông thường bạn sẽ lấy từ cơ sở dữ liệu)
            OrderPro order = new OrderPro
            {
                ID = 1001,
                DateOrder = DateTime.Now,
                AddressDeliverry = "123 Đường ABC, Quận 1, TP.HCM",
                NameCus = "Nguyễn Văn A",
                PhoneCus = "0123456789",
                TinhTrang = "Đang Xác Nhận",
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        IDProduct = 1,
                        NamePro = "Sản phẩm X",
                        Quantity = 2,
                        UnitPrice = 50000
                    },
                    new OrderDetail
                    {
                        IDProduct = 2,
                        NamePro = "Sản phẩm Y",
                        Quantity = 1,
                        UnitPrice = 75000
                    }
                },
                Customer = new Customer
                {
                    // Giả sử đối tượng Customer có thuộc tính Email
                    EmailCus = "khachhang@example.com"
                }
            };

            // Gửi email xác nhận đơn hàng
            Task t = SendConfirmationEmailToCus(order);
            t.Wait(); // Chờ đến khi gửi email xong (trong ứng dụng Console, bạn có thể dùng Wait)

            Console.WriteLine("Nhấn phím bất kỳ để thoát...");
            Console.ReadKey();
        }
        public static async Task SendConfirmationEmailToCus(OrderPro order)
        {
            // Kiểm tra email khách hàng
            string toEmail = order.Customer != null ? order.Customer.EmailCus : "";
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                Console.WriteLine("Email khách hàng không được cung cấp.");
                return;
            }

            // Tạo tiêu đề và nội dung email
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

            // Cấu hình SMTP (ví dụ sử dụng Gmail)
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
                Console.WriteLine("Email xác nhận đã được gửi thành công.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi gửi email: " + ex.Message);
            }
        }
    }
}