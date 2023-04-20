using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pay.Models
{
    public class PayVo
    {
        public string ORDERNO { get; set; }
        public string PRODUCTNAME { get; set; }
        public string AMOUNT { get; set; }
        public string BUYERNAME { get; set; }
        public string BUYEREMAIL { get; set; }
        public string PAYMETHOD { get; set; }
        public string PRODUCTCODE { get; set; }
        public string BUYERID { get; set; }
        public string BUYERADDRESS { get; set; }
        public string BUYERPHONE { get; set; }
        public string RETURNURL { get; set; }
        public string CARDNO { get; set; }
        public string EXPIREDT { get; set; }
        public string QUOTA { get; set; }
        public string CARDPWD { get; set; }
        public string CARDAUTH { get; set; }
        public string TID { get; set; }
        public string ETC1 { get; set; }
        public string ETC2 { get; set; }
        public string ETC3 { get; set; }
        public string ETC4 { get; set; }
        public string ETC5 { get; set; }
        public string REASON { get; set; }
        public string STD_DT { get; set; }
        public string END_DT { get; set; }

    }
}