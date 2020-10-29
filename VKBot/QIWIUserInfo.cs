using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKBot
{
    class QIWIUserInfo
    {
        public class authInfo
        {
            public string boundEmail { get; set; }
            public string ip { get; set; }
            public string lastLoginDate { get; set; }
            public class mobilePinInfo { 
                public string lastMobilePinChange { get; set; }
                public bool mobilePinUsed { get; set; }
                public string nextMobilePinChange { get; set; }
            }
            public class passInfo
            {
                public string lastPassChange { get; set; }
                public string nextPassChange { get; set; }
                public bool passwordUsed { get; set; }
            }
            public long personId { get; set; }
            public class pinInfo{
                public bool pinUsed { get; set; }
            }
            public string registrationDate { get; set; }
        }

        public class contractInfo
        {
            public bool blocked { get; set; }
            public long contractId { get; set; }
            public string creationDate { get; set; }
            public class Nickname {
                public string nickname { get; set; }
                public bool CanChange { get; set; }
                public string description { get; set; }
            }
        }

        public class userInfo
        {
            public long defaultPayCurrency { get; set; }
            public long defaultPaySource { get; set; }
            public string email { get; set; }
            public long firstTxnId { get; set; }
            public string language { get; set; }
            public string phoneHash { get; set; }
            public string promoEnabled { get; set; }
        }
        public class QIWI
        {
            public authInfo AuthInfo { get; set; }
            public contractInfo ContractInfo { get; set; }
            public userInfo UserInfo { get; set; }
        }
    }
}
