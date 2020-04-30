using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net.Http;
using Newtonsoft.Json;

namespace MT4_CalcCommission
{
    class Program
    {
        static void Main()
        {
            List<broker_productList> broker_productList = new List<broker_productList>();
            List<accountList> accountList = new List<accountList>();
            List<clientList> clientList = new List<clientList>();
            string Status="";
            Program test = new Program();
            broker_productList = test.GetBrokerProduct();
            if(broker_productList.Count() > 0)
            {
                foreach(broker_productList broker_product in broker_productList)
                {
                    accountList = test.GetAccountList(broker_product);                  
                }
                if(accountList.Count() > 0)
                {
                    foreach(accountList account in accountList)
                    {
                        Status = test.run_calc_commission(account);
                        Console.WriteLine(Status);
                    }
                }
                else
                {
                    Console.WriteLine("No Account");    //此Broker沒有客戶
                }       
            }
            else
            {
                Console.WriteLine("No MarginTrades");   // 沒有MarginTrades的Broker
            }
        }

        public MySqlConnection connMySQL(string dbName)
        {
            string dbHost = "103.30.70.187";
            string dbUser = "krc";
            string dbPass = "cLTnqU3b";
            string connStr = "server=" + dbHost + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName;
            MySqlConnection conn = new MySqlConnection(connStr);
            MySqlCommand command = conn.CreateCommand();
            conn.Open();
            return conn;
        }

        public List<broker_productList> GetBrokerProduct()
        {
            List<broker_productList> broker_productList = new List<broker_productList>();
            MySqlConnection conn = connMySQL("krc_backend");
            string cmdText = "SELECT * FROM broker_product";
            MySqlCommand cmd = new MySqlCommand(cmdText, conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (Convert.ToInt32(reader["mt4_productID"]) == 14)
                    {
                        broker_productList.Add(
                            new broker_productList()
                            {
                                servername = reader["mt4_server"].ToString(),
                                bId = Convert.ToInt32(reader["bId"]),
                                id = Convert.ToInt32(reader["Id"]),
                                mt4_commission_account = Convert.ToInt32(reader["mt4_commission_account"]),
                                lot_commission = Convert.ToInt32(reader["lot_commission"]),
                                mt4_commission_percent = Convert.ToInt32(reader["mt4_commission_percent"])
                            }
                        );
                    }
                }
            }
            conn.Close();
            return broker_productList;
        }

        public List<accountList> GetAccountList(broker_productList broker_product)
        {
            List<accountList> accountList = new List<accountList>();
            List<clientList> clientList = new List<clientList>();
            accountList.Add(
                new Program.accountList()
                {
                    ib_mt4_login = broker_product.mt4_commission_account,
                    broker_productList = broker_product,
                }
            );
            while (GetIBCode(broker_product, accountList).Count() > 0) ;
            
            /**** IB的客戶 ****/
            MySqlConnection conn = connMySQL("ci_broker_" + broker_product.bId);   
            string cmdText = "SELECT * FROM client_product WHERE apply_account = 1 AND pId=" + broker_product.id;
            MySqlCommand cmd = new MySqlCommand(cmdText, conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    clientList = GetClientList(broker_product, Convert.ToInt32(reader["mt4_login"]));
                    accountList.Add(
                        new accountList()
                        {
                            ib_mt4_login = Convert.ToInt32(reader["mt4_login"]),
                            broker_productList = broker_product,
                        }
                    );
                }
            }
            conn.Close();
            return accountList;
        }

        public List<clientList> GetClientList(broker_productList broker_product, int ib_mt4_login)
        {
            List<clientList> clientList = new List<clientList>();
            MySqlConnection conn = connMySQL("ci_broker_" + broker_product.bId);
            string cmdText = "SELECT * FROM client_product WHERE apply_account = 0 AND introducer_IB_Code = " + ib_mt4_login +" AND pId = " + broker_product.id;
            MySqlCommand cmd = new MySqlCommand(cmdText, conn);
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    clientList.Add(
                        new clientList()
                        {
                            client_mt4_login = Convert.ToInt32(reader["mt4_login"])
                        }
                    );
                }
            }
            conn.Close();
            return clientList;
        }

        public List<accountList> GetIBCode(broker_productList broker_product, List<accountList> accountList)
        {
            List<accountList> ibList = new List<Program.accountList>();
            MySqlConnection conn = connMySQL("ci_broker_" + broker_product.bId);
            MySqlCommand cmd;
            foreach (accountList accountValue in accountList)
            {
                string cmdText = "SELECT * FROM client_product WHERE apply_account = 1 AND introducer_IB_Code = " + accountValue.ib_mt4_login + " AND pId = " + broker_product.id;
                cmd = new MySqlCommand(cmdText, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ibList.Add(
                            new Program.accountList()
                            {
                                ib_mt4_login = Convert.ToInt32(reader["mt4_login"]),
                                broker_productList = broker_product,
                            }
                        );
                    }
                }
            }
            
            conn.Close();
            return ibList;
        }

        private string run_calc_commission(accountList account)
        {
            string clientlist = "";
            //foreach(clientList List in account.clientList)
            //{
            //    clientlist = clientlist + List.client_mt4_login + ",";
            //}
            string Api_Url = "http://mt4api.4webdemo.com/";
            HttpClient client = new HttpClient();
            var json_content = new Dictionary<string, string>
            {
                ["servername"] = account.broker_productList.servername,
                ["broker_mt4_login"] = account.ib_mt4_login.ToString(),
                ["broker_mt4_commission_percent"] = account.broker_productList.mt4_commission_percent.ToString(),
                ["lot_commission"] = account.broker_productList.lot_commission.ToString(),
                ["ib_mt4_login"] = account.ib_mt4_login.ToString(),
                ["clientlist"] = clientlist
            };
            var postData = new FormUrlEncodedContent(json_content);
            var post = client.PostAsync(Api_Url + "Webapi/CalculationLot", postData).Result;
            string responseBody = post.Content.ReadAsStringAsync().Result;
            var responseList = JsonConvert.DeserializeObject<dynamic>(responseBody); //memberId,account,password
            return responseList;
        }

        public class broker_productList
        {
            public string servername { get; set; }
            public int bId { get; set; }
            public int id { get; set; }
            public int mt4_commission_account { get; set; }
            public int lot_commission { get; set; }
            public int mt4_commission_percent { get; set; }
        }

        public class accountList
        {
            public broker_productList broker_productList { get; set; }
            public int ib_mt4_login { get; set; }
        }

        public class clientList
        {
            public int client_mt4_login { get; set; }
        }
    }
}
