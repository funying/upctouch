using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using System.Net.Security;
using System.Web.Services;

namespace CAMAPI
{
    public class FundAPI
    {
        WebReference.SamService svc = new WebReference.SamService();
        public FundAPI() {
            //string sam_username1 = "SAMAPI";
            //string sam_password1 = "Ruijie123";
            svc.Url = ConfigurationManager.AppSettings["svcUrl"].ToString();
            string sam_username1 = ConfigurationManager.AppSettings["sam_username"].ToString();
            string sam_password1 = ConfigurationManager.AppSettings["sam_password"].ToString();
            System.Net.NetworkCredential netCredential = new System.Net.NetworkCredential(sam_username1, sam_password1);
            Uri uri = new Uri(svc.Url);
            System.Net.ICredentials credentials = netCredential.GetCredential(uri, "Basic");
            svc.Credentials = credentials;
            svc.PreAuthenticate = false;
            //if (true)//是否调用https协议
            //{
            //    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
            //    {
            //        return true;
            //    };
            //}

        }
        public void getUserparams(string userId) { 

        }

        public string getFund(string userId,decimal fee)
        {
            AccountOperationResult res = fund(userId, 11, fee);
            string str=res.errorMessage(res.ErrorCode);
            if (str=="")
            {
                return "";
            }
            return str;
        }
       private AccountOperationResult fund(string userId,int fundType,decimal fee)
       {
           AccountOperationResult res = new AccountOperationResult();
           WebReference.accountOperationResult qu= svc.fund(userId, fundType, fee, true, svc.createTxid());
           res.ErrorCode = qu.errorCode;
           if (res.errorMessage(res.ErrorCode) == "")
           {
               WebReference.accountingInfo ac = qu.accountInfo;
               AccountingInfo acinfo = new AccountingInfo();
               acinfo.accountId = ac.accountId;
               acinfo.accountState = ac.accountState;
               acinfo.accountFee = ac.accountFee;
               res.accountInfo = acinfo;
           }
           return res;
       }
        public UserInfo getUserInfo(QueryUserParams quparam)
        {
            QueryUserResult res = queryUser(quparam);
            if (res.errorCode == 0)
            {
                return res.data;
            }
            return null;
        }
       public QueryUserResult queryUser(QueryUserParams quparam)
       {
           WebReference.queryUserParams userparams=new WebReference.queryUserParams();
           userparams.userId = quparam.userId;
           userparams.reserved0 = quparam.reserved0;
           WebReference.queryUserResult qures;
           try
           {
                qures = svc.queryUser(userparams);
           }
           catch(Exception ex){
               throw new Exception(ex.Message);
           }

           QueryUserResult res = new QueryUserResult();

           if (qures != null && qures.errorCode == 0 && qures.data != null)
           {

               res.errorCode = qures.errorCode;
               res.errorMessage = qures.errorMessage;
               WebReference.userInfo[] user = qures.data;
               UserInfo info = new UserInfo();
               info.userId = user[0].userId;
               info.userName = user[0].userName;
               info.accountFee = user[0].accountFee;
               info.accountState = user[0].accountState;
               info.certNo = user[0].certNo;
               info.certType = user[0].certType;
               info.atName = user[0].atName;
               info.packageName = user[0].packageName;
               res.data = info;
           }
           else
           {
               res.errorCode = qures.errorCode;
               res.errorMessage = qures.errorMessage;
               res.data = null;
           }
           return res;
       }


    }
}
