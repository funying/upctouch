using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Timers;
using System.Windows.Threading;
namespace Wpf.Controls
{
    public partial class PopupElapseWindow : Window
    {

        private Timer timer = null;
        private int totalSeconds;
        /// <summary>
        /// 单位毫秒
        /// </summary>
        public double Interval { get; set; }
        public string Message { get; set; }
        public string Title { get; set; }
        public bool IsError { get; set; }
        SolidColorBrush msgColor = new SolidColorBrush(Colors.Red);

        public PopupElapseWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(PopupElapseWindow_Loaded);
            this.Unloaded += new RoutedEventHandler(PopupElapseWindow_Unloaded);
            //默认10秒
            timer = new Timer();
            Interval = 10000;
            IsError = false;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ShowInTaskbar = false;
        }

        void PopupElapseWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= new ElapsedEventHandler(timer_Elapsed);
                timer = null;
            }
            msgColor = null;

        }

        void PopupElapseWindow_Loaded(object sender, RoutedEventArgs e)
        {
            totalSeconds = (int)Interval / 1000;
            tbCountNumber.Text = totalSeconds.ToString();
            tbMsg.Text = Message;
            if (IsError)
            {
                tbMsg.Foreground = msgColor;
                msgColor = null;
            }
            if (String.IsNullOrEmpty(Title))
            {
                tbTitle.Text = "系统提示";
            }
            else
            {
                tbTitle.Text = Title;
            }


            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int currentSecond = --totalSeconds;
            Dispatcher.Invoke((Action<int>)(delegate(int second)
            {
                tbCountNumber.Text = currentSecond.ToString();

            }), currentSecond);
            if (currentSecond <= 0)
            {
                if (timer != null)
                {
                    timer.Stop();
                    timer.Elapsed -= new ElapsedEventHandler(timer_Elapsed);
                    timer = null;
                }
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
                {
                    this.Close();
                });
            }
        }

        private void ibtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= new ElapsedEventHandler(timer_Elapsed);
                timer = null;
            }
            this.DialogResult = true;
            this.Close();

        }
    }
}
