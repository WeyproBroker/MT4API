//+------------------------------------------------------------------+
//|                                  MetaTrader 4 Manager API Sample |
//|                   Copyright 2001-2014, MetaQuotes Software Corp. |
//|                                        http://www.metaquotes.net |
//+------------------------------------------------------------------+
#include "stdafx.h"
#include "ManagerAPISample.h"
#include <string>
using namespace std;
//---
CManagerInterface *ExtManager = NULL;
CManagerInterface *ExtDealer = NULL;
CManagerInterface *ExtManagerPump = NULL;
volatile UINT      ExtPumpingMsg = 0;
volatile UINT      ExtDealingMsg = 0;
LPCSTR path_cstr();
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
BEGIN_MESSAGE_MAP(CManagerAPISampleApp, CWinApp)
	//{{AFX_MSG_MAP(CManagerAPISampleApp)
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CManagerAPISampleApp::CManagerAPISampleApp() : m_factory("C:\\Windows\\mtmanapi.dll")
{
	CManagerAPISampleApp::InitInstance();
}

//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
CManagerAPISampleApp theApp;
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
BOOL CManagerAPISampleApp::InitInstance()
{
	//---
	if (!AfxSocketInit())
	{
		AfxMessageBox(IDP_SOCKETS_INIT_FAILED);
		return(FALSE);
	}
	//--- internal message
	//--- create manager api instance
	if (m_factory.IsValid() == FALSE
		|| (ExtManager = m_factory.Create(ManAPIVersion)) == NULL
		|| (ExtManagerPump = m_factory.Create(ManAPIVersion)) == NULL
		|| (ExtDealer = m_factory.Create(ManAPIVersion)) == NULL)
	{
		return(FALSE);
	}
	//---
	//---
	return(FALSE);
}
//+------------------------------------------------------------------+
//|                                                                  |
//+------------------------------------------------------------------+
int CManagerAPISampleApp::ExitInstance()
{
	//---
	if (ExtManager != NULL) { ExtManager->Release();     ExtManager = NULL; }
	if (ExtDealer != NULL) { ExtDealer->Release();      ExtDealer = NULL; }
	if (ExtManagerPump != NULL) { ExtManagerPump->Release(); ExtManagerPump = NULL; }
	//---
	return CWinApp::ExitInstance();
}
//+------------------------------------------------------------------+
/*** 登入 ***/
extern "C" __declspec(dllexport) void Connect()
{
	CManagerAPISampleApp();
	LPCSTR server;
	std::string str = "103.30.69.22:446";
	server = (LPCSTR)str.c_str();
	//char server[128] = "103.30.69.22:446";
	ExtManager->Connect(server);

	/*** 登入 ****/
	LPCSTR password;
	std::string pass = "Report123!@#";
	password = (LPCSTR)pass.c_str();
	ExtManager->Login(15, password);	
}

/*** 取得用戶詳細資料 ****/
extern "C" __declspec(dllexport) string GetUserDetail(int account)
{
	Connect();
	int users_total = 1;
	int login = account;
	UserRecord *user = ExtManager->UserRecordsRequest(&login, &users_total);
	string str_login = to_string(user->login);
	string str_name = user->name;
	string str_group = user->group;
	string str_leverage = to_string(user->leverage);
	string str_agent = to_string(user->agent_account);
	string str_email = user->email;
	string str_comment = user->comment;
	string str_enable = to_string(user->enable);
	string str_read_only = to_string(user->enable_read_only);
	string str_report = to_string(user->send_reports);
	string str_format = "login=" + str_login + ",name=" + str_name + ",group=" + str_group + ",leverage=" + str_leverage + ",agent=" + str_agent + ",email=" + str_email + ",comment=" + str_comment + ",enable=" + str_enable + ",read_only=" + str_read_only + ",report=" + str_report;
	CHAR *data = new char[str_format.length() + 1];
	strcpy_s(data, str_format.length() + 1, str_format.c_str());
	return data;
}
/**** 檢查用戶密碼 ****/
extern "C" __declspec(dllexport) int CheckPassword(int account, char password[16])
{
	Connect();
	std::string pass = "";
	int res = RET_ERROR;
	for (int i = 0; i < 16; i++) 
	{
		pass += password[i];
	}
	LPCSTR data;
	data = (LPCSTR)pass.c_str();
	res = ExtManager->UserPasswordCheck(account, data);
	return res;
}

/*** 註冊帳號 ***/
extern "C" __declspec(dllexport) int AddUser(int account, char name[128], char group[16], int leverage, char password[16], char password_investor[16], char email[48], char country[32], char state[32], char city[32], char address[128], char comment[64], char phone[32], char zipcode[16])
{
	Connect();
	int res = RET_ERROR;
	UserRecord user = { 0 };
	//for (int i = 1000; i < 100000; i++)
	//{
	//	int check;
	//	check = GetUser(i);
	//	/*** 判斷帳號有沒有被使用 ***/
	//	if (check == 0)
	//	{
	//		user.login = i;
	//		break;
	//	}
	//}
	user.login = account;
	for (int i = 0; i < 128; i++)
	{
		user.name[i] = name[i];
	}
	for (int i = 0; i < 16; i++)
	{
		user.group[i] = group[i];
	}
	user.leverage = leverage;
	for (int i = 0; i < 16; i++)
	{
		user.password[i] = password[i];
	}
	for (int i = 0; i < 16; i++)
	{
		user.password_investor[i] = password_investor[i];
	}
	for (int i = 0; i < 48; i++)
	{
		user.email[i] = email[i];
	}
	for (int i = 0; i < 32; i++) 
	{
		user.country[i] = country[i];
	}
	for (int i = 0; i < 32; i++)
	{
		user.state[i] = state[i];
	}
	for (int i = 0; i < 32; i++)
	{
		user.city[i] = city[i];
	}
	for (int i = 0; i < 128; i++)
	{
		user.address[i] = address[i];
	}
	for (int i = 0; i < 64; i++)
	{
		user.comment[i] = comment[i];
	}
	for (int i = 0; i < 32; i++)
	{
		user.phone[i] = phone[i];
	}
	for (int i = 0; i < 16; i++)
	{
		user.zipcode[i] = zipcode[i];
	}
	user.enable = TRUE;
	user.send_reports = TRUE;
	user.user_color = USER_COLOR_NONE;
	res = ExtManager->UserRecordNew(&user);
	LPCSTR ErrorMessage = ExtManager->ErrorDescription(res);
	return res;
}

/*** 取得使用者 ***/
extern "C" __declspec(dllexport) int GetUser(int account)
{
	Connect();
	int users_total = 1;
	int login = account;
	UserRecord *user = ExtManager->UserRecordsRequest(&login, &users_total);
	return users_total;
}

/**** 帳號資料修改 ****/
extern "C" __declspec(dllexport) int UpdateUser(int account, char name[128], char group[16], int leverage, char password[16], char password_investor[16], char email[48], char country[32], char state[32], char city[32], char address[128], char comment[64], char phone[32], char zipcode[16])
{
	Connect();
	UserRecord user = { 0 };
	user.login = account;
	for (int i = 0; i < 128; i++)
	{
		user.name[i] = name[i];
	}
	for (int i = 0; i < 16; i++)
	{
		user.group[i] = group[i];
	}
	user.leverage = leverage;
	for (int i = 0; i < 16; i++)
	{
		user.password[i] = password[i];
	}
	for (int i = 0; i < 16; i++)
	{
		user.password_investor[i] = password_investor[i];
	}
	for (int i = 0; i < 48; i++)
	{
		user.email[i] = email[i];
	}
	for (int i = 0; i < 32; i++) 
	{
		user.country[i] = country[i];
	}
	for (int i = 0; i < 32; i++)
	{
		user.state[i] = state[i];
	}
	for (int i = 0; i < 32; i++)
	{
		user.city[i] = city[i];
	}
	for (int i = 0; i < 128; i++)
	{
		user.address[i] = address[i];
	}
	for (int i = 0; i < 64; i++)
	{
		user.comment[i] = comment[i];
	}
	for (int i = 0; i < 32; i++)
	{
		user.phone[i] = phone[i];
	}
	for (int i = 0; i < 16; i++)
	{
		user.zipcode[i] = zipcode[i];
	}
	
	int res = ExtManager->UserRecordUpdate(&user);
	return res;
}


extern "C" __declspec(dllexport) CHAR* test(int account)
{
	Connect();
	int users_total = 1;
	int login = account;
	UserRecord *user = ExtManager->UserRecordsRequest(&login, &users_total);
	string str_login = to_string(user->login);
	string str_name = user->name;
	string str_group = user->group;
	string str_leverage = to_string(user->leverage);
	string str_agent = to_string(user->agent_account);
	string str_email = user->email;
	string str_comment = user->comment;
	string str_enable = to_string(user->enable);
	string str_read_only = to_string(user->enable_read_only);
	string str_report = to_string(user->send_reports);
	string str_format = "login=" + str_login + ",name=" + str_name + ",group=" + str_group + ",leverage=" + str_leverage + ",agent=" + str_agent + ",email=" + str_email + ",comment=" + str_comment + ",enable=" + str_enable + ",read_only=" + str_read_only + ",report=" + str_report;
	CHAR *data = new char[str_format.length() + 1];
	strcpy_s(data, str_format.length() + 1, str_format.c_str());
	return data;
}

/***** 交易手續【出金、入金、內轉】 *****/
extern "C" __declspec(dllexport) int Transaction(int account, int amount, char comment[32])
{
	Connect();
	TradeTransInfo info = { 0 };
	char tmp[32];
	info.type = TT_BR_BALANCE;
	info.cmd = OP_BALANCE;
	info.orderby = account;
	info.price = double(amount);
	for (int i = 0; i < 32; i++) 
	{
		info.comment[i] = comment[i];
	}
	int res = ExtManager->TradeTransaction(&info);
	return res;
}