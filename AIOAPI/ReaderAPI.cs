using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;

namespace AIOAPI
{
    [SecurityCritical]
    public class ReaderAPI
    {
        public ReaderAPI()
        {

        }

        [StructLayout(LayoutKind.Sequential,Pack=1)]
        public struct CardConsume
        {
            public UInt32 AccountNo;/*帐号*/
            public UInt32 CardNo;/*卡号*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string FinancingType;/*务类型*/
            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            //public char[] FinancingType;

            public Int32 CardBalance;/*卡余额，精确至分*/
            public Int32 TranAmt;/*交易额，精确至分*/
            public UInt16 UseCardNum;/*用卡此时，交易前*/
            public UInt16 TerminalNo;/*终端编号*/

            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 7)]
            public string PassWord;/*卡密码*/
            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 7)]
            //public char[] PassWord;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string Operator;/*操作员*/
            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            //public char[] Operator;


            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 129)]
            public string Abstract;/*摘要*/

            //[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 129)]
            //public char[] Abstract;/*摘要*/

            public Int32 TranJnl;/*交易流水号*/
            public Int32 BackJnl;/*后台交易流水号*/
            public Int16 RetCode;/*后台处理返回值*/
        };
        //[StructLayout(LayoutKind.Explicit)]
        //public struct CardConsume
        //{

        //    [FieldOffset(0)]
        //    public Int32 AccountNo;/*帐号*/
        //    [FieldOffset(4)]
        //    public UInt32 CardNo;/*卡号*/
        //    [FieldOffset(8)]
        //    public string FinancingType;/*财务类型*/
        //    [FieldOffset(12)]
        //    public Int32 CardBalance;/*卡余额，精确至分*/
        //    [FieldOffset(20)]
        //    public Int32 TranAmt;/*交易额，精确至分*/
        //    [FieldOffset(24)]
        //    public short UseCardNum;/*用卡此时，交易前*/
        //    [FieldOffset(28)]
        //    public short TerminalNo;/*终端编号*/

        //    //[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 6)]
        //    [FieldOffset(36)]
        //    public string PassWord;/*卡密码*/
        //    //[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 3)]
        //    [FieldOffset(40)]
        //    public string Operator;/*操作员*/
        //    [FieldOffset(44)]
        //    //[MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 131)]
        //    public string Abstract;/*摘要*/
        //    [FieldOffset(168)]
        //    public int TranJnl;/*交易流水号*/
        //    [FieldOffset(172)]
        //    public int BackJnl;/*后台交易流水号*/
        //    [FieldOffset(176)]
        //    public short RetCode;/*后台处理返回值*/
        //};

        /*long c++(4个字节，c#字节) -> int*/
        /*char->byte(c中1个字节，c#2个字节) */
        [StructLayout(LayoutKind.Sequential)]
        public struct AccountMsg
        {
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string Name;/*姓名*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 2)]
            public string SexNo;/*性别*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 19)]
            public string DeptCode;/*部门代码*/
            public UInt32 CardNo;/*卡号*/
            public UInt16 AccountNo;/*帐号*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string StudentCode;/*学号*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 21)]
            public string IDCard;/*身份证号*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string PID;/*身份代码*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 13)]
            public string IDNo;/*身份序号*/
            public Int32 Balance;/*现余额*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 7)]
            public string Password;/*消费密码*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 7)]
            public string ExpireDate;/*账户截止日期*/
            public UInt16 SubSeq;/*补助戳*/
            public SByte IsOpenInSys;//char IsOpenInSys;/*是否在本系统内开通*/
            public Int16 TerminalNo;/*终端号码,提取补助时需要填写*/
            public Int16 Retcode;/*后台处理返回值*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Flag;/*状态(2004-08-26增加)*/
#if JIAXING_THIRD //嘉兴学院版本嘉兴学院版本
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst =17)]
            public string QueryPin;//查询密码
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst =21)]
            public string BankAcc;//银行帐号
            public int TmpBalance;//当前过渡余额
            public int PreTemBalance;//上次过渡余
            public int TranLimit;//自动转账警戒额
            public int TransferAmt;//自动转账金额
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst =30)]
            public string Pad;//预留字段
#else
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string CardType;/*卡类型*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 4)]
            public string AccType;/*电子账户类型，如果输入则会查询相应的电子帐户余额*/
            public UInt16 UsedCardNum;/*卡片上的用卡次数*/
            public int AccAmt;/*精确查询时根据输入的AccType查询到的电子帐户余额*/
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 70)]
            public string Pad;/*预留字段*/
#endif
        };

      


        #region dll importer
        /*EXTC int  WINAPI TA_Init3(char *IP, short port, unsigned short SysCode, unsigned short TerminalNo,
						   bool *ProxyOffline, ULONG *MaxJnl, char* signonPWD);*/
        [DllImport("AIO_API.dll", EntryPoint = "TA_Init3", SetLastError = true,
           CharSet = CharSet.Ansi, ExactSpelling = false,
           CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_Init3(string IP, Int16 port, Int16 SysCode, Int16 TerminalNo, ref Int16 ProxyOffline, ref UInt32 MaxJnl, string signonPWD);

        [DllImport("AIO_API.dll", EntryPoint = "TA_Init", SetLastError = true,
           CharSet = CharSet.Ansi, ExactSpelling = false,
           CallingConvention = CallingConvention.StdCall)]
        public static extern bool TA_Init(string IP, Int16 port, Int16 SysCode, Int16 TerminalNo, ref Int16 ProxyOffline, ref UInt32 MaxJnl);

        //[DllImport("AIO_API.dll", EntryPoint = "TA_CRInit", SetLastError = true,
        //    CharSet = CharSet.Auto, ExactSpelling = false,
        //    CallingConvention = CallingConvention.StdCall)]
        ///初始化读卡器
        //EXTC int _stdcall TA_CRInit(char CardReaderType,int port,long Baud_Rate);
        [DllImport("AIO_API.dll", EntryPoint = "TA_CRInit", CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_CRInit(int CardReaderType, Int32 port, Int32 Baud_Rate);
        //public static extern int TA_CRInit(int CardReaderType, int port, long Baud_Rate);

        //EXTC BOOL _stdcall TA_CRClose(void);
        [DllImport("AIO_API.dll", EntryPoint = "TA_CRClose", SetLastError = true,
            CharSet = CharSet.Auto, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern Int16 TA_CRClose();

        //EXTC int _stdcall TA_CRBeep(unsigned int BeepMSecond);
        [DllImport("AIO_API.dll", EntryPoint = "TA_CRBeep", SetLastError = true,
            CharSet = CharSet.Auto, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_CRBeep(uint BeepMSecond);

        //EXTC int _stdcall TA_FastGetCardNo(unsigned int *CardNo);
        [DllImport("AIO_API.dll", EntryPoint = "TA_FastGetCardNo", SetLastError = true,
            CharSet = CharSet.Auto, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_FastGetCardNo(ref UInt32 CardNo);


        //EXTC int _stdcall TA_ReadCardSimple(AccountMsg * pAccMsg);
        [DllImport("AIO_API.dll", EntryPoint = "TA_ReadCardSimple", SetLastError = true,
            CharSet = CharSet.Auto, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_ReadCardSimple(ref AccountMsg CardNo);

        //读卡并且验证白名单
        //EXTC int _stdcall TA_ReadCard(AccountMsg *pAccMsg,bool CheckID=true ,bool CheckSub= false);
        [DllImport("AIO_API.dll", EntryPoint = "TA_ReadCard", SetLastError = true,
            CharSet = CharSet.Auto, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_ReadCard(ref AccountMsg CardNo, bool CheckID = true, bool CheckSub = false);

        ///////////////////////////////////////卡片消费交易类的API函数/////////////////////////////////////////
        /// <summary>
        /// 卡片消费(可以脱机)
        /// </summary>
        /// <param name="pCardCons">入口：CardNo 消费卡片的卡号；Operator 填写两个字节的操作员代码；TranAmt 卡片消费的交易额，必须小于0；TranJnl 流水号</param>
        /// <param name="pCardCons">出口：Code 后台交易的返回值，可以从控制文件中查询得到；BackJnl 交易的后台流水号； Balance 卡片余额</param>
        /// <param name="IsVerfy">是否验证消费限额</param>
        /// <param name="TimeOut">交易超时时间，缺省为10秒</param>
        /// <returns>见返回值列表Errormsg.h</returns>
        [DllImport("AIO_API.dll", EntryPoint = "TA_Consume", SetLastError = true, CharSet = CharSet.Auto, ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_Consume(ref CardConsume pCardCons, bool IsVerfy, Int16 TimeOut = 10);
        //  int _stdcall TA_Consume(CardConsume *pCardCons, bool IsVerfy, short TimeOut=10);

        [DllImport("AIO_API.dll", EntryPoint = "TA_InqAcc", SetLastError = true,
            CharSet = CharSet.Auto, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int TA_InqAcc(ref AccountMsg pAccMsg, short TimeOut = 10);


        [DllImport("AIO_API.dll", EntryPoint = "TA_InqAcc", SetLastError = true,
            CharSet = CharSet.Auto, ExactSpelling = false,
            CallingConvention = CallingConvention.StdCall)]
      public static extern  int TA_Refund (ref CardConsume pCardCons, short TimeOut=10);


        #endregion

    }
}
