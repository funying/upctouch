using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMAPI
{
    public class QueryUserResult
    {
        //错误码
        public  int errorCode { get; set; }
        //错误消息
        public  string errorMessage { get; set; }
        //符合查询条件的用户总数
        //用户信息
        public  UserInfo data { get; set; }
    }
}
