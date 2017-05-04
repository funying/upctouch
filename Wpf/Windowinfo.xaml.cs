using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Wpf
{
    /// <summary>
    /// Windowinfo.xaml 的交互逻辑
    /// </summary>
    public partial class Windowinfo : Window
    {
        private System.Timers.Timer timer = new System.Timers.Timer(3000);
        int timeOut = int.Parse(Tool.GetConfigKeyValue("TIME_OUT"));
        public Windowinfo()
        {
            InitializeComponent();
            
            
        }
        public static string tubalance ;
        public static string tucardbalance;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dTimer.Stop();
            this.Close();
            
        }
        private void despatcher(object sender, EventArgs e)
        {
            dTimer.Stop();
            this.Close();
            
        }

        DispatcherTimer dTimer = new System.Windows.Threading.DispatcherTimer();
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ubalance.Text = tubalance;
            ucardbalance.Text = tucardbalance;
            dTimer.Interval = new TimeSpan(0, 0, timeOut);//15秒后自动返回
            dTimer.Tick += new EventHandler(despatcher);
            dTimer.Start();
        }
    }
}
