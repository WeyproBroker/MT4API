using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;
namespace MT4_WEBAPI.Controllers
{
    public class WebapiController : Controller
    {
        [DllImport("dllimport.dll", EntryPoint = "CheckPassword", CallingConvention = CallingConvention.Cdecl)]
        static extern int CheckPassword(int account, char[] password);
        [DllImport("dllimport.dll", EntryPoint = "AddUser", CallingConvention = CallingConvention.Cdecl)]
        static extern int AddUser(int account, char[] name, char[] group, int leverage, char[] password, char[] password_investor, char[] email, char[] country, char[] state, char[] city, char[] address, char[] comment, char[] phone, char[] zipcode);
        [DllImport("dllimport.dll", EntryPoint = "UpdateUser", CallingConvention = CallingConvention.Cdecl)]
        static extern int UpdateUser(int account, char[] name, char[] group, int leverage, char[] password, char[] password_investor, char[] email, char[] country, char[] state, char[] city, char[] address, char[] comment, char[] phone, char[] zipcode);
        [DllImport("dllimport.dll", EntryPoint = "GetUser", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetUser(int account);
        [DllImport("dllimport.dll", EntryPoint = "GetUserDetail", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr GetUserDetail(int account);
        [DllImport("dllimport.dll", EntryPoint = "test", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern IntPtr test(int account);
        [DllImport("dllimport.dll", EntryPoint = "Withdraw", CallingConvention = CallingConvention.Cdecl)]
        static extern int Transaction(int account, int amount, char[] comment);

        /**** 檢查密碼 ****/
        [HttpPost]
        public JsonResult CheckPassword(int account, string password)
        {
            int res = CheckPassword(account, password.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }
        
        /**** 檢查帳號 ****/
        [HttpGet]
        // GET: Webapi
        public JsonResult CheckAccount(int account)
        {
            int res = GetUser(account);
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }

        /*** 新增帳號 ****/
        [HttpPost]
        public JsonResult AddUser(int account, string name, string group, int? leverage, string password, string password_investor, string email, string country, string state, string city, string address, string comment, string phone, string zipcode)
        {
            if (leverage == null)
            {
                leverage = 100;
            }
            int res = AddUser(account, name.ToCharArray(), group.ToCharArray(), Convert.ToInt32(leverage), password.ToCharArray(), password_investor.ToCharArray(), email.ToCharArray(), country.ToCharArray(), state.ToCharArray(), city.ToCharArray(), address.ToCharArray(), comment.ToCharArray(), phone.ToCharArray(), zipcode.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }

        /***** 更新帳號 ****/
        [HttpPost]
        public JsonResult UpdateUser(int account, string name, string group, int? leverage, string password, string password_investor, string email, string country, string state, string city, string address, string comment, string phone, string zipcode)
        {
            int res = UpdateUser(account, name.ToCharArray(), group.ToCharArray(), Convert.ToInt32(leverage), password.ToCharArray(), password_investor.ToCharArray(), email.ToCharArray(), country.ToCharArray(), state.ToCharArray(), city.ToCharArray(), address.ToCharArray(), comment.ToCharArray(), phone.ToCharArray(), zipcode.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }
        /***** 查詢帳號詳細資料 *****/
        [HttpGet]
        public JsonResult GetUserData(int account)
        {
            string data = Marshal.PtrToStringAnsi(GetUserDetail(account));
            return this.Json(data, JsonRequestBehavior.AllowGet);
        }
        /**** 查詢淨利 ****/
        [HttpGet]
        public JsonResult GetEquity(int account)
        {
            /*** 欄位代號 ***/
            int login_number = 0;  // 帳號
            int balance_number = 24; // 餘額
            int equity_number = 33; // 淨值
            int profit_number = 19; // 獲利
            int credit_number = 27; // 信用
            int margin_number = 34; // 已用預付款
            int margin_free_number = 36; // 可用預付款
            int margin_level_number = 35; // 預付款比例

            Equity data = new Equity();

            MySqlConnection conn = connMySQL();
            String user_cmdText = "SELECT * FROM mt4_users WHERE LOGIN =" + account;
            MySqlCommand user_cmd = new MySqlCommand(user_cmdText, conn);
            MySqlDataReader user_reader = user_cmd.ExecuteReader(); //execure the reader
            while (user_reader.Read())
            {
                data.login = user_reader.GetInt32(login_number);
                data.balance = user_reader.GetDouble(balance_number);
                data.equity = user_reader.GetDouble(equity_number);
                data.credit = user_reader.GetDouble(credit_number);
                data.margin = user_reader.GetDouble(margin_number);
                data.margin_free = user_reader.GetDouble(margin_free_number);
                data.margin_level = user_reader.GetDouble(margin_level_number);
            }
            conn.Close();
            conn = connMySQL();
            string trades_cmdText = "SELECT * FROM mt4_trades WHERE LOGIN =" + account;
            MySqlCommand trades_cmd = new MySqlCommand(trades_cmdText, conn);
            MySqlDataReader trades_reader = trades_cmd.ExecuteReader(); //execure the reader
            while (trades_reader.Read())
            {
                data.profit = trades_reader.GetDouble(profit_number);
            }
            conn.Close();
            if (data.login == 0)
            {
                data.status = "No MT4_Account";
            }
            else
            {
                data.status = "Success";
            }
            return this.Json(data, JsonRequestBehavior.AllowGet);
        }

        /**** 交易手續【出金、入金、內轉】 ****/
        [HttpGet]
        public JsonResult Transaction(int account, int amount, string comment)
        {
            int res;
            res = Transaction(account, amount, comment.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }
        /**** 測試API ****/
        [HttpGet]
        public JsonResult test_api(int account)
        {
            string strIP = Marshal.PtrToStringAnsi(test(account));
            return this.Json(strIP, JsonRequestBehavior.AllowGet);
        }

        /**** 連接MYSQL ****/
        public MySqlConnection connMySQL()
        {
            string dbHost = "127.0.0.1";
            string dbUser = "root";
            string dbPass = "root12ab";
            string dbName = "mt4_report";
            string connStr = "server=" + dbHost + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName;
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand command = conn.CreateCommand();
            conn.Open();
            return conn;
        }

        public class Equity
        {
            public string status { get; set; }
            public int login { get; set; }
            public double balance { get; set; }
            public double equity { get; set; }
            public double profit { get; set; }
            public double credit { get; set; }
            public double margin { get; set; }
            public double margin_free { get; set; }
            public double margin_level { get; set; }
        }
    }
}