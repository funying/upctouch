using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services;

namespace CAMAPI
{
    public class AccountingInfo
    {
        public  string accountId{ get; set; }//账户名
        public  Int16 accountState{ get; set; }//账户状态。1—正常 2—透支 3—欠费 100—余额不足
        public  decimal accountFee { get; set; }//账户余额
    }
}
