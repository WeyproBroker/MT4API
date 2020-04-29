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
        static extern int CheckPassword(char[] server, int account, char[] password);
        [DllImport("dllimport.dll", EntryPoint = "AddUser", CallingConvention = CallingConvention.Cdecl)]
        static extern int AddUser(char[] server, int account, char[] name, char[] group, int leverage, char[] password, char[] password_investor, char[] email, char[] country, char[] state, char[] city, char[] address, char[] comment, char[] phone, char[] zipcode);
        [DllImport("dllimport.dll", EntryPoint = "UpdateUser", CallingConvention = CallingConvention.Cdecl)]
        static extern int UpdateUser(char[] server, int account, char[] name, char[] group, int leverage, char[] password, char[] password_investor, char[] email, char[] country, char[] state, char[] city, char[] address, char[] comment, char[] phone, char[] zipcode);
        [DllImport("dllimport.dll", EntryPoint = "GetUser", CallingConvention = CallingConvention.Cdecl)]
        static extern int GetUser(char[] server, int account);
        [DllImport("dllimport.dll", EntryPoint = "GetUserDetail", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr GetUserDetail(char[] server, int account);
        [DllImport("dllimport.dll", EntryPoint = "test", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        static extern IntPtr test();
        [DllImport("dllimport.dll", EntryPoint = "Transaction", CallingConvention = CallingConvention.Cdecl)]
        static extern int Transaction(char[] server, int account, double amount, char[] comment);
        [DllImport("dllimport.dll", EntryPoint = "GetBalance", CallingConvention = CallingConvention.Cdecl)]
        static extern double GetBalance(char[] server, int account);

        /**** MT4 Server設定 *****/
        public char[] Server_Setting(string servername)
        {
            List<ServerData> ServerData = new List<WebapiController.ServerData>();
            ServerData = ServerList();
            string Server = "";
            foreach(ServerData Data in ServerData)
            {
                if(servername.IndexOf(Data.ServerName) != -1)
                {
                    Server = Data.Host + "," + Data.Login + "," + Data.Password;
                }
            }
            return Server.ToCharArray();
            //string[] ServerList = new string[1];
            //ServerList[0] = "103.30.69.22:446,15,Report123!@#";
            //switch (servername)
            //{
            //    case "Test":
            //        return ServerList[0].ToCharArray();
            //    default:
            //        return ServerList[0].ToCharArray();
            //}
        }

        /**** 取得MT4 Server列表 ****/
        public List<ServerData> ServerList()
        {
            List<ServerData> ServerData = new List<WebapiController.ServerData>();
            ServerData.Add(
                new WebapiController.ServerData()
                {
                    ServerName = "Test",
                    Host = "103.30.69.22:446",
                    Login = 15,
                    Password = "Report123!@#"
                }
            );

            ServerData.Add(
                new WebapiController.ServerData()
                {
                    ServerName = "KRC_REAL",
                    Host = "103.30.69.22:446",
                    Login = 15,
                    Password = "Report123!@#"
                }
            );

            ServerData.Add(
                new WebapiController.ServerData()
                {
                    ServerName = "KRC_DEMO",
                    Host = "103.30.69.22:446",
                    Login = 15,
                    Password = "Report123!@#"
                }
            );
            return ServerData;
        }

        [HttpGet]
        public JsonResult GetServerList()
        {
            List<ServerData> ServerData = new List<WebapiController.ServerData>();
            ServerData = ServerList();

            return this.Json(ServerData, JsonRequestBehavior.AllowGet);
        }
        /**** 檢查密碼 ****/
        [HttpPost]
        public JsonResult CheckPassword(string servername, int account, string password)
        {
            char[] Server = Server_Setting(servername);
            int res = CheckPassword(Server, account, password.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }
        
        /**** 檢查帳號 ****/
        [HttpGet]
        // GET: Webapi
        public JsonResult CheckAccount(string servername, int account)
        {

            char[] Server = Server_Setting(servername);
            int res = GetUser(Server, account);
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }

        /*** 新增帳號 ****/
        [HttpPost]
        public JsonResult AddUser(string servername, int account, string name, string group, int leverage, string password, string password_investor, string email, string country, string state, string city, string address, string comment, string phone, string zipcode)
        {
            char[] Server = Server_Setting(servername);
            int res = AddUser(Server, account, name.ToCharArray(), group.ToCharArray(), leverage, password.ToCharArray(), password_investor.ToCharArray(), email.ToCharArray(), country.ToCharArray(), state.ToCharArray(), city.ToCharArray(), address.ToCharArray(), comment.ToCharArray(), phone.ToCharArray(), zipcode.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }

        /***** 更新帳號 ****/
        [HttpPost]
        public JsonResult UpdateUser(string servername, int account, string name, string group, int? leverage, string password, string password_investor, string email, string country, string state, string city, string address, string comment, string phone, string zipcode)
        {
            char[] Server = Server_Setting(servername);
            int res = UpdateUser(Server, account, name.ToCharArray(), group.ToCharArray(), Convert.ToInt32(leverage), password.ToCharArray(), password_investor.ToCharArray(), email.ToCharArray(), country.ToCharArray(), state.ToCharArray(), city.ToCharArray(), address.ToCharArray(), comment.ToCharArray(), phone.ToCharArray(), zipcode.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }
        /***** 查詢帳號詳細資料 *****/
        [HttpGet]
        public JsonResult GetUserData(string servername, int account)
        {
            char[] Server = Server_Setting(servername);
            string data = Marshal.PtrToStringAnsi(GetUserDetail(Server, account));
            string[] data_array = data.Split(',');
            AccountData Data = new AccountData();
            Data.login = Convert.ToInt32(data_array[0]);
            Data.name = data_array[1];
            Data.group = data_array[2];
            Data.leverage = Convert.ToInt32(data_array[3]);
            Data.balance = Convert.ToDouble(data_array[4]);
            Data.agent_account = Convert.ToInt32(data_array[5]);
            Data.email = data_array[6];
            Data.comment = data_array[7];
            Data.enable = Convert.ToInt32(data_array[8]);
            Data.read_only = Convert.ToInt32(data_array[9]);
            Data.report = Convert.ToInt32(data_array[10]);
            return this.Json(Data, JsonRequestBehavior.AllowGet);
        }
        /**** 查詢淨利 ****/
        [HttpGet]
        public JsonResult GetEquity(string servername, int account)
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

        /**** 計算口數傭金 ****/
        [HttpPost]
        public JsonResult CalculationLot(string servername ,string broker_mt4_login, string broker_mt4_commission_percent, string lot_commission, string ib_mt4_login, string clientlist)
        {
            double total_lot = 0.0; // 總口數
            string StartDate = DateTime.Now.ToString("yyyy-MM-dd");
            string EndDate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            MySqlConnection conn = connMySQL();
            string trades_cmdText = "SELECT * FROM mt4_trades WHERE LOGIN =" + broker_mt4_login + " and CLOSE_TIME >= '" + StartDate + "' and CLOSE_TIME <= '" + EndDate + "'";
            MySqlCommand trades_cmd = new MySqlCommand(trades_cmdText, conn);
            using (MySqlDataReader reader = trades_cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    total_lot += Convert.ToInt32(reader["VOLUME"]);
                }
            }
            if(total_lot > 0)
            {
                total_lot = total_lot / 100;
                string[] clientList = clientlist.Split(',');
                foreach (string clientValue in clientList)
                {
                    trades_cmdText = "SELECT * FROM mt4_trades WHERE LOGIN =" + clientValue + " and CLOSE_TIME >= '" + StartDate + "' and CLOSE_TIME <= '" + EndDate + "'";
                    trades_cmd = new MySqlCommand(trades_cmdText, conn);
                    using (MySqlDataReader reader = trades_cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            total_lot += Convert.ToInt32(reader["VOLUME"]);
                        }
                    }
                }

                double lot_commission_money = total_lot * Convert.ToInt32(lot_commission); // 總傭金 = IB底下帳戶的總口數 x 每口傭金價格
                                                                                        /*** 撥給小broker ***/
                double broker_commission_money = lot_commission_money * (Convert.ToInt32(broker_mt4_commission_percent) / 100);
                /*** 撥給IB ****/
                double ib_commission_money = lot_commission_money - broker_commission_money;

                /*** 匯入MT4 ***/
                char[] Server = Server_Setting(servername);
                /*** 匯入小broker mt4帳戶 ***/
                int res;
                res = Transaction(Server, Convert.ToInt32(broker_mt4_login), broker_commission_money, "ib_commission_money".ToCharArray());
                /*** 匯入IB mt4帳戶 ***/
                res = Transaction(Server, Convert.ToInt32(ib_mt4_login), ib_commission_money, "ib_commission_money".ToCharArray());

                return this.Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json("No Commission Money Record", JsonRequestBehavior.AllowGet);
            }
            
        }

        /**** 交易手續【出金、入金、內轉】 ****/
        [HttpGet]
        public JsonResult Transaction(string servername, int account, double amount, string comment)
        {
            char[] Server = Server_Setting(servername);
            int res;
            res = Transaction(Server, account, amount, comment.ToCharArray());
            return this.Json(res, JsonRequestBehavior.AllowGet);
        }

        /**** 查詢帳戶餘額 ****/
        [HttpGet]
        public JsonResult GetUserBalance(string servername, int account)
        {
            char[] Server = Server_Setting(servername);
            double balance = GetBalance(Server, account);
            return this.Json(balance, JsonRequestBehavior.AllowGet);
        }

        /**** 測試API ****/
        [HttpGet]
        public JsonResult test_api()
        {
            double total_lot = 0.0; // 口數
            MySqlConnection conn = connMySQL();
            string trades_cmdText = "SELECT * FROM mt4_trades WHERE LOGIN =" + 890000373 +" and CLOSE_TIME >= '2020-03-20' and CLOSE_TIME <= '2020-04-27'";
            MySqlCommand trades_cmd = new MySqlCommand(trades_cmdText, conn);
            using (MySqlDataReader reader = trades_cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    total_lot += Convert.ToInt32(reader["VOLUME"]);
                }
            }
            //    MySqlDataReader trades_reader = trades_cmd.ExecuteReader(); //execure the reader
            //while (trades_reader.Read())
            //{
            //    lot_str += ","+trades_reader;
            //    return this.Json(lot_str, JsonRequestBehavior.AllowGet);                
            //}
            conn.Close();         
            return this.Json(total_lot/100, JsonRequestBehavior.AllowGet);
            //string data = Marshal.PtrToStringAnsi(test());
            //return this.Json(data, JsonRequestBehavior.AllowGet);
        }

        /**** 連接MYSQL ****/
        public MySqlConnection connMySQL()
        {
            string dbHost = "127.0.0.1";
            string dbUser = "root";
            string dbPass = "!QW@2we3";
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
        public class AccountData
        {
            public int login { get; set; }
            public string name { get; set; }
            public string group { get; set; }
            public int leverage { get; set; }
            public double balance { get; set; }
            public int agent_account { get; set; }
            public string email { get; set; }
            public string comment { get; set; }
            public int enable { get; set; }
            public int read_only { get; set; }
            public int report { get; set; }
        }

        public class ServerData
        {
            public string ServerName { get; set; }
            public string Host { get; set; }
            public int Login { get; set; }
            public string Password { get; set; }
        }

        public class LotData
        {
            public int Lot { get; set; }
        }
    }
}