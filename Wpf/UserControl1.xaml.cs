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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }
        public static string trecode;
        public static string trecodeNo;
        public static string tubalance;
        public static string tucardbalance;

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            recode.Text = trecode;
            recodeNo.Text = trecodeNo;
            ubalance.Text = tubalance;
            ucardbalance.Text = tucardbalance;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Main main = new Main();
            //MainWindow m = new MainWindow();
            ////m.b = true;
            //m.MainFrame.Content = main;
        }
    }
}

