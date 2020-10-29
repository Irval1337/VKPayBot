using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet;

namespace VKBot
{
    class Market
    {
        public static string[] GetList(string userId, VkApi vkapi)
        {
            var ranks = File.ReadAllLines(@"Data\Ranks.txt");
            string userRank = UserRanks.Rank(userId);

            if (Properties.Settings.Default.MarketInheritance)
            {
                List<string> market = new List<string>();

                for (int i = 0; i < ranks.Length; i++)
                {
                    var Rank = ranks[i].Split(new string[] { ";;;5" }, StringSplitOptions.None);
                    var Market = Rank[3].Split(new string[] { ";0;" }, StringSplitOptions.None);
                    foreach(var m in Market)
                        market.Add(GetName(m.Split(' ')[0], vkapi) + "|" + m.Split(' ')[0] + "|" + m.Split(' ')[1]);

                    if (Rank[0] == userRank)
                        break;
                }
                return market.ToArray();
            }
            else
            {
                foreach (string rank in ranks)
                {
                    var Rank = rank.Split(new string[] { ";;;5" }, StringSplitOptions.None);
                    if (Rank[0] == userRank)
                    {
                        var market = Rank[3].Split(new string[] { ";0;" }, StringSplitOptions.None);
                        List<string> list = new List<string>();
                        foreach (var m in market)
                            list.Add(GetName(m.Split(' ')[0], vkapi) + "|" + m.Split(' ')[0] + "|" + m.Split(' ')[1]);

                        return list.ToArray();
                    }
                }
            }
            return null;
        }

        public static string GetName(string uri, VkApi vkapi)
        {
            vkapi.Authorize(new VkNet.Model.ApiAuthParams() { AccessToken = Properties.Settings.Default.UserTokenVk });
            uri = uri.Substring(uri.IndexOf("product") + "product".Length);

            string id = "";
            if (uri.Contains("%2F"))
                id = uri.Substring(0, uri.IndexOf("%2F"));
            else if (uri.Contains("?"))
                id = uri.Substring(0, uri.IndexOf("?"));
            else
                id = uri;

            var market = vkapi.Markets.GetById(new List<string> { id }, true)[0];
            return market.Title;
        }

        public static VkNet.Model.Market MerketInfo(string userId, string title, VkApi vkapi)
        {
            vkapi.Authorize(new VkNet.Model.ApiAuthParams() { AccessToken = Properties.Settings.Default.UserTokenVk });

            string[] market = GetList(userId, vkapi);
            string uri = "";

            foreach (var m in market)
                if (m.Split('|')[0].ToLower() == title)
                    uri = m.Split('|')[1];

            
            uri = uri.Substring(uri.IndexOf("product") + "product".Length);
            

            string id = "";
            if (uri.Contains("%2F"))
                id = uri.Substring(0, uri.IndexOf("%2F"));
            else if (uri.Contains("?"))
                id = uri.Substring(0, uri.IndexOf("?"));
            else
                id = uri;

            return vkapi.Markets.GetById(new List<string> { id }, true)[0];
        }

        public static int CheckAgreement(string userId, VkNet.Model.Market product, long? price, string Product_uri)
        {
            var payments = File.ReadAllLines(@"Data\Payments.txt");
            int random = new Random().Next(1, 999999);

            Checker:
            foreach(string payment in payments)
            {
                var Info = payment.Split('|');
                if (Info[0] == userId)
                    return -777;
                if (Info[5] == random.ToString())
                {
                    random = new Random().Next(1, 999999);
                    goto Checker;
                }
                else
                    break;
            }

            File.AppendAllLines(@"Data\Payments.txt", new string[] { $"{userId}|{price}|{DateTime.Now}|{product.Title}|{Product_uri}|{random}|false" });

            return random;
        }

        public static void SetAgreement(string userId)
        {
            var payments = File.ReadAllLines(@"Data\Payments.txt");
            List<string> newPayments = new List<string>();
            foreach (string payment in payments)
            {
                if (payment.StartsWith(userId))
                {
                    var info = payment.Split('|');
                    newPayments.Add(string.Join("|", info.Take(6)) + "|true");
                }
                else
                    newPayments.Add(payment);
            }

            File.WriteAllLines(@"Data\Payments.txt", newPayments);
        }

        public static void DelAgreement(string userId)
        {
            var payments = File.ReadAllLines(@"Data\Payments.txt");
            List<string> newPayments = new List<string>();
            foreach (string payment in payments)
                if (!payment.StartsWith(userId))
                    newPayments.Add(payment);

            File.WriteAllLines(@"Data\Payments.txt", newPayments);
        }

        public static bool HasPayment(string userId)
        {
            var payments = File.ReadAllLines(@"Data\Payments.txt");
            foreach (string payment in payments)
                if (payment.StartsWith(userId))
                    return payment.Split('|')[6] == "true";

            return false;
        }

        public static string[] PaymentInfo(string userId)
        {
            var payments = File.ReadAllLines(@"Data\Payments.txt");
            List<string> info = new List<string>();
            foreach (string payment in payments)
            {
                if (payment.StartsWith(userId))
                {
                    info = payment.Split('|').ToList();
                    break;
                }
            }

            return info.ToArray();
        }

        public static void Success(string userId, string amount)
        {
            var users = File.ReadAllLines(@"Data\Users.txt");
            List<string> Users = new List<string>();
            foreach (string user in users) {
                if (user.StartsWith($"{userId};;;5"))
                {
                    var us = user.Split(new string[] { ";;;5" }, StringSplitOptions.None);
                    int Amount = Convert.ToInt32(us[2]);
                    Users.Add(string.Join(";;;5", us.Take(2)) + $";;;5{Amount + Convert.ToInt32(amount)}");
                }
                else
                    Users.Add(user);
            }
            File.WriteAllLines(@"Data\Users.txt", Users);
        }
    }
}
