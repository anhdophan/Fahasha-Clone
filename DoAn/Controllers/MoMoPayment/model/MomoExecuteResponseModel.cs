
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DoAn.Controllers.MoMoPayment.model
{
    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string OrderInfo { get; set; }
    }
}