using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Web.Mvc;
using pay.Models;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace pay.Controllers
{
    public class HomeController : Controller
    {

        string API_ID = "sandbox_pcm2jg05";
        string API_KEY = "0cfcc7d91408924b0a3450f86b99e808db7729aaab90678256";

        string KEYIN_API_ID = "8ctehu8258h";
        string KEYIN_API_KEY = "8eac55dee33ef74159b328d2bf674abfcb5c2f7a042e668cc4";

        public ActionResult Index()
        {
            //System.Diagnostics.Trace.WriteLine("");
            return View();
        }

        //인증요청 화면 CALL
        public ActionResult Ready()
        {
            return View();
        }

        //인증요청 서버통신 CALL
        [HttpPost]
        public ContentResult ReadyCall(PayVo payVo)
        {
            string URL = "https://sandbox.cookiepayments.com/pay/ready";

            //전송 데이터 JSON 형식 만들기
            var json = new JObject();
            json.Add("API_ID", API_ID);                     //쿠키페이 결제 연동 ID
            json.Add("ORDERNO", payVo.ORDERNO);             //주문번호
            json.Add("PRODUCTNAME", payVo.PRODUCTNAME);     //상품명
            json.Add("AMOUNT", payVo.AMOUNT);               //결제 금액
            json.Add("BUYERNAME", payVo.BUYERNAME);         //고객명
            json.Add("BUYEREMAIL", payVo.BUYEREMAIL);       //고객 E-MAIL
            json.Add("RETURNURL", payVo.RETURNURL);         //결제 완료 후 리다이렉트 URL
            json.Add("PRODUCTCODE", payVo.PRODUCTCODE);     //상품 코드
            json.Add("PAYMETHOD", payVo.PAYMETHOD);         //결제 수단
            json.Add("BUYERID", payVo.BUYERID);             //고객 ID
            json.Add("BUYERADDRESS", payVo.BUYERADDRESS);   //고객 주소
            json.Add("BUYERPHONE", payVo.BUYERPHONE);       //고객 휴대폰번호
            json.Add("ETC1", payVo.ETC1);                   //사용자 추가필드 1
            json.Add("ETC2", payVo.ETC2);                   //사용자 추가필드 2
            json.Add("ETC3", payVo.ETC3);                   //사용자 추가필드 3
            json.Add("ETC4", payVo.ETC4);                   //사용자 추가필드 4
            json.Add("ETC5", payVo.ETC5);                   //사용자 추가필드 5

            //요청 HEADER 세팅
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Headers.Add("ApiKey", API_KEY);

            //데이터 전송
            byte[] bytes = Encoding.UTF8.GetBytes(json.ToString());
            request.ContentLength = bytes.Length;
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(bytes, 0, bytes.Length);
            reqStream.Flush();
            reqStream.Close();

            //응답
            string response_data = "";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            HttpStatusCode status = response.StatusCode;
            Stream response_stream = response.GetResponseStream();
            using (StreamReader read_stream = new StreamReader(response_stream))
            {
                response_data = read_stream.ReadToEnd();
                read_stream.Close();
                response_stream.Close();
                response.Close();
            }

            return Content(response_data);
        }

        //인증결제 완료전문 MYSQL DB 입력
        public void CompleteMysql(ResponseVo param)
        {
            //데이터 DB 입력
            using (MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3307;Database=csdb;Uid=csuid;Pwd=1234;"))
            {
                var command = connection.CreateCommand();
                command.CommandText = @"UPDATE '결제 테이블'
                                           SET ACCEPT_DATE = @ACCEPT_DATE
                                              ,ACCEPT_NO = @ACCEPT_NO
                                              ,TID = @TID
                                              ,CARDCODE = @CARDCODE
                                              ,CARDNAME = @CARDNAME
                                              ,QUOTA = @QUOTA
                                              ,PAY_STATUS = @PAY_STATUS
                                         WHERE ORDERNO = @ORDERNO
                                           AND AMOUNT = @AMOUNT
                                           AND PAY_STATUS = @PAY_STATUS2";

                command.Parameters.AddWithValue("@ACCEPT_DATE", param.ACCEPTDATE);
                command.Parameters.AddWithValue("@ACCEPT_NO", param.ACCEPTNO);
                command.Parameters.AddWithValue("@TID", param.TID);
                command.Parameters.AddWithValue("@CARDCODE", param.CARDCODE);
                command.Parameters.AddWithValue("@CARDNAME", param.CARDNAME);
                command.Parameters.AddWithValue("@QUOTA", param.QUOTA);
                command.Parameters.AddWithValue("@PAY_STATUS", "결제성공");
                command.Parameters.AddWithValue("@ORDERNO", param.ORDERNO);
                command.Parameters.AddWithValue("@AMOUNT", param.AMOUNT);
                command.Parameters.AddWithValue("@PAY_STATUS2", "결제대기");

                connection.Open();
                int result = command.ExecuteNonQuery();
                connection.Close();
            }
        }



        //결제정보 검증화면
        public ActionResult Paycert()
        {
            return View();
        }

        //결제정보 검증 전문 요청
        public JsonResult PaycertCall(PayVo payVo)
        {
            //토큰 발행 요청
            string TOKEN_URL = "https://sandbox.cookiepayments.com/payAuth/token";

            //전송 데이터 JSON 형식 만들기
            var token_json = new JObject();
            token_json.Add("pay2_id", API_ID);     //쿠키페이 결제 연동 ID
            token_json.Add("pay2_key", API_KEY);   //쿠키페이 연동 키

            //요청 HEADER 세팅
            HttpWebRequest token_request = (HttpWebRequest)WebRequest.Create(TOKEN_URL);
            token_request.Method = "POST";
            token_request.ContentType = "application/json";

            //데이터 전송
            byte[] token_bytes = Encoding.UTF8.GetBytes(token_json.ToString());
            token_request.ContentLength = token_bytes.Length;
            Stream token_reqStream = token_request.GetRequestStream();
            token_reqStream.Write(token_bytes, 0, token_bytes.Length);
            token_reqStream.Flush();
            token_reqStream.Close();

            //응답
            HttpWebResponse token_response = (HttpWebResponse)token_request.GetResponse();
            HttpStatusCode status = token_response.StatusCode;
            Stream token_res_stream = token_response.GetResponseStream();
            StreamReader token_read_stream = new StreamReader(token_res_stream);
            string token_resp_data = token_read_stream.ReadToEnd();
            //응답 토근 정보
            var token_result = JObject.Parse(token_resp_data);
            token_read_stream.Close();
            token_res_stream.Close();
            token_response.Close();

            //0000: 성공
            string cert_result = "";
            string rtn_cd = Convert.ToString(token_result["RTN_CD"]);
            if (rtn_cd == "0000")
            {
                //결제 검증 요청
                string CERT_URL = "https://sandbox.cookiepayments.com/api/paycert";
                string TOKEN = Convert.ToString(token_result["TOKEN"]);
                HttpWebRequest cert_request = (HttpWebRequest)WebRequest.Create(CERT_URL);
                cert_request.Method = "POST";
                cert_request.ContentType = "application/json";
                cert_request.Headers.Add("TOKEN", TOKEN);

                var cert_json = new JObject();
                cert_json.Add("tid", payVo.TID);     //결제 후 응답받은 PG사 거래 고유번호

                byte[] cert_bytes = Encoding.UTF8.GetBytes(cert_json.ToString());
                cert_request.ContentLength = cert_bytes.Length;
                Stream cert_stream = cert_request.GetRequestStream();
                cert_stream.Write(cert_bytes, 0, cert_bytes.Length);
                cert_stream.Flush();
                cert_stream.Close();

                //응답
                HttpWebResponse cert_response = (HttpWebResponse)cert_request.GetResponse();
                HttpStatusCode cert_status = cert_response.StatusCode;
                Stream cert_rep_stream = cert_response.GetResponseStream();
                StreamReader cert_read_stream = new StreamReader(cert_rep_stream, Encoding.UTF8);
                //결과처리
                cert_result = cert_read_stream.ReadToEnd();
                cert_read_stream.Close();
                cert_rep_stream.Close();
                cert_response.Close();
            }

            return Json(cert_result);
        }

        //비인증결제 요청 화면
        public ActionResult Keyin()
        {
            return View();
        }

        //비인증결제 요청 전문
        public JsonResult KeyinCall(PayVo payVo)
        {
            //토큰 발행 요청
            string TOKEN_URL = "https://sandbox.cookiepayments.com/payAuth/token";

            //전송 데이터 JSON 형식 만들기
            var token_json = new JObject();
            token_json.Add("pay2_id", API_ID);     //쿠키페이 결제 연동 ID
            token_json.Add("pay2_key", API_KEY);   //쿠키페이 연동 키

            //요청 HEADER 세팅
            HttpWebRequest token_request = (HttpWebRequest)WebRequest.Create(TOKEN_URL);
            token_request.Method = "POST";
            token_request.ContentType = "application/json";

            //데이터 전송
            byte[] token_bytes = Encoding.UTF8.GetBytes(token_json.ToString());
            token_request.ContentLength = token_bytes.Length;
            Stream token_reqStream = token_request.GetRequestStream();
            token_reqStream.Write(token_bytes, 0, token_bytes.Length);
            token_reqStream.Flush();
            token_reqStream.Close();

            //응답
            HttpWebResponse token_response = (HttpWebResponse)token_request.GetResponse();
            HttpStatusCode status = token_response.StatusCode;
            Stream token_res_stream = token_response.GetResponseStream();
            StreamReader token_read_stream = new StreamReader(token_res_stream);
            string token_resp_data = token_read_stream.ReadToEnd();
            //응답 토근 정보
            var token_result = JObject.Parse(token_resp_data);
            token_read_stream.Close();
            token_res_stream.Close();
            token_response.Close();

            //0000: 성공
            string keyin_resp_data = "";
            string rtn_cd = Convert.ToString(token_result["RTN_CD"]);
            if (rtn_cd == "0000")
            {
                //비인증 결제 요청
                string KEYIN_URL = "https://sandbox.cookiepayments.com/keyin/payment";  //요청 URL
                string TOKEN = Convert.ToString(token_result["TOKEN"]);                 //응답 받은 토큰 값
                //요청 HEADER 세팅
                HttpWebRequest keyin_request = (HttpWebRequest)WebRequest.Create(KEYIN_URL);
                keyin_request.Method = "POST";
                keyin_request.ContentType = "application/json";
                keyin_request.Headers.Add("ApiKey", KEYIN_API_KEY);
                keyin_request.Headers.Add("TOKEN", TOKEN);

                //전송 데이터 JSON 형식 만들기
                var keyin_json = new JObject();
                keyin_json.Add("API_ID", KEYIN_API_ID);             //COOKIEPAY에서 발급받은 가맹점연동 ID
                keyin_json.Add("ORDERNO", payVo.ORDERNO);           //주문번호
                keyin_json.Add("PRODUCTNAME", payVo.PRODUCTNAME);   //상품명
                keyin_json.Add("AMOUNT", payVo.AMOUNT);             //결제금액
                keyin_json.Add("BUYERNAME", payVo.BUYERNAME);       //고객명
                keyin_json.Add("BUYEREMAIL", payVo.BUYEREMAIL);     //고객 E-MAIL
                keyin_json.Add("CARDNO", payVo.CARDNO);             //카드번호
                keyin_json.Add("EXPIREDT", payVo.EXPIREDT);         //카드유효기간
                keyin_json.Add("BUYERID", payVo.BUYERID);           //고객 ID
                keyin_json.Add("BUYERPHONE", payVo.BUYERPHONE);     //고객 휴대폰번호
                keyin_json.Add("QUOTA", payVo.QUOTA);               //할부개월
                keyin_json.Add("CARDPWD", payVo.CARDPWD);           //비밀번호 앞두자리
                keyin_json.Add("CARDAUTH", payVo.CARDAUTH);         //생년월일

                //데이터 전송
                byte[] keyin_bytes = Encoding.UTF8.GetBytes(keyin_json.ToString());
                keyin_request.ContentLength = keyin_bytes.Length;
                Stream keyin_reqStream = keyin_request.GetRequestStream();
                keyin_reqStream.Write(keyin_bytes, 0, keyin_bytes.Length);
                keyin_reqStream.Flush();
                keyin_reqStream.Close();

                //응답
                HttpWebResponse keyin_response = (HttpWebResponse)keyin_request.GetResponse();
                HttpStatusCode keyin_status = keyin_response.StatusCode;
                Stream keyin_res_stream = keyin_response.GetResponseStream();
                StreamReader keyin_read_stream = new StreamReader(keyin_res_stream);
                keyin_resp_data = keyin_read_stream.ReadToEnd();

                //응답 정보
                var keyin_result = JObject.Parse(keyin_resp_data);
                keyin_read_stream.Close();
                keyin_res_stream.Close();
                keyin_response.Close();

                string RESULTCODE = Convert.ToString(keyin_result["RESULTCODE"]);
                //응답 성공
                if (RESULTCODE == "0000")
                {
                    string ORDERNO = Convert.ToString(keyin_result["ORDERNO"]);
                    int AMOUNT = Convert.ToInt32(keyin_result["AMOUNT"]);
                    string TID = Convert.ToString(keyin_result["TID"]);
                    string ACCEPTDATE = Convert.ToString(keyin_result["ACCEPTDATE"]);
                    string ACCEPTNO = Convert.ToString(keyin_result["ACCEPTNO"]);
                    string CARDNAME = Convert.ToString(keyin_result["CARDNAME"]);
                    string TEMP_ORDERNO = Convert.ToString(keyin_result["TEMP_ORDERNO"]);
                    string RECEIPT_URL = Convert.ToString(keyin_result["RECEIPT_URL"]);
                    string QUOTA = Convert.ToString(keyin_result["QUOTA"]);

                    /*
                    //데이터 MS-SQL DB 입력           
                    using (SqlConnection connection = new SqlConnection("Server=hostname; Uid=csuid; Pwd =1234; database=csdb;"))
                    {
                        SqlCommand command = new SqlCommand();
                        command.Connection = connection;
                        command.CommandText = @"UPDATE PAY_MASTER
                                                   SET ACCEPT_DATE = @ACCEPT_DATE
                                                      ,ACCEPT_NO = @ACCEPT_NO
                                                      ,TID = @TID
                                                      ,CARDNAME = @CARDNAME
                                                      ,QUOTA = @QUOTA
                                                 WHERE ORDERNO = @ORDERNO
                                                   AND AMOUNT = @AMOUNT";

                        command.Parameters.AddWithValue("@ACCEPTDATE", ACCEPTDATE);
                        command.Parameters.AddWithValue("@ACCEPTNO", ACCEPTNO);
                        command.Parameters.AddWithValue("@TID", TID);
                        command.Parameters.AddWithValue("@CARDNAME", CARDNAME);
                        command.Parameters.AddWithValue("@QUOTA", QUOTA);
                        command.Parameters.AddWithValue("@ORDERNO", ORDERNO);
                        command.Parameters.AddWithValue("@AMOUNT", AMOUNT);

                        connection.Open();
                        int result = command.ExecuteNonQuery();
                        connection.Close();
                    }
                    */

                    /*데이터 MYSQL DB 입력
                    using (MySqlConnection connection = new MySqlConnection("Server=localhost;Port=3307;Database=csdb;Uid=csuid;Pwd=1234;"))
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = @"UPDATE PAY_MASTER
                                                   SET ACCEPT_DATE = @ACCEPT_DATE
                                                      ,ACCEPT_NO = @ACCEPT_NO
                                                      ,TID = @TID
                                                      ,CARDNAME = @CARDNAME
                                                      ,QUOTA = @QUOTA
                                                 WHERE ORDERNO = @ORDERNO
                                                   AND AMOUNT = @AMOUNT";
                        command.Parameters.AddWithValue("@ACCEPTDATE", ACCEPTDATE);
                        command.Parameters.AddWithValue("@ACCEPTNO", ACCEPTNO);
                        command.Parameters.AddWithValue("@TID", TID);
                        command.Parameters.AddWithValue("@CARDNAME", CARDNAME);
                        command.Parameters.AddWithValue("@QUOTA", QUOTA);
                        command.Parameters.AddWithValue("@ORDERNO", ORDERNO);
                        command.Parameters.AddWithValue("@AMOUNT", AMOUNT);
                        connection.Open();
                        int result = command.ExecuteNonQuery();
                        connection.Close();(
                    }*/
        }
            }

            return Json(keyin_resp_data);
        }

        //결제 취소 요청 화면
        public ActionResult Cancel()
        {
            return View();
        }

        public ActionResult CancelKeyin()
        {
            return View();
        }

        //결제 취소 요청 전문
        public JsonResult CancelCall(PayVo payVo)
        {
            //토큰 발행 요청
            string TOKEN_URL = "https://sandbox.cookiepayments.com/payAuth/token";

            //전송 데이터 JSON 형식 만들기
            var token_json = new JObject();
            token_json.Add("pay2_id", API_ID);     //쿠키페이 결제 연동 ID
            token_json.Add("pay2_key", API_KEY);   //쿠키페이 연동 키

            //요청 HEADER 세팅
            HttpWebRequest token_request = (HttpWebRequest)WebRequest.Create(TOKEN_URL);
            token_request.Method = "POST";
            token_request.ContentType = "application/json";

            //데이터 전송
            byte[] token_bytes = Encoding.UTF8.GetBytes(token_json.ToString());
            token_request.ContentLength = token_bytes.Length;
            Stream token_reqStream = token_request.GetRequestStream();
            token_reqStream.Write(token_bytes, 0, token_bytes.Length);
            token_reqStream.Flush();
            token_reqStream.Close();

            //응답
            HttpWebResponse token_response = (HttpWebResponse)token_request.GetResponse();
            HttpStatusCode status = token_response.StatusCode;
            Stream token_res_stream = token_response.GetResponseStream();
            StreamReader token_read_stream = new StreamReader(token_res_stream);
            string token_resp_data = token_read_stream.ReadToEnd();
            
            //응답 토근 정보
            var token_result = JObject.Parse(token_resp_data);
            token_read_stream.Close();
            token_res_stream.Close();
            token_response.Close();

            //0000: 성공
            string cancel_resp_data = "";
            string rtn_cd = Convert.ToString(token_result["RTN_CD"]);
            if (rtn_cd == "0000")
            {
                //결제 취소 요청
                string CANCEL_URL = "https://sandbox.cookiepayments.com/api/cancel";  //요청 URL
                string TOKEN = Convert.ToString(token_result["TOKEN"]);               //응답 받은 토큰 값
                //요청 HEADER 세팅
                HttpWebRequest cancel_request = (HttpWebRequest)WebRequest.Create(CANCEL_URL);
                cancel_request.Method = "POST";
                cancel_request.ContentType = "application/json";
                cancel_request.Headers.Add("ApiKey", API_KEY);
                cancel_request.Headers.Add("TOKEN", TOKEN);

                //전송 데이터 JSON 형식 만들기
                var cancel_json = new JObject();
                cancel_json.Add("tid", payVo.TID);
                cancel_json.Add("reason", payVo.REASON);

                //데이터 전송
                byte[] cancel_bytes = Encoding.UTF8.GetBytes(cancel_json.ToString());
                cancel_request.ContentLength = cancel_bytes.Length;
                Stream cancel_reqStream = cancel_request.GetRequestStream();
                cancel_reqStream.Write(cancel_bytes, 0, cancel_bytes.Length);
                cancel_reqStream.Flush();
                cancel_reqStream.Close();

                //응답
                HttpWebResponse cancel_response = (HttpWebResponse)cancel_request.GetResponse();
                HttpStatusCode cancel_status = cancel_response.StatusCode;
                Stream cancel_res_stream = cancel_response.GetResponseStream();
                StreamReader cancel_read_stream = new StreamReader(cancel_res_stream);
                cancel_resp_data = cancel_read_stream.ReadToEnd();
                cancel_read_stream.Close();
                cancel_res_stream.Close();
                cancel_response.Close();
            }

            return Json(cancel_resp_data);
        }

        //결제 취소 요청 전문
        public JsonResult CancelKeyinCall(PayVo payVo)
        {
            //토큰 발행 요청
            string TOKEN_URL = "https://sandbox.cookiepayments.com/payAuth/token";

            //전송 데이터 JSON 형식 만들기
            var token_json = new JObject();
            token_json.Add("pay2_id", KEYIN_API_ID);     //쿠키페이 결제 연동 ID
            token_json.Add("pay2_key", KEYIN_API_KEY);   //쿠키페이 연동 키

            //요청 HEADER 세팅
            HttpWebRequest token_request = (HttpWebRequest)WebRequest.Create(TOKEN_URL);
            token_request.Method = "POST";
            token_request.ContentType = "application/json";

            //데이터 전송
            byte[] token_bytes = Encoding.UTF8.GetBytes(token_json.ToString());
            token_request.ContentLength = token_bytes.Length;
            Stream token_reqStream = token_request.GetRequestStream();
            token_reqStream.Write(token_bytes, 0, token_bytes.Length);
            token_reqStream.Flush();
            token_reqStream.Close();

            //응답
            HttpWebResponse token_response = (HttpWebResponse)token_request.GetResponse();
            HttpStatusCode status = token_response.StatusCode;
            Stream token_res_stream = token_response.GetResponseStream();
            StreamReader token_read_stream = new StreamReader(token_res_stream);
            string token_resp_data = token_read_stream.ReadToEnd();

            //응답 토근 정보
            var token_result = JObject.Parse(token_resp_data);
            token_read_stream.Close();
            token_res_stream.Close();
            token_response.Close();

            //0000: 성공
            string cancel_resp_data = "";
            string rtn_cd = Convert.ToString(token_result["RTN_CD"]);
            if (rtn_cd == "0000")
            {
                //결제 취소 요청
                string CANCEL_URL = "https://sandbox.cookiepayments.com/api/cancel";  //요청 URL
                string TOKEN = Convert.ToString(token_result["TOKEN"]);               //응답 받은 토큰 값
                //요청 HEADER 세팅
                HttpWebRequest cancel_request = (HttpWebRequest)WebRequest.Create(CANCEL_URL);
                cancel_request.Method = "POST";
                cancel_request.ContentType = "application/json";
                cancel_request.Headers.Add("ApiKey", KEYIN_API_KEY);
                cancel_request.Headers.Add("TOKEN", TOKEN);

                //전송 데이터 JSON 형식 만들기
                var cancel_json = new JObject();
                cancel_json.Add("tid", payVo.TID);
                cancel_json.Add("reason", payVo.REASON);

                //데이터 전송
                byte[] cancel_bytes = Encoding.UTF8.GetBytes(cancel_json.ToString());
                cancel_request.ContentLength = cancel_bytes.Length;
                Stream cancel_reqStream = cancel_request.GetRequestStream();
                cancel_reqStream.Write(cancel_bytes, 0, cancel_bytes.Length);
                cancel_reqStream.Flush();
                cancel_reqStream.Close();

                //응답
                HttpWebResponse cancel_response = (HttpWebResponse)cancel_request.GetResponse();
                HttpStatusCode cancel_status = cancel_response.StatusCode;
                Stream cancel_res_stream = cancel_response.GetResponseStream();
                StreamReader cancel_read_stream = new StreamReader(cancel_res_stream);
                cancel_resp_data = cancel_read_stream.ReadToEnd();
                cancel_read_stream.Close();
                cancel_res_stream.Close();
                cancel_response.Close();
            }

            return Json(cancel_resp_data);
        }

        //결제내역 조회 화면
        public ActionResult List()
        {
            return View();
        }

        //결제내역 조회 전문 요청
        public JsonResult ListCall(PayVo payVo)
        {
            //토큰 발행 요청
            string TOKEN_URL = "https://sandbox.cookiepayments.com/payAuth/token";

            //전송 데이터 JSON 형식 만들기
            var token_json = new JObject();
            token_json.Add("pay2_id", API_ID);     //쿠키페이 결제 연동 ID
            token_json.Add("pay2_key", API_KEY);   //쿠키페이 연동 키

            //요청 HEADER 세팅
            HttpWebRequest token_request = (HttpWebRequest)WebRequest.Create(TOKEN_URL);
            token_request.Method = "POST";
            token_request.ContentType = "application/json";

            //데이터 전송
            byte[] token_bytes = Encoding.UTF8.GetBytes(token_json.ToString());
            token_request.ContentLength = token_bytes.Length;
            Stream token_reqStream = token_request.GetRequestStream();
            token_reqStream.Write(token_bytes, 0, token_bytes.Length);
            token_reqStream.Flush();
            token_reqStream.Close();

            //응답
            HttpWebResponse token_response = (HttpWebResponse)token_request.GetResponse();
            HttpStatusCode status = token_response.StatusCode;
            Stream token_res_stream = token_response.GetResponseStream();
            StreamReader token_read_stream = new StreamReader(token_res_stream);
            string token_resp_data = token_read_stream.ReadToEnd();
            //응답 토근 정보
            var token_result = JObject.Parse(token_resp_data);
            token_read_stream.Close();
            token_res_stream.Close();
            token_response.Close();

            //0000: 성공
            string list_resp_data = "";
            string rtn_cd = Convert.ToString(token_result["RTN_CD"]);
            if (rtn_cd == "0000")
            {
                //결제내역 조회 요청
                string LIST_URL = "https://sandbox.cookiepayments.com/api/paysearch";  //요청 URL
                string TOKEN = Convert.ToString(token_result["TOKEN"]);                //응답 받은 토큰 값
                //요청 HEADER 세팅
                HttpWebRequest list_request = (HttpWebRequest)WebRequest.Create(LIST_URL);
                list_request.Method = "POST";
                list_request.ContentType = "application/json";
                list_request.Headers.Add("TOKEN", TOKEN);

                //전송 데이터 JSON 형식 만들기
                var list_json = new JObject();
                list_json.Add("API_ID", API_ID);            //쿠키페이 결제 연동 ID
                if (payVo.TID != "")
                {
                    list_json.Add("TID", payVo.TID);        //TID 입력 시 기간 무시
                }
                list_json.Add("STD_DT", payVo.STD_DT);
                list_json.Add("END_DT", payVo.END_DT);

                //데이터 전송
                byte[] list_bytes = Encoding.UTF8.GetBytes(list_json.ToString());
                list_request.ContentLength = list_bytes.Length;
                Stream list_reqStream = list_request.GetRequestStream();
                list_reqStream.Write(list_bytes, 0, list_bytes.Length);
                list_reqStream.Flush();
                list_reqStream.Close();

                //응답
                HttpWebResponse list_response = (HttpWebResponse)list_request.GetResponse();
                HttpStatusCode list_status = list_response.StatusCode;
                Stream list_res_stream = list_response.GetResponseStream();
                StreamReader list_read_stream = new StreamReader(list_res_stream);
                list_resp_data = list_read_stream.ReadToEnd();
                list_read_stream.Close();
                list_res_stream.Close();
                list_response.Close();
            }

            return Json(list_resp_data);
        }
    }
}