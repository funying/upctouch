﻿using AIOAPI;
using CAMAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using Wpf.Controls;

namespace Wpf
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
        }

        int CardReaderIntervel = Convert.ToInt32(Tool.GetConfigKeyValue("CardReaderIntervel"));
        log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        //全局变量定义
        //每个月金额
        bool checkSub = Boolean.Parse(Tool.GetConfigKeyValue("CHECK_SUB"));
        bool checkTempPack = Boolean.Parse(Tool.GetConfigKeyValue("CheckTempPack"));
        double payMonth = Convert.ToDouble(Tool.GetConfigKeyValue("COST_MONTH"));
        string SAMIP = Tool.GetConfigKeyValue("samIP");
        int SAMPORT = Convert.ToInt32(Tool.GetConfigKeyValue("samPORT"));
        FundAPI fd = new FundAPI();
        ReaderAPI.AccountMsg msg = new ReaderAPI.AccountMsg();
        DispatcherTimer dTimer = new System.Windows.Threading.DispatcherTimer();
        Random radom = new Random();
        string templateList = Tool.GetConfigKeyValue("TemplateList");
        string packageList = Tool.GetConfigKeyValue("PackageList");
        string MODE = Tool.GetConfigKeyValue("MODE");
        bool mb = false;
        double db,moneyToPay;
        public DispatcherTimer dtReadCard = null;
        private Boolean isCharging = false;
        static object lock_tick = new object();
        
        void dtReadCard_Tick(object sender, EventArgs e) 
        {
            lock (lock_tick)
            {
                var task = new Task<int>(() =>
                {
                    //把耗时的操作放到这里 处理好返回结果
                    return ReadCard();
                });
                task.ContinueWith(t =>
                {
                    //进行一些逻辑处理，也可以放到上面的Task中
                    //在把结果显示到界面 可以传参什么的
                    this.Dispatcher.BeginInvoke(new Action<int>((result) =>
                    {
                        TimerSubWork(result);
                    }), t.Result);
                });
                task.Start();
            }
        }

        private int TwoServerOnlineCheck()
        {
            //bool read = Read.PortIsOpen(SAMIP,SAMPORT);
            //500ms如果返回没有成功则为失败
            Telnet samT = new Telnet(SAMIP, SAMPORT, 500);
            bool checkSam = samT.Connect();
            string SIOS_IP = Tool.GetConfigKeyValue("SIOS_IP");
            Telnet SIOST = new Telnet(SIOS_IP,8500, 500);
            bool checkSIOS = SIOST.Connect();
            if (checkSam && checkSIOS)
            {
                return 0;
            }
            else if (checkSam == true && checkSIOS == false)
            {
                return 9114;
            }
            else if (checkSam == false && checkSIOS == true)
            {
                return 9115;
            }
            else
            {
                return 9116;
            }
        }
        private int ReadCard()
        {
            try
            {
                msg.TerminalNo = short.Parse(Tool.GetConfigKeyValue("TERMINAL_NO"));
                int retCode = ReaderAPI.TA_ReadCardSimple(ref msg);
                log.Info("读卡返回:" + retCode);
                if (Tool.IsCardReaderExc && (retCode == 0 || retCode == -1222 || retCode == -1223))
                {
                    return 0;
                }
                isCharging = false;

                if (0 == retCode || retCode == -1222 || retCode == -1223)
                {
                    if (!Tool.IsCardReaderExc)
                    {
                        ReaderAPI.TA_CRBeep(300);
                        if (!Tool.NetWorkValidBeforeCharge)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    key0k.IsEnabled = false;
                                    key0k.Content = "网络联通检测..";
                                }));
                            });
                            //双网络畅通性检测
                            int checkTwoServer = TwoServerOnlineCheck();
                            if (checkTwoServer != 0)
                            {
                                return checkTwoServer;
                            }
                            else
                            {
                                Tool.NetWorkValidBeforeCharge = true;
                                Task.Factory.StartNew(() =>
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                    {
                                        key0k.IsEnabled = true;
                                        key0k.Content = "充值";
                                    }));
                                });
                            }
                        }
                    }
                    Tool.IsCardReaderExc = true;//成功读取到卡片信息

                    Task.Factory.StartNew(() =>
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            TotalToPay.Text = "0.00";
                            UcardBalance.Text = "读取中..";
                            UBalance.Text = "读取中..";
                            UstudentCode.Text = "读取中..";
                            Uname.Text = "读取中..";
                            Usate.Text = "读取中..";

                        }));
                    });


                    Tool.StudentCode = msg.StudentCode;
                    Tool.StudentName = msg.Name;
                    string balance = (Math.Abs(msg.Balance % 100)).ToString();
                    if (balance.Length == 1)
                    {
                        balance = "0" + balance;
                    }
                    Tool.CardBalance = msg.Balance / 100 + "." + balance;
                    log.Info(string.Format("当前用户{0},用户名{1},账户余额{2}(元)", msg.StudentCode, msg.Name, Tool.CardBalance));
                    if (!string.IsNullOrEmpty(Tool.StudentCode))
                    {
                        string logs = string.Format("query user:" + msg.StudentCode);
                        log.Info(logs);
                        QueryUserParams quparam = new QueryUserParams();
                        quparam.userId = Tool.StudentCode;
                        quparam.reserved0 = "disableUserIdLikeCondition";
                        QueryUserResult quResult = fd.queryUser(quparam);
                        if (quResult.data != null)
                        {
                            Tool.AccountState = quResult.data.getAccountState(quResult.data.accountState);//状态
                            Tool.NetBalance = quResult.data.accountFee.ToString();//账户余额
                            if (!string.IsNullOrEmpty(quResult.data.atName))
                            {
                                Tool.AtName = quResult.data.atName;
                            }
                            else
                            {
                                Tool.AtName = "无模版信息";
                            }

                            if (!string.IsNullOrEmpty(quResult.data.packageName))
                            {
                                Tool.PackageName = quResult.data.packageName;
                            }
                            else
                            {
                                Tool.PackageName = "无套餐信息";
                            }

                            string[] templateArr = templateList.Split('|');
                            string[] packageArr = packageList.Split('|');
                            Boolean isTempLimited = false, isPackLimited = false;
                            for (int i = 0; i < templateArr.Length; i++)
                            {
                                if (quResult.data.atName == templateArr[i])
                                {
                                    isTempLimited = true;
                                    break;
                                }
                            }

                            for (int j = 0; j < packageArr.Length; j++)
                            {
                                if (quResult.data.packageName == packageArr[j])
                                {
                                    isPackLimited = true;
                                    break;
                                }
                            }

                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });

                            if (!isTempLimited && !isPackLimited && checkTempPack)
                            {
                                return -9009;
                            }
                            log.Info("user state:" + quResult.data.accountState.ToString());
                            if (MODE.ToUpper() == "SINGLE" && (quResult.data.accountState == 3 || quResult.data.accountState == 2 || quResult.data.accountState == 100))
                            {
                                return -9111;//单例而且欠费
                            }
                            else if (MODE.ToUpper() == "SINGLE" && quResult.data.accountState != 0)
                            {
                                return -9112;
                            }
                        }
                        else
                        {
                            if (quResult.errorCode == 0)
                            {

                                Tool.IsCardReaderExc = false;
                                return -9001;
                            }
                            else
                            {
                                Tool.IsCardReaderExc = false;
                                return -9002;
                            }
                        }
                    }
                    else
                    {
                        Tool.IsCardReaderExc = false;
                        return -9003;
                    }
                }
                else
                {
                    msg = new ReaderAPI.AccountMsg();//没读到卡，直接清空数据
                    Tool.IsCardReaderExc = false; //解锁
                    Tool.NetWorkValidBeforeCharge = false;
                    Task.Factory.StartNew(() =>
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            key0k.IsEnabled = false;//没有读到卡片的时候，按钮处置为灰色
                        }));
                    });
                }
                return retCode;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                Tool.NetWorkValidBeforeCharge = false;
                Tool.IsCardReaderExc = false; //解锁
                Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        key0k.IsEnabled = false;//没有读到卡片的时候，按钮处置为灰色
                    }));
                });
                return -9998;
            }
        }
        /*
        private int ReadCard()
        {
            try
            {
                msg.TerminalNo = short.Parse(Tool.GetConfigKeyValue("TERMINAL_NO"));
                int retCode = ReaderAPI.TA_ReadCardSimple(ref msg);
                log.Info("读卡返回:" + retCode);
                if (retCode == 0) 
                {
                    Task.Factory.StartNew(() =>
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            TotalToPay.Text = "0.00";
                            UcardBalance.Text = "读取中..";
                            UBalance.Text = "读取中..";
                            UstudentCode.Text = "读取中..";
                            Uname.Text = "读取中..";
                            Usate.Text = "读取中..";

                        }));
                    });

                    if (!Tool.IsCardReaderExc)
                    {
                        ReaderAPI.TA_CRBeep(300);
                    }
                    Tool.IsCardReaderExc = true;//成功读取到卡片信息
                }
                //读到卡的情况下
                if (Tool.IsCardReaderExc &&(retCode==0 || retCode==-1222 || retCode==-1223))
                {
                    //如果网络验证没有通过
                    if (!Tool.NetWorkValidBeforeCharge)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                            {
                                key0k.IsEnabled = false;
                                key0k.Content = "网络联通检测..";
                            }));
                        });
                        //双网络畅通性检测
                        int checkTwoServer = TwoServerOnlineCheck();
                        if (checkTwoServer != 0)
                        {
                            return checkTwoServer;
                        }
                        else
                        {
                            Tool.NetWorkValidBeforeCharge = true;
                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    key0k.IsEnabled = true;
                                    key0k.Content = "充值";
                                }));
                            });
                        }
                    }
                    return 0;    
                }
                isCharging = false;
                if (0 == retCode || retCode == -1222 || retCode == -1223)
                {   
                    Tool.StudentCode = msg.StudentCode;
                    Tool.StudentName = msg.Name;
                    string balance = (Math.Abs(msg.Balance % 100)).ToString();
                    if (balance.Length == 1)
                    {
                        balance = "0" + balance;
                    }
                    Tool.CardBalance = msg.Balance / 100 + "." + balance;
                    log.Info(string.Format("当前用户{0},用户名{1},账户余额{2}(元)", msg.StudentCode, msg.Name,Tool.CardBalance));
                    if (!string.IsNullOrEmpty(Tool.StudentCode))
                    {
                        string logs = string.Format("query user:" + msg.StudentCode);
                        log.Info(logs);
                        QueryUserParams quparam = new QueryUserParams();
                        quparam.userId = Tool.StudentCode;
                        quparam.reserved0 = "disableUserIdLikeCondition";
                        QueryUserResult quResult = fd.queryUser(quparam);
                        if (quResult.data != null)
                        {
                            Tool.AccountState = quResult.data.getAccountState(quResult.data.accountState);//状态
                            Tool.NetBalance = quResult.data.accountFee.ToString();//账户余额
                            if (!string.IsNullOrEmpty(quResult.data.atName))
                            {
                                Tool.AtName = quResult.data.atName;
                            }
                            else 
                            {
                                Tool.AtName = "无模版信息";
                            }

                            if (!string.IsNullOrEmpty(quResult.data.packageName))
                            {
                                Tool.PackageName = quResult.data.packageName;
                            }
                            else
                            {
                                Tool.PackageName = "无套餐信息";
                            }

                            string[] templateArr = templateList.Split('|');
                            string[] packageArr = packageList.Split('|');
                            Boolean isTempLimited = false, isPackLimited = false;
                            for (int i = 0; i < templateArr.Length; i++)
                            {
                                if (quResult.data.atName == templateArr[i])
                                {
                                    isTempLimited = true;
                                    break;
                                }
                            }

                            for (int j = 0; j < packageArr.Length; j++)
                            {
                                if (quResult.data.packageName == packageArr[j])
                                {
                                    isPackLimited = true;
                                    break;
                                }
                            }

                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });

                            if (!isTempLimited && !isPackLimited&&checkTempPack)
                            {
                                return -9009;
                            }
                            log.Info("user state:" + quResult.data.accountState.ToString());
                            if (MODE.ToUpper() == "SINGLE" && (quResult.data.accountState == 3 || quResult.data.accountState == 2 || quResult.data.accountState == 100))
                            {
                                return -9111;//单例而且欠费
                            }
                            else if (MODE.ToUpper() == "SINGLE" && quResult.data.accountState != 0)
                            {
                                return -9112;
                            }
                        }
                        //无用户信息
                        else
                        {
                            if (quResult.errorCode == 0)
                            {

                                Tool.IsCardReaderExc = false;
                                return -9001;
                            }
                            else
                            {
                                Tool.IsCardReaderExc = false;
                                return -9002;
                             }
                        }
                    }
                    else
                    {
                        Tool.IsCardReaderExc = false;
                        return -9003;
                    }
                }
                else 
                {
                    msg = new ReaderAPI.AccountMsg();//没读到卡，直接清空数据
                    Tool.IsCardReaderExc = false; //解锁
                    Tool.NetWorkValidBeforeCharge = false;
                    Task.Factory.StartNew(() =>
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                        {
                            key0k.IsEnabled = false;//没有读到卡片的时候，按钮处置为灰色
                        }));
                    });
                }
                return retCode;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                Tool.NetWorkValidBeforeCharge = false;
                Tool.IsCardReaderExc = false; //解锁
                Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                    {
                        key0k.IsEnabled = false;//没有读到卡片的时候，按钮处置为灰色
                    }));
                });
                //9999网络发生异常
                return -9998;
            }
        }
        */
        private void SetButtonColor(int index)
        {
            
        }

        object sbuLockObj = new object();
        private void TimerSubWork(int retCode)
        {
            lock (sbuLockObj)
            {
                try
                {
                    if (MODE == "SINGLE")
                    {
                        if(retCode==-9111)
                        {
                            ChargeOneMonth();//设置充值一个月的信息
                            if (MODE == "SINGLE")
                            {
                                ChargeToYKT(true);
                            }
                            else 
                            {
                                ChargeToYKT(false);
                            }
                            return;
                        }
                        else if (retCode == -9112)
                        {
                            StopTimer();
                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });
                            string errMsg = GetErrMsgByCode(retCode);
                            PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                            popupElapseWindow.Message = errMsg;
                            popupElapseWindow.IsError = true;
                            popupElapseWindow.Interval = 3000;
                            popupElapseWindow.ShowDialog();
                            StartTimer();
                        }
                        else if (0 == retCode || retCode == -1222 || retCode == -1223)
                        {

                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });
                        }
                        //单例模式下，只有-9111为可以进行充值的用户
                        else if (retCode != -1203)
                        {
                            StopTimer();
                            Tool.IsCardReaderExc = false;

                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    TotalToPay.Text = "0.00";
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });

                            Tool.CardBalance = "0.00";
                            Tool.AccountState = "请读卡";
                            Tool.NetBalance = "0.00";
                            Tool.StudentCode = "请读卡";
                            Tool.StudentName = "请读卡";


                            key0k.Content = "充值";
                            if (Tool.IsCardReaderExc)
                            {
                                key0k.IsEnabled = true;
                            }
                            keyCancel.IsEnabled = true;
                            string errMsg = GetErrMsgByCode(retCode);
                            if (retCode == -9009)
                            {
                                errMsg += string.Format("\n当前用户模版：{0}\n当前用户套餐：{1}", Tool.AtName, Tool.PackageName);
                            }
                            PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                            popupElapseWindow.Message = errMsg;
                            popupElapseWindow.IsError = true;
                            popupElapseWindow.Interval = 3000;
                            popupElapseWindow.ShowDialog();
                            StartTimer();
                            return;
                        }
                        else
                        {
                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    TotalToPay.Text = "0.00";
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });

                            //no card
                            Tool.IsCardReaderExc = false;
                            Tool.CardBalance = "0.00";
                            Tool.AccountState = "请读卡";
                            Tool.NetBalance = "0.00";
                            Tool.StudentCode = "请读卡";
                            Tool.StudentName = "请读卡";

                            key0k.Content = "充值";
                            if (Tool.IsCardReaderExc)
                            {
                                key0k.IsEnabled = true;
                            }
                            keyCancel.IsEnabled = true;
                        }
                         
                        
                    }
                    //非single方式
                    else
                    {
                        if (0 == retCode || retCode == -1222 || retCode == -1223)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });
                        }
                        else if (-1203 != retCode)
                        {
                            StopTimer();
                            Tool.IsCardReaderExc = false;
                           
                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    TotalToPay.Text = "0.00";
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });

                            Tool.CardBalance = "0.00";
                            Tool.AccountState = "请读卡";
                            Tool.NetBalance = "0.00";
                            Tool.StudentCode = "请读卡";
                            Tool.StudentName = "请读卡";


                            key0k.Content = "充值";
                            if (Tool.IsCardReaderExc)
                            {
                                key0k.IsEnabled = true;
                            }
                            keyCancel.IsEnabled = true;
                            string errMsg = GetErrMsgByCode(retCode);
                            if (retCode == -9009)
                            {
                                errMsg += string.Format("\n当前用户模版：{0}\n当前用户套餐：{1}", Tool.AtName, Tool.PackageName);
                            }
                            PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                            popupElapseWindow.Message = errMsg;
                            popupElapseWindow.IsError = true;
                            popupElapseWindow.Interval = 3000;
                            popupElapseWindow.ShowDialog();
                            StartTimer();
                        }
                        //非single,无卡状态
                        else
                        {

                            Task.Factory.StartNew(() =>
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                {
                                    TotalToPay.Text = "0.00";
                                    UcardBalance.Text = Tool.CardBalance;
                                    UBalance.Text = Tool.NetBalance;
                                    UstudentCode.Text = Tool.StudentCode;
                                    Uname.Text = Tool.StudentName;
                                    Usate.Text = Tool.AccountState;

                                }));
                            });

                            //no card
                            Tool.IsCardReaderExc = false;
                            Tool.CardBalance = "0.00";
                            Tool.AccountState = "请读卡";
                            Tool.NetBalance = "0.00";
                            Tool.StudentCode = "请读卡";
                            Tool.StudentName = "请读卡";

                           
                            key0k.Content = "充值";
                            if (Tool.IsCardReaderExc)
                            {
                                key0k.IsEnabled = true;
                            }
                            keyCancel.IsEnabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Info(ex.ToString());
                    return;
                }
            }
        }

        /// <summary>
        /// 开始读卡
        /// </summary>
        public void StopTimer()
        {
            dtReadCard.Stop();
        }
        /// <summary>
        /// 停止读卡
        /// </summary>
        /// <param name="startReader"></param>
        public void StartTimer()
        {
            dTimer.Interval = new TimeSpan(0, 0, CardReaderIntervel);
            //dtReadCard.Interval =  TimeSpan.FromSeconds(CardReaderIntervel);
            dtReadCard.Start();
        }
        public string GetErrMsgByCode(int retCode)
        {
            switch (Math.Abs(retCode))
            {
                case 0:
                    return "成功";
                case 1:
                    return "Err:1,版本不符";
                case 2:
                    return "Err:2,返回码错误";
                case 3:
                    return "Err:3,数据长度错误";
                case 4:
                    return "Err:4,文件名非法";
                case 5:
                    return "Err:5,文件非法访问";
                case 6:
                    return "Err:6,操作失败";
                case 100:
                    return "Err:100,记录不存在";
                case 101:
                    return "Err:101,下载文件失败";
                case 200:
                    return "Err:200,读卡器网络异常";
                case 201:
                    return "Err:201,文件发送出错";
                case 202:
                    return "Err:202,数据接收出错";
                case 203:
                    return "Err:203,文件接收出错";
                case 204:
                    return "Err:204,发送文件出错";
                case 300:
                    return "Err:300,子系统代码无效";
                case 301:
                    return "Err:301,站点号无效";

                case 500:
                    return "Err:500,加密卡头读取错误";

                case 501:
                    return "Err:501,读配置读取区错误";
                case 502:
                    return "Err:502,密钥读取错误";
                case 503:
                    return "Err:503,加密卡打开失败";
                case 1001:
                    return "Err:1001,SIOS没有运行";
                case 1002:
                    return "Err:1002,DSQL操作错误";
                case 1003:
                    return "Err:1003,缓冲区小，不能拷贝";
                case 1004:
                    return "Err:1004,解包出错";
                case 1005:
                    return "Err:1005,重做业务";
                case 1006:
                    return "Err:1006,未找到相片文件";
                case 1007:
                    return "Err:1007,指定文件不存在";
                case 1100:
                    return "Err:1100,指定文件已经存在";
                case 1101:
                    return "Err:1101,操作被拒绝";
                case 1102:
                    return "Err:1102,没有文件";
                case 1103:
                    return "Err:1103,删除文件失败";
                case 1104:
                    return "Err:1104,通讯失败";
                case 1200:
                    return "Err:1200,交易额错误";
                case 1201:
                    return "Err:1201,第三方未初始化";
                case 1202:
                    return "Err:1202,读卡器错误";
                case 1203:
                    return "Err:1203,读卡失败";
                case 1204:
                    return "Err:1204,写卡失败";
                case 1205:
                    return "Err:1205,功能调用限制";
                case 1206:
                    return "Err:1206,不是消费卡";
                case 1207:
                    return "Err:1207,非本院校卡";
                case 1208:
                    return "Err:1208,过期卡";
                case 1209:
                    return "Err:1209,修改卡次数失败";
                case 1210:
                    return "Err:1210,写卡时卡号不符";
                case 1211:
                    return "Err:1211,卡消费密码错误";
                case 1212:
                    return "Err:1212,卡内余额不足";
                case 1213:
                    return "Err:1213,超过消费限额";
                case 1214:
                    return "Err:1214,挂失卡";
                case 1215:
                    return "Err:1215,冻结卡";
                case 1216:
                    return "Err:1216,卡号帐号不符";
                case 1217:
                    return "Err:1217,身份关闭";
                case 1218:
                    return "Err:1218,加载链接库失败";
                case 1219:
                    return "Err:1219,读卡器初始化失败";
                case 1220:
                    return "Err:1220,参数错误";
                case 1221:
                    return "Err:1221,没有该账户";
                case 1222:
                    return "Err:1222,补助成功";
                case 1223:
                    return "Err:1223,补助失败";
                case 1224:
                    return "Err:1224,当前打开多个登录页面";
                case 1225:
                    return "Err:1225加载dll失败";
                case 9001:
                    return "未查询到该帐号信息，请确认是否已开户";
                case 9002:
                    return "计费系统查询出错，请确认帐号状态正常";
                case 9003:
                    return "该卡片没有学工号！";
                case 9009:
                    return "不支持该模版套餐下的用户充值。";
                case 9111:
                    return "当前仅支持欠费用户充值，默认充值一个月网费";
                case 9112:
                    return "当前仅支持欠费用户充值，默认充值一个月网费";
                case 9113:
                    return "扣费成功，缴费失败，请联系管理员";
                case 9114:
                    return "一卡通服务器连接异常，请联系管理员";
                case 9115:
                    return "SAM+缴费服务器访问异常，请联系管理员";
                case 9116:
                    return "一卡通服务器、SAM服务器访问异常，请联系管理员";
                case 9998:
                    return "SAM+访问异常，扣款成功，缴费失败，请联系管理员";
                default:
                    return "系统异常";
            }
        }

        //充值
        private void cz_Click(object sender, RoutedEventArgs e) 
        {
            
        }


        private void ChargeToYKT(Boolean isSingleMode) 
        {
            if (Convert.ToDouble(TotalToPay.Text.ToString()) <= 0)
            {
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "请选择要充值月数";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                return;
            }
            int moneyToPayCaled = (int)(Convert.ToDecimal(string.Format("{0:F}", TotalToPay.Text.Trim())) * 100);
            log.Info(string.Format("当前充值用户{0},支付金额{1}分,一卡通余额{1}分", Tool.StudentCode, moneyToPayCaled, msg.Balance));
            if (moneyToPayCaled > msg.Balance) 
            {
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "当前余额不足，无法完成支付，请确认";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                return;
            }
            isCharging = true;
            if (!isSingleMode)
            {
                key0k.Content = "充值中...";
                key0k.IsEnabled = false;
                keyCancel.IsEnabled = false;
            }
            else 
            {
                chargeText.Text = string.Format("扣取1个月费用:{0}元,自动充值中..", payMonth);
            }
            var task = new Task<int>(() =>
            {
                //把耗时的操作放到这里 处理好返回结果
                //Thread.Sleep(500);
                //return 1;
                int doubleCheckNetWork = TwoServerOnlineCheck();
                return doubleCheckNetWork;
            });
            task.ContinueWith(t =>
            {
                //进行一些逻辑处理，也可以放到上面的Task中

                //在把结果显示到界面 可以传参什么的
                this.Dispatcher.BeginInvoke(new Action<int>((result) =>
                {
                    if (result == 0)
                    {
                        try
                        {
                            ReaderAPI.CardConsume cc = new ReaderAPI.CardConsume();
                            cc.AccountNo = 70004;
                            ReaderAPI.TA_FastGetCardNo(ref cc.CardNo);
                            cc.Operator = "01";
                            cc.TerminalNo = ushort.Parse(Tool.GetConfigKeyValue("TERMINAL_NO")); ;
                            int money = (int)(Convert.ToDecimal(string.Format("{0:F}", TotalToPay.Text.Trim())) * 100);
                            int i = getConsume(cc, money);
                            if (i == 0)
                            {
                                decimal moneyToSam = Convert.ToDecimal(TotalToPay.Text);
                                string fundResult = fd.getFund(UstudentCode.Text.Trim(), moneyToSam);
                                log.Info(string.Format("Charge for user:{0},charge money:{1},SAM returns after recharging:{2}", UstudentCode.Text, moneyToSam, fundResult));
                                if (string.IsNullOrEmpty(fundResult))
                                {
                                    string moneydb = string.Empty;
                                    double iss = 0.0;
                                    try
                                    {
                                        Windowinfo.tubalance = string.Format("{0:F}", (Convert.ToDouble(UBalance.Text.Trim()) + Convert.ToDouble(TotalToPay.Text)));
                                        string s1 = (money % 100).ToString();
                                        if (s1.Length == 1)
                                        {
                                            s1 = "0" + s1;
                                        }
                                        moneydb = money / 100 + "." + s1;
                                        iss = Convert.ToDouble(UcardBalance.Text);
                                        Windowinfo.tucardbalance = (iss - Convert.ToDouble(moneydb)).ToString();
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(ex.ToString());
                                        Windowinfo.tucardbalance = "读取中，请返回首页查看";
                                        Windowinfo.tubalance = "读取中，请返回首页查看";
                                    }
                                    StopTimer();//显示成功界面后，停止读卡
                                    log.Info("---begin to show info window---");
                                    msg = new ReaderAPI.AccountMsg();
                                    Windowinfo wf = new Windowinfo();
                                    wf.Topmost = true;
                                    wf.ShowDialog();
                                    //成功界面关闭以后，恢复读卡
                                    StartTimer();
                                    //充值成功后，将用户数据清空
                                    isCharging = false;
                                    log.Info("---info window is closed---");
                                    //充值成功以后更新余额的显示,当Single模式下不触发重新刷卡，防止再次扣款
                                    if (MODE != "SINGLE")
                                    {
                                        Tool.IsCardReaderExc = false;
                                    }
                                    //当充值完毕以后，清空该值，强制一卡通进行再次读卡
                                    Task.Factory.StartNew(() =>
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                                        {
                                            try
                                            {
                                                Tool.CardBalance = (iss - Convert.ToDouble(moneydb)).ToString();
                                                Tool.NetBalance = string.Format("{0:F}", (Convert.ToDouble(UBalance.Text.Trim()) + Convert.ToDouble(TotalToPay.Text)));
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Error(ex.ToString());
                                                Tool.CardBalance = "读取失败，请刷卡重试";
                                                Tool.NetBalance = "读取失败，请刷卡重试";
                                            }
                                            if (!isSingleMode)
                                            {
                                                if (Tool.IsCardReaderExc)
                                                {
                                                    key0k.IsEnabled = true;
                                                }
                                                keyCancel.IsEnabled = true;
                                                key0k.Content = "充值";
                                            }
                                            else
                                            {
                                                chargeText.Text = "仅支持欠费用户充值，自动缴1个月网费";
                                            }
                                            TotalToPay.Text = "0.00";
                                            UcardBalance.Text = Tool.CardBalance;
                                            UBalance.Text = Tool.NetBalance;
                                            UstudentCode.Text = Tool.StudentCode;
                                            Uname.Text = Tool.StudentName;
                                            Usate.Text = Tool.AccountState;
                                        }));
                                    });

                                    return;
                                }
                                else
                                {
                                    key0k.Content = "充值";
                                    if (Tool.IsCardReaderExc)
                                    {
                                        key0k.IsEnabled = true;
                                    }
                                    keyCancel.IsEnabled = true;
                                    isCharging = false;
                                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                                    popupElapseWindow.Message = "充值失败,原因:" + fundResult;
                                    popupElapseWindow.IsError = true;
                                    popupElapseWindow.Interval = 3000;
                                    popupElapseWindow.ShowDialog();
                                    return;
                                }
                            }
                            else
                            {
                                key0k.Content = "充值";
                                if (Tool.IsCardReaderExc)
                                {
                                    key0k.IsEnabled = true;
                                }
                                keyCancel.IsEnabled = true;
                                isCharging = false;
                                
                                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                                popupElapseWindow.Message = "充值失败,请确认卡片保持在读卡器上";
                                popupElapseWindow.IsError = true;
                                popupElapseWindow.Interval = 3000;
                                popupElapseWindow.ShowDialog();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                            popupElapseWindow.Message = "充值失败,错误:"+ex.ToString();
                            popupElapseWindow.IsError = true;
                            popupElapseWindow.Interval = 3000;
                            popupElapseWindow.ShowDialog();
                            key0k.Content = "充值";
                            isCharging = false;
                            keyCancel.IsEnabled = true;
                            log.Error(ex.ToString());
                            return;
                        }
                    }
                    //充值按钮点击，再次确认此时网络状态
                    else 
                    {
                        Tool.NetWorkValidBeforeCharge = false;
                        Tool.IsCardReaderExc = false;//增加此逻辑，用于当用户在点击充值时，网络断开，发起重新刷卡和网络验证
                        string networkErr = GetErrMsgByCode(result);
                        PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                        popupElapseWindow.Message = networkErr;
                        popupElapseWindow.IsError = true;
                        popupElapseWindow.Interval = 3000;
                        popupElapseWindow.ShowDialog();
                        key0k.Content = "充值";
                        keyCancel.IsEnabled = true;
                        return;
                    }
                }), t.Result);
            });
            task.Start();
        }

        private void keyOk_Click(object sender, RoutedEventArgs e)
        {
            log.Info(string.Format("充值模块:开始充值数据交换,用户{0},充值金额{1},账户余额{2}", Tool.StudentCode, TotalToPay.Text,Tool.CardBalance));
            if (MODE == "SINGLE")
            {
                ChargeToYKT(true);
            }
            else
            {
                ChargeToYKT(false);
            }
        }
        int getConsume(ReaderAPI.CardConsume cc,int money)
        {
            int Jnl = radom.Next(Int32.MinValue, Int32.MaxValue);
            cc.TranJnl = Jnl;
            cc.TranAmt = -money;
            int nRet = ReaderAPI.TA_Consume(ref cc, false);
            if (nRet == 0 && cc.RetCode == -2)
            {
                nRet=getConsume(cc, money);
            }
            return nRet;
        }

        private void ChargeOneMonth() 
        {
            if (!isCharging)
            {
                moneyToPay = payMonth;
                if (payMonth > msg.Balance / 100)
                {
                    StopTimer();
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 1, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    StartTimer();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
        }

        private void key1_Click(object sender, RoutedEventArgs e)
        {
            if (!Tool.IsCardReaderExc)
            {
                StopTimer();
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "请将卡片置于读卡器上，完成卡片信息读取";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                StartTimer();
                return;
            }

            if (!isCharging)
            {
                moneyToPay = payMonth;
                if (payMonth > msg.Balance / 100)
                {
                    StopTimer();
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 1, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    StartTimer();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
            log.Info(string.Format("用户{0},选择充值{1}个月，充值金额{2}元,isCharging:{3}", Tool.StudentCode, 1, TotalToPay.Text, isCharging));
        }


        private void key2_Click(object sender, RoutedEventArgs e)
        {
            if (!Tool.IsCardReaderExc)
            {
                StopTimer();
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "请将卡片置于读卡器上，完成卡片信息读取";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                StartTimer();
                return;
            }
            if (!isCharging)
            {
                moneyToPay = payMonth * 2;
                if (payMonth > msg.Balance / 100)
                {
                    StopTimer();
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 2, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    StartTimer();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }

            log.Info(string.Format("用户{0},选择充值{1}个月，充值金额{2}元,isCharging:{3}", Tool.StudentCode, 2, TotalToPay.Text, isCharging));
        }

        private void key3_Click(object sender, RoutedEventArgs e)
        {
            if (!Tool.IsCardReaderExc)
            {
                StopTimer();
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "请将卡片置于读卡器上，完成卡片信息读取";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                StartTimer();
                return;
            }
            if (!isCharging)
            {
                moneyToPay = payMonth * 3;
                if (payMonth > msg.Balance / 100)
                {
                    StopTimer();
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 3, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    StartTimer();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }

            log.Info(string.Format("用户{0},选择充值{1}个月，充值金额{2}元,isCharging:{3}", Tool.StudentCode, 3, TotalToPay.Text, isCharging));
        }

        private void key4_Click(object sender, RoutedEventArgs e)
        {
            if (!Tool.IsCardReaderExc)
            {
                StopTimer();
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "请将卡片置于读卡器上，完成卡片信息读取";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                StartTimer();
                return;
            }
            if (!isCharging)
            {
                moneyToPay = payMonth * 4;
                if (payMonth > msg.Balance / 100)
                {
                    StopTimer();
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 4, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    StartTimer();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }

            log.Info(string.Format("用户{0},选择充值{1}个月，充值金额{2}元,isCharging:{3}", Tool.StudentCode, 4, TotalToPay.Text, isCharging));
        }

        private void key5_Click(object sender, RoutedEventArgs e)
        {
            if (!Tool.IsCardReaderExc)
            {
                StopTimer();
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "请将卡片置于读卡器上，完成卡片信息读取";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                StartTimer();
                return;
            }
            if (!isCharging)
            {
                moneyToPay = payMonth * 5;
                if (payMonth > msg.Balance / 100)
                {
                    StopTimer();
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 5, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    StartTimer();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }

            log.Info(string.Format("用户{0},选择充值{1}个月，充值金额{2}元,isCharging:{3}", Tool.StudentCode, 5, TotalToPay.Text, isCharging));
        }

        private void key6_Click(object sender, RoutedEventArgs e)
        {
            if (!Tool.IsCardReaderExc)
            {
                StopTimer();
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "请将卡片置于读卡器上，完成卡片信息读取";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 3000;
                popupElapseWindow.ShowDialog();
                StartTimer();
                return;
            }
            if (!isCharging)
            {
                moneyToPay = payMonth * 6;
                if (payMonth > msg.Balance / 100)
                {
                    StopTimer();
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 6, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    StartTimer();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
            log.Info(string.Format("用户{0},选择充值{1}个月，充值金额{2}元,isCharging:{3}", Tool.StudentCode, 6, TotalToPay.Text, isCharging));
        }

        private void key7_Click(object sender, RoutedEventArgs e)
        {
            if (!isCharging)
            {
                moneyToPay = payMonth * 7;
                if (payMonth > msg.Balance / 100)
                {
                    MessageBox.Show(string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 1, payMonth), "错误", MessageBoxButton.OK, MessageBoxImage.Error,MessageBoxResult.OK,MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
        }

        private void key8_Click(object sender, RoutedEventArgs e)
        {
            moneyToPay = payMonth * 8;
            if (payMonth > msg.Balance/100)
            {
                MessageBox.Show(string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 1, payMonth), "错误", MessageBoxButton.OK, MessageBoxImage.Error,MessageBoxResult.OK,MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            TotalToPay.Text = String.Format("{0:F}", moneyToPay);
            key0k.Content = string.Format("充值({0}元)", moneyToPay);
        }

        private void key9_Click(object sender, RoutedEventArgs e)
        {
            if (!isCharging)
            {
                moneyToPay = payMonth * 9;
                if (payMonth > msg.Balance)
                {
                    MessageBox.Show(string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 1, payMonth), "错误", MessageBoxButton.OK, MessageBoxImage.Error,MessageBoxResult.OK,MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);

                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
        }

        private void key10_Click(object sender, RoutedEventArgs e)
        {
            if (!isCharging)
            {
                moneyToPay = payMonth * 10;
                if (payMonth > msg.Balance / 100)
                {
                    MessageBox.Show(string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 1, payMonth), "错误", MessageBoxButton.OK, MessageBoxImage.Error,MessageBoxResult.OK,MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
        }

        private void key11_Click(object sender, RoutedEventArgs e)
        {
            if (!isCharging)
            {
                moneyToPay = payMonth * 11;
                if (payMonth > msg.Balance / 100)
                {
                    MessageBox.Show(string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 1, payMonth), "错误", MessageBoxButton.OK, MessageBoxImage.Error,MessageBoxResult.OK,MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
        }
        /// <summary>
        /// 退格事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void key12_Click(object sender, RoutedEventArgs e)
        {
            if (!isCharging)
            {
                moneyToPay = payMonth * 12;
                if (payMonth > msg.Balance / 100)
                {
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = string.Format("选择充值{0}个月，每月金额{1}元，余额不足。", 12, payMonth);
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 3000;
                    popupElapseWindow.ShowDialog();
                    return;
                }
                TotalToPay.Text = String.Format("{0:F}", moneyToPay);
                key0k.Content = string.Format("充值({0}元)", moneyToPay);
            }
        }
        private void Page_Loaded(object sender, RoutedEventArgs e) 
        {
            ReaderAPI.TA_CRClose();
            if (MODE.ToUpper() == "SINGLE") 
            {
                chargeText.Text = "仅支持欠费用户充值，自动缴1个月网费";
                key1.Visibility = Visibility.Collapsed;
                key2.Visibility = Visibility.Collapsed;
                key3.Visibility = Visibility.Collapsed;
                key4.Visibility = Visibility.Collapsed;
                key5.Visibility = Visibility.Collapsed;
                key6.Visibility = Visibility.Collapsed;
                keyCancel.Visibility = Visibility.Collapsed;
                key0k.Visibility = Visibility.Collapsed;
            }
            int init = InitCardThird();
            //int init = 0;//for test
            if (0 == init)
            {
                int iniCardReader = ReaderAPI.TA_CRInit(0, 1, 19200);
                //int iniCardReader = 1;
                log.Info("初始化读卡器:" + iniCardReader.ToString());
                if (iniCardReader != 0)
                {
                    key0k.IsEnabled = false;
                    PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                    popupElapseWindow.Message = "读卡器初始化失败，请排查读卡器连接是否正常";
                    popupElapseWindow.IsError = true;
                    popupElapseWindow.Interval = 300000;
                    popupElapseWindow.ShowDialog();
                    return;
                }
                //读卡
                dtReadCard = new DispatcherTimer();
                dtReadCard.Tick += new EventHandler(dtReadCard_Tick);
                dtReadCard.Interval = TimeSpan.FromSeconds(CardReaderIntervel);
                dtReadCard.Start();
            }
            else
            {
                key0k.IsEnabled = false;
                PopupElapseWindow popupElapseWindow = new PopupElapseWindow();
                popupElapseWindow.Message = "网络初始化失败，请排查到一卡通网络是否畅通";
                popupElapseWindow.IsError = true;
                popupElapseWindow.Interval = 30000;
                popupElapseWindow.ShowDialog();
                return;
            }
        }
        
        /// <summary>
        /// 初始化卡数据
        /// </summary>
        /// <returns></returns>
        public int InitCardThird()
        {
            try
            {
                string sios_ip = Tool.GetConfigKeyValue("SIOS_IP");
                short port = 8500;
                short sys_code = Int16.Parse(Tool.GetConfigKeyValue("SYS_CODE"));
                short prox_offline = 0;
                short terminal_no = Int16.Parse(Tool.GetConfigKeyValue("TERMINAL_NO"));
                UInt32 maxjnl = 1;
                string signonpwd = "123";
                int ret = ReaderAPI.TA_Init3(sios_ip, port, sys_code, terminal_no, ref prox_offline, ref maxjnl, signonpwd);
                log.Info("初始化第三方返回：" + ret.ToString());
                return ret;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return 9999;
            }
        }

        private void keyCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!isCharging)
            {
                TotalToPay.Text = "0.00";
                key0k.Content = "充值";
            }
        }
    }
}
