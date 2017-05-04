using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AIOAPI
{
    public class Read
    {
        public Read()
        {

        }
        public static bool PortIsOpen(string IpStr,int port)
        {
            IPAddress ip = IPAddress.Parse(IpStr);
            IPEndPoint point = new IPEndPoint(ip,port);
            try
            {
                TcpClient tcp = new TcpClient();
                tcp.Connect(point);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    //    string b;
    //    string ip = ConfigurationManager.AppSettings["SIOS_IP"].ToString();//IP-第三方代理服务器SIOS的IP地址,
    //    Int16 port = Convert.ToInt16(ConfigurationManager.AppSettings["port"].ToString());
    //    //  ushort port = 0;//port-第三方代理服务器sios 的端口号
    //    //int str_code = 5;
    //    //short Timeout = 10;
    //    UInt16 syscode = Convert.ToUInt16(ConfigurationManager.AppSettings["SYSCODE"].ToString());//SysCode-给第三方系统分配的系统代码,
    //    //Int16 syscode = Convert.ToInt16(ConfigurationManager.AppSettings["SYSCODE"].ToString());
    //    Int16 Timeout = Convert.ToInt16(ConfigurationManager.AppSettings["timeout"].ToString());//缺省时间
    //    UInt16 TerMinalNo = Convert.ToUInt16(ConfigurationManager.AppSettings["terminalno"].ToString());//TerminalNo -默认1
    //    //Int16 TerMinalNo = Convert.ToInt16(ConfigurationManager.AppSettings["terminalno"].ToString());
    //    //初始化卡片信息
    //    bool proxy ;//代理服务是否脱机  
    //    UInt64 max;
    //    UInt32 g_uJnl;
    //    public Read()
    //    {
    //        //ushort us = (ushort)syscode;

    //      //nRet=  ReaderAPI.TA_Init(ip, port, syscode, TerMinalNo, ref proxy, ref max);
    //        g_uJnl =(UInt32) ReaderAPI.TA_Init3(ip, port, syscode, TerMinalNo, ref proxy, ref max, "123");

    //        //nRet = ReaderAPI.TA_Init3("172.16.200.15", 8500, 18, 1, ref proxy, ref max, "123");
    //    }
    //    //动态初始化卡片库
    //    public void IniCard()
    //    {
    //       // char cardreaderType = '1';//0为usb类型读卡器，1为串口读卡器
    //        Int16 cardreaderType = Convert.ToInt16(ConfigurationManager.AppSettings["cardreaderType"].ToString());
    //        // int port = 0;//cardport-串口读卡器的串口号，0～1分别代表串口1~4。
    //        int cardport = Convert.ToInt32(ConfigurationManager.AppSettings["cardport"].ToString());
    //        //int Raud_rate = 19200;//Baud_Rate-串口读卡器的波特率。
    //        int Raud_rate = Convert.ToInt32(ConfigurationManager.AppSettings["Raud_rate"].ToString());
    //        //ReaderAPI.TA_CRInit(cardreaderType, cardport, Raud_rate);
    //        int Ret;
    //        Ret = ReaderAPI.TA_CRInit(cardreaderType, cardport, 19200);
    //        ReaderAPI.TA_CRBeep(50);
    //        if (Ret != 0)
    //        {
    //            ReaderAPI.TA_CRClose();
    //            ReaderAPI.TA_CRBeep(50);
    //            ReaderAPI.TA_CRInit(1, 0, 19200);
    //        }
    //        //UInt32 CardNo = 0000;
    //        //ReaderAPI.TA_FastGetCardNo(ref CardNo);
    //        //ReaderAPI.CardConsume cc = new ReaderAPI.CardConsume();
    //        //cc.AccountNo = 70004;
    //        //cc.CardNo = 701985094;
    //        ////cc.CardNo = 1900418624;
    //        //cc.TranJnl = ++g_uJnl;//(从init里面返回的值，保证流水号不重复)
    //        //cc.TranAmt = -1;//卡片消费金额必须小于0
    //        //cc.TerminalNo = 1;
    //        //cc.Operator = "01";
    //        //int et = ReaderAPI.TA_Consume(ref cc, true);
    //        //getInfo(CardNo);
    //        //int test = et;
    //        //Console.WriteLine(test.ToString());
    //    }
    //    //关闭读卡
    //    public void CloseCard()
    //    {
    //        ReaderAPI.TA_CRClose();
    //    }
    //    public void getCRBeep(UInt32 beep)
    //    {
    //        ReaderAPI.TA_CRBeep(beep);
    //    }
    //    //快读读取卡片序列号
    //    public int getReadCard()
    //    {
    //        uint CardNo = 0000;
    //        ReaderAPI.TA_FastGetCardNo(ref CardNo);
    //        return getInfo(CardNo);
    //    }
    //    //读取卡片信息.如果读取卡片序列号不能使用就使用这个，返回AccoutMsg
    //    ReaderAPI.AccountMsg msg = new ReaderAPI.AccountMsg();
    //    public bool getReadCardSimple()
    //    {
    //        //ReaderAPI.AccountMsg msg = new ReaderAPI.AccountMsg();
    //        ReaderAPI.TA_ReadCardSimple(ref msg);
    //        AccountMsg.Name = msg.Name;
    //        AccountMsg.IDCard = msg.IDCard;
    //        AccountMsg.Balance = msg.Balance;
    //        AccountMsg.flag = msg.Flag;
    //        AccountMsg.CardNo = msg.CardNo;
    //        AccountMsg.IDCard = msg.IDCard;
    //        AccountMsg.StudentCode = msg.StudentCode; 
    //        return AccountMsg.StudentCode == "" ? false : true;
    //    }

    //    //根据卡片序列号读取卡片信息
    //    private int getInfo(uint CardNo)
    //    {
    //        ReaderAPI.AccountMsg msg = new ReaderAPI.AccountMsg();
    //        msg.CardNo = CardNo;
    //       // msg.StudentCode = "201435020050";
    //        int i=  ReaderAPI.TA_InqAcc(ref msg);//根据卡片序列号查询信息
    //        AccountMsg.PID = msg.PID;
    //        AccountMsg.Name = msg.Name;
    //        AccountMsg.IDCard = msg.IDCard;
    //        AccountMsg.Balance = msg.Balance;
    //        AccountMsg.flag = msg.Flag;
    //        AccountMsg.CardNo = msg.CardNo;
    //        AccountMsg.AccountNo = msg.AccountNo;
    //        AccountMsg.StudentCode = msg.StudentCode;
    //      return  i;
    //    }
    //    //验证密码
    //    public bool Checkpwd(uint accountNo, string password)
    //    {
    //        int i = ReaderAPI.TA_CheckQpwd(accountNo, password);
    //        if (i == 0)
    //        {
    //            return true;
    //        }
    //        return false;
    //    }
    //    //卡片消费信息
    //    public void setReadCard(Int32 money)
    //    {
    //        IniCard();
    //        UInt32 CardNo = 0000;
    //        ReaderAPI.TA_FastGetCardNo(ref CardNo);
    //        getInfo(CardNo);
    //        ReaderAPI.CardConsume cc = new ReaderAPI.CardConsume();
    //        cc.AccountNo = 70004;
    //        cc.CardNo = msg.CardNo;
    //        //cc.CardNo = 1900418624;
    //       // cc.TerminalNo = 2;
    //        //cc.TranJnl = 1111;
    //        cc.TranJnl = ++g_uJnl;//(从init里面返回的值，保证流水号不重复)
    //        cc.TranAmt = -1;//卡片消费金额必须小于0
    //        //cc.TranAmt=money;
    //        cc.TerminalNo = 1;
    //        cc.Operator = "01";
    //       // CloseCard();
    //        //cc.TranJnl =(UInt32)nRet;/*从TA_Init()取得最大流水号，然后依次递增，保证最近发送的流水不重复发送，如果流水重复,cc.RetCode==-2*/
    //        try
    //        {
    //            int et = ReaderAPI.TA_Consume(ref cc, true);
    //            //int xx = ReaderAPI.TA_Refund(ref cc);
    //            int test = et;
    //            Console.WriteLine(test.ToString());
    //        }
    //        catch (Exception ex)
    //        {
    //            throw new Exception(ex.Message);
    //        }
    //        finally {
    //            CloseCard();
    //        }
            
    //        //卡片消费
    //    }
    }
}
