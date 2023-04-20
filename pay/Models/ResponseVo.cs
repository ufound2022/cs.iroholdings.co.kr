using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace pay.Models
{
    public class ResponseVo
    {
        public string RESULTCODE { get; set; }
        public string RESULTMSG { get; set; }
        public string ORDERNO { get; set; }
        public int AMOUNT { get; set; }
        public string TID { get; set; }
        public string ACCEPTDATE { get; set; }
        public string ACCEPTNO { get; set; }
        public string CASH_BILL_NO { get; set; }
        public string CARDNAME { get; set; }
        public string ACCOUNTNO { get; set; }
        public string RECEIVERNAME { get; set; }
        public string DEPOSITENDDATE { get; set; }
        public string CARDCODE { get; set; }
        public string QUOTA { get; set; }
        public string ETC1 { get; set; }
        public string ETC2 { get; set; }
        public string ETC3 { get; set; }
        public string ETC4 { get; set; }
        public string ETC5 { get; set; }
    }
}