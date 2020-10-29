using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet.Exception;

namespace VKBot
{
    class UserRanks
    {
        public static void Register(string userId)
        {
            var ranks = File.ReadAllLines(@"Data\Ranks.txt");
            var newRank = ranks[0].Split(new string[] { ";;;5" }, StringSplitOptions.None);
            File.AppendAllLines(@"Data\Users.txt", new string[] { $"{userId};;;5{newRank[0]};;;50" });
        }

        public static bool isExists(string userId)
        {
            var users = File.ReadAllLines(@"Data\Users.txt");
            foreach (string user in users)
                if (user.StartsWith($"{userId};;;5"))
                    return true;

            return false;
        }

        public static string Rank(string userId)
        {
            var users = File.ReadAllLines(@"Data\Users.txt");
            string[] User = null;
            foreach (string user in users)
                if (user.StartsWith($"{userId};;;5"))
                    User = user.Split(new string[] { ";;;5" }, StringSplitOptions.None);

            if (User != null)
            {
                int Amount = Convert.ToInt32(User[2]);
                string oldRank = User[1];
                return NewRank(userId, oldRank, Amount)[0];
            }

            return null;
        }

        public static int Amount(string userId)
        {
            var users = File.ReadAllLines(@"Data\Users.txt");
            string[] User = null;
            foreach (string user in users)
                if (user.StartsWith($"{userId};;;5"))
                    User = user.Split(new string[] { ";;;5" }, StringSplitOptions.None);

            if (User != null)
                return Convert.ToInt32(User[2]);

            return -777;
        }

        public static string[] NewRank(string userId, string oldRank, int Amount)
        {
            var ranks = File.ReadAllLines(@"Data\Ranks.txt");

            for (int i = 0; i < ranks.Length; i++)
            {
                var rank = ranks[i].Split(new string[] { ";;;5" }, StringSplitOptions.None);
                if (rank[0] == oldRank)
                {
                    if (i == ranks.Length - 1)
                        return rank;
                    else
                    {
                        var newRank = ranks[i + 1].Split(new string[] { ";;;5" }, StringSplitOptions.None);
                        var Rank = (Convert.ToInt32(newRank[2]) <= Amount) ? newRank : rank;
                        var users = File.ReadAllLines(@"Data\Users.txt");
                        List<string> newUsers = new List<string>();
                        foreach (var user in users)
                            newUsers.Add(user.StartsWith($"{userId};;;5") ? $"{userId};;;5{Rank[0]};;;5{string.Join(";;;5", user.Split(new string[] { ";;;5" }, StringSplitOptions.None).Skip(2))}" : user);

                        File.WriteAllLines(@"Data\Users.txt", newUsers);

                        return Rank;
                    }
                }
            }
            return null;
        }

        public static string NextRank(string userId)
        {
            var ranks = File.ReadAllLines(@"Data\Ranks.txt");
            var userRank = Rank(userId);
            for (int i = 0; i < ranks.Length; i++)
            {
                var rank = ranks[i].Split(new string[] { ";;;5" }, StringSplitOptions.None);
                if (rank[0] == userRank)
                    return (i == ranks.Length - 1) ? rank[0] : ranks[i+1].Split(new string[] { ";;;5" }, StringSplitOptions.None)[0];
            }
            return null;
        }

        public static int Discount(string userId)
        {
            var users = File.ReadAllLines(@"Data\Users.txt");
            string[] User = null;
            foreach (string user in users)
                if (user.StartsWith($"{userId};;;5"))
                    User = user.Split(new string[] { ";;;5" }, StringSplitOptions.None);

            if (User != null)
            {
                int Amount = Convert.ToInt32(User[2]);
                string oldRank = User[1];
                return Convert.ToInt32(NewRank(userId, oldRank, Amount)[1]);
            }

            return -777;
        }

        public static string[] GetList()
        {
            var ranks = File.ReadAllLines(@"Data\Ranks.txt");
            return ranks;
        }

        public static string[] UserRankInfo(string userId)
        {
            string UserRank = Rank(userId);
            var ranks = File.ReadAllLines(@"Data\Ranks.txt");

            foreach (string rank in ranks)
                if (rank.Split(new string[] { ";;;5" }, StringSplitOptions.None)[0] == UserRank)
                    return rank.Split(new string[] { ";;;5" }, StringSplitOptions.None);

            return null;
        }
    }
}
