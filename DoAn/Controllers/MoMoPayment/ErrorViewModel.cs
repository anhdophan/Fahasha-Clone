
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DoAn.Controllers.MoMoPayment
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}