using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAMAPI
{
    public class QueryUserParams
    {
        //用户名 Sam账号
        public  string userId { get; set; }
        //用户姓名
        public  string userName { get; set; }
        //用户账户状态，账户状态只能为：1—正常用户 2—透支用户 3—欠费用户 4—暂停用户 5—黑名单用户 100—余额不足
        public  int accountState { get; set; }
        //证件类型。只能为0—未选择(其他)1—身份证2—学生证3—军官证4—护照
        public  int certificateType { get; set; }
        //证件号码。。一般是身份证
        public string certificateNo { get; set; }
        public string reserved0 { get; set; }
    }
}
