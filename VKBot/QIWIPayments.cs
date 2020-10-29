
namespace VKBot
{
    class QIWIPayments
    {
        public class data
        {
            public string personId { get; set; }
            public string date { get; set; }
            public string status { get; set; }
            public sum sum { get; set; }
            public commission commission { get; set; }
            public total total { get; set; }
            public string comment { get; set; }

        }
        public class sum
        {
            public double amount { get; set; }
            public int currency { get; set; }
        }
        public class commission
        {
            public double amount { get; set; }
            public int currency { get; set; }
        }
        public class total
        {
            public double amount { get; set; }
            public int currency { get; set; }
        }
        public class QIWIPayment
        {
            public data[] Data { get; set; }
        }
    }
}
