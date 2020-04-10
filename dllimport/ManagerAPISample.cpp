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
CManagerAPISampleApp::CManagerAPISampleApp() : m_factory("C:\\Windows\\System32\\mtmanapi.dll")
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
extern "C" __declspec(dllexport) void Connect();
extern "C" __declspec(dllexport) string GetUserDetail(int account);
extern "C" __declspec(dllexport) int CheckPassword(int account, char password[16]);
extern "C" __declspec(dllexport) int AddUser(int account, char name[128], char group[16], int leverage, char password[16], char password_investor[16], char email[48], char country[32], char state[32], char city[32], char address[128], char comment[64], char phone[32], char zipcode[16]);
extern "C" __declspec(dllexport) int GetUser(int account);
extern "C" __declspec(dllexport) int UpdateUser(int account, char name[128], char group[16], int leverage, char password[16], char password_investor[16], char email[48], char country[32], char state[32], char city[32], char address[128], char comment[64], char phone[32], char zipcode[16]);
extern "C" __declspec(dllexport) int test();
//extern "C" __declspec(dllexport) int Withdraw();
/*** 登入 ***/
void __declspec(dllexport) Connect() 
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
string __declspec(dllexport) GetUserDetail(int account) 
{
	char data[10000] = { 0 };
	char name[128] = { 0 };
	Connect(); 
	int users_total = 1;
	int login = account;
	UserRecord *user = ExtManager->UserRecordsRequest(&login, &users_total);
	for (int i = 0; i < 128; i++) 
	{
		name[i] = user->name[i];
	}
	return name;
}
/**** 檢查用戶密碼 ****/
int __declspec(dllexport) CheckPassword(int account, char password[16]) 
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
int __declspec(dllexport) AddUser(int account, char name[128], char group[16], int leverage, char password[16], char password_investor[16], char email[48], char country[32], char state[32], char city[32], char address[128], char comment[64], char phone[32], char zipcode[16])
{
	Connect();
	///*** PHP輸入的值 ***/
	//std::string account = "34";
	//std::string password = "www123";
	//std::string name = "eee";
	//std::string group = "manager";
	//std::string email = "rrr@qwe.com";

	//char array_password[16] = { 0 };
	//char array_name[128] = { 0 };
	//char array_group[16] = { 0 };
	//char array_email[48] = { 0 };

	//strcpy_s(array_password, password);
	//strcpy_s(array_name, name);
	//strcpy_s(array_group, group);
	//strcpy_s(array_email, email);

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
int __declspec(dllexport) GetUser(int account) 
{
	Connect();
	int users_total = 1;
	int login = account;
	UserRecord *user = ExtManager->UserRecordsRequest(&login, &users_total);
	return users_total;
}

/**** 帳號資料修改 ****/
int __declspec(dllexport) UpdateUser(int account, char name[128], char group[16], int leverage, char password[16], char password_investor[16], char email[48], char country[32], char state[32], char city[32], char address[128], char comment[64], char phone[32], char zipcode[16])
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

int __declspec(dllexport) test() 
{
	return 2;
}
