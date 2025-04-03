
using DoAn.Controllers.MoMoPayment.Order;
using DoAn.Controllers.MoMoPayment.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace DoAn.Controllers.MoMoPayment
{
    public class MoMoHomeController : Controller
    {
        private IMomoService _momoService;

        public MoMoHomeController(IMomoService momoService)
        {
            _momoService = momoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }
    }
}