using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services;

namespace CAMAPI
{
    [Serializable]
    public class AccountOperationResult
    {
        int errorCode;
        public  int ErrorCode
        {
            get { return errorCode; }
            set { errorCode = value; }
        }//错误码
        public  string errorMessage (int errorCode){
                switch (errorCode)
                {
                    case 10001: return "执行过程中发生异常，请查看后台错误日志";
                    case 20001: return "用户名是必选项，请输入用户名";
                    case 30001: return "用户不存在，请重新输入";
                    case 30020: return "业务操作ID已经存在，请输入一个新的业务操作ID";
                    case 0: return "";
                    default: return "错误";
                }
        }//错误消息
        public  AccountingInfo accountInfo{ get;set; }//用户账户信息
    }
}
