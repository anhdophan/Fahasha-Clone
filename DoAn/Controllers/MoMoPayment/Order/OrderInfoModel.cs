
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DoAn.Controllers.MoMoPayment.Order
{
    public class OrderInfoModel
    {
        public string FullName { get; set; }
        public string OrderId { get; set; }
        public string OrderInfo { get; set; }
        public double Amount { get; set; }
    }
}