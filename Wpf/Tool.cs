using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Configuration;
using System.Text.RegularExpressions;

namespace Wpf
{
    public class Tool
    {
        public static T FindVisualParent<T>(DependencyObject obj) where T : class
        {
            while (obj != null)
            {
                if (obj is T)
                    return obj as T;

                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }

        //public static MyOrder myOrder = null;
        //public static QuickOrder QO = null;
        //public static Login login = null;
        //public static DIYSeat diySeat = null;
        //public static LayoutSeat layoutSeat = null;
        //public static MainWindow mainWindow = null;
        public static bool IsCardReaderExc = false;
        public static bool IsLoginPageTimerInited=false;
        public static bool IsLoginPageInited = false;
        public static bool isDiyFristTimeShow = true;
        public static int KeepAliveTickCount = 0;
        /// <summary>
        /// 充值前网络是否通过网络验证
        /// </summary>
        public static bool NetWorkValidBeforeCharge = false;

        //public static void TurnToMyOrder()
        //{
        //    mainWindow.MainFrame.Content = myOrder;
        //    mainWindow.RestartTimeOutTicks();
        //}

        //public static void TurnToLayOutSeat()
        //{
        //    mainWindow.MainFrame.Content = layoutSeat;
        //    mainWindow.RestartTimeOutTicks();
        //}

        //public static void TurnToQO()
        //{
        //    mainWindow.MainFrame.Content = QO;
        //    mainWindow.RestartTimeOutTicks();
        //}

        //public static void TurnToLogin()
        //{
        //    mainWindow.MainFrame.Content = login;
        //    mainWindow.RestartTimeOutTicks();
        //}

        //public static void TurnTODIY() 
        //{
        //    mainWindow.MainFrame.Content = diySeat;
        //    mainWindow.RestartTimeOutTicks();
        //}
        //////Frame全局变量
        //public static MyOrder myOrder = new MyOrder();
        //public static QuickOrder QOTrue = new QuickOrder(true);
        //public static Login login = new Login();
        //public static DIYSeat diySeat = new DIYSeat();

        public static string GetConfigKeyValue(string keyName)
        {
            //return System.Configuration.ConfigurationSettings.AppSettings[keyName];
            string keyValue=string.Empty;
            try
            {
                keyValue = ConfigurationManager.AppSettings[keyName].ToString();
            }
            catch
            {
                keyValue = "NULL";
            }
            return keyValue;
        }
        /// <summary>
        /// 模版名称
        /// </summary>
        public static String AtName
        {
            get;
            set;
        }
        /// <summary>
        /// 套餐名称
        /// </summary>
        public static String PackageName
        {
            get;
            set;
        }

        /// <summary>
        /// 学生姓名
        /// </summary>
        public static String StudentName
        {
            get;
            set;
        }


        /// <summary>
        /// 学号
        /// </summary>
        public static String StudentCode
        {
            get;
            set;
        }
        /// <summary>
        /// 卡内余额
        /// </summary>
        public static String CardBalance
        {
            get;
            set;
        }
        /// <summary>
        /// 网卡帐号
        /// </summary>
        public static String NetBalance
        {
            get;
            set;
        }
        private static string _state = "未查询";
        public static String AccountState
        {
            get { return _state; }
            set { _state = value; } 
        }

        /// <summary>
        /// 身份证号
        /// </summary>
        public static int IDCard
        {
            get;
            set;
        }
        /// <summary>
        /// 密码
        /// </summary>
        public static String PassWord
        {
            get;
            set;
        }

        /// <summary>
        /// 卡号
        /// </summary>
        public static int CardNo
        {
            get;
            set;
        }
        /// <summary>
        /// 帐号
        /// </summary>
        public static int AccountNo
        {
            get;
            set;
        }

    }

}
