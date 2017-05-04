using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIOAPI
{
    public class AccountMsg
    {
        //姓名
        public static string Name { get; set; }
        //卡号
        public static uint CardNo { get; set; }
        //账号
        public static uint AccountNo { get; set; }
        //学号
        public static string StudentCode { get; set; }
        //身份证号码
        public static string PID { get; set; }
        //身份证类型——代码
        public static string IDCard { get; set; }
        //密码
        public static int Balance { get; set; }
        //状态
        public static string flag { get; set; }
    }
}
