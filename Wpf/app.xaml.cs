using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Diagnostics;
using Microsoft.Win32;
namespace Wpf
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    
    public partial class App : Application
    {
        log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public App()
        {
            log4net.Config.XmlConfigurator.Configure();
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is System.Exception)
                {
                    HandleException((System.Exception)e.ExceptionObject);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public void HandleException(Exception ex)
        {
            string strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now.ToString() + "\r\n";
            string str = string.Format(strDateInfo + "Application UnhandledException:{0};\n\r堆栈信息:{1}", ex.Message, ex.StackTrace);
            log.Error(str);
        }
    }
}
