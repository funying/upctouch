using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMAPI
{
    public class UserInfo
    {
        //用户名。不能以空格开头或结尾，可以包含任意中英文，字符和数字，但<, >, %, \, @, ", '等特殊字符除外!长度不能超过32个字节
        public  string userId { get; set; }
        //用户姓名，可以包含任意中英文，字符和数字，但 <, >, \, ', " 这五个特殊字符除外。长度不能超过64个字节
        public  string userName { get; set; }
        /// <summary>
        /// 模版名称
        /// </summary>
        public string atName { get; set; }
        /// <summary>
        /// 套餐名称
        /// </summary>
        public string packageName { get; set; }

        //证件类型。只能为0—未选择(其他)1—身份证2—学生证3—军官证4—护照
        public  int certType { get; set; }
        //证件号码
        public  string certNo { get; set; }
        //账户状态。1—正常 2—透支 3—欠费 100—余额不足
        public  int accountState { get; set; }
        //账户余额

        public int errorCode { get; set; }
        public string errorMsg { get; set; }
        public  decimal accountFee { get; set; }
        public string getAccountState(int i)
        { 
            switch(i){
                case 1: return "正常";
                case 2: return "透支";
                case 3: return "欠费";
                case 100: return "余额不足";
                default: return "余额不足";
            }
        }
    }
}
