using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;
using static VKBot.QIWIUserInfo;

namespace VKBot
{
    public partial class Main : Form
    {
        public static VkApi vkapi = new VkApi();
        public static VkApi UserVkapi = new VkApi();
        public static Thread BotVkThread;
        static QIWI Qiwi;
        public Main()
        {
            InitializeComponent();
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BotVkThread == null || !BotVkThread.IsAlive)
                new Settings().ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (BotVkThread == null || !BotVkThread.IsAlive)
            {
                if (!string.IsNullOrEmpty(Properties.Settings.Default.TokenVk) && !string.IsNullOrEmpty(Properties.Settings.Default.TokenQIWI) && !string.IsNullOrEmpty(Properties.Settings.Default.IdVk) && !string.IsNullOrEmpty(Properties.Settings.Default.UserTokenVk))
                {
                    vkapi.Authorize(new ApiAuthParams() { AccessToken = Properties.Settings.Default.TokenVk });

                    BotVkThread = new Thread(BotVkAPI);
                    BotVkThread.Start(this);

                    button1.Text = "Остановить";
                }
                else
                    MessageBox.Show("Ошибка заполнения полей токенов/ID");
            }
            else
            {
                BotVkThread.Abort();
                button1.Text = "Запустить";
            }
        }

        private static void BotVkAPI(object obj)
        {
            Main main = (Main)obj;
        Start:
            try
            {
                while (true)
                {
                    var s = vkapi.Groups.GetLongPollServer(Convert.ToUInt64(Properties.Settings.Default.IdVk));
                    var poll = vkapi.Groups.GetBotsLongPollHistory(
                       new BotsLongPollHistoryParams()
                       { Server = s.Server, Ts = s.Ts, Key = s.Key, Wait = 25 });
                    if (poll?.Updates == null) continue;

                    foreach (var a in poll.Updates)
                    {
                        if (a.Type == GroupUpdateType.MessageNew)
                        {
                            long? userID = a.Message.PeerId;

                            if (!UserRanks.isExists(userID.ToString()))
                                UserRanks.Register(userID.ToString());

                            string userMessage = a.Message.Text.ToLower();
                            Log.Add(userMessage, userID, false);
                            var user = vkapi.Users.Get(new List<long> { (long)a.Message.PeerId }, VkNet.Enums.Filters.ProfileFields.FirstName)[0];

                            if (userMessage == "начать")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Купить товар", "");
                                keyboard.AddLine();
                                keyboard.AddButton("Позвать саппорта", "");
                                keyboard.AddButton("Ранги", "");
                                SendMessage($"{user.FirstName}, вас приветствует бот для онлайн покупки в нашем сообществе. Выберите интересующую вас команду:", userID, keyboard.Build());
                            }
                            else if (userMessage == "позвать саппорта")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Закрыть тикет", "", KeyboardButtonColor.Negative);
                                SendMessage($"Постарайтесь максимально подробно и понятно объяснить свою проблему или же задать вопрос тех поддержке. Вам ответят в ближайшее время!", userID, keyboard.Build());
                            }
                            else if (userMessage == "закрыть тикет")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Купить товар", "");
                                keyboard.AddLine();
                                keyboard.AddButton("Позвать саппорта", "");
                                keyboard.AddButton("Ранги", "");
                                SendMessage($"Тикет в тех поддержку успешно закрыт!", userID, keyboard.Build());
                            }
                            else if (userMessage == "купить товар")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                string[] market = Market.GetList(userID.ToString(), UserVkapi);

                                for (int i = 0; i < market.Length; i++)
                                {
                                    keyboard.AddButton(market[i].Split('|')[0], "");
                                    if (i % 2 == 1)
                                        keyboard.AddLine();
                                    if (i == market.Length - 1 && i % 2 == 0)
                                        keyboard.AddLine();
                                }

                                keyboard.AddButton("Отмена покупки", "", KeyboardButtonColor.Negative);

                                SendMessage($"Выберите один из доступных вам товаров:", userID, keyboard.Build());
                            }
                            else if (userMessage == "отмена покупки")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Купить товар", "");
                                keyboard.AddLine();
                                keyboard.AddButton("Позвать саппорта", "");
                                keyboard.AddButton("Ранги", "");
                                SendMessage($"Покупка отменена", userID, keyboard.Build());
                            }
                            else if (userMessage == "ранги" || userMessage == "назад")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Мой ранг", "");
                                keyboard.AddLine();
                                keyboard.AddButton("Закрыть", "", KeyboardButtonColor.Negative);

                                var userRank = UserRanks.UserRankInfo(userID.ToString());
                                var Ranks = UserRanks.GetList();
                                string ranks = "";

                                foreach (string rank in Ranks)
                                {
                                    var info = rank.Split(new string[] { ";;;5" }, StringSplitOptions.None);
                                    var market = info[3].Split(new string[] { ";0;" }, StringSplitOptions.None);
                                    ranks += $"{info[0]}\n&#12288;Скидка на покупку - {info[1]}%\n&#12288;Сумма для получения - {info[2]} рублей\n&#12288;Уникальные товары:\n&#12288;&#12288;{string.Join("\n&#12288;&#12288;", market.Select(x => { x = x.Split(' ')[0]; return x; }).ToList())}\n\n";
                                }

                                SendMessage($"{user.FirstName}, ваш текущий ранг - {userRank[0]}\n" +
                                    "Список доступных пользователям рангов:\n" + ranks, userID, keyboard.Build());
                            }
                            else if (userMessage == "закрыть")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Купить товар", "");
                                keyboard.AddLine();
                                keyboard.AddButton("Позвать саппорта", "");
                                keyboard.AddButton("Ранги", "");
                                SendMessage($"Страница закрыта. Выберите интересующую вас команду:", userID, keyboard.Build());
                            }
                            else if (userMessage == "мой ранг")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Назад", "", KeyboardButtonColor.Negative);

                                var userRank = UserRanks.UserRankInfo(userID.ToString());

                                SendMessage($"Ваш текущий ранг - {userRank[0]}\n" +
                                    $"Скидка при покупке - {userRank[1]}% \n" +
                                    $"Потраченная сумма - {UserRanks.Amount(userID.ToString())} рублей\n" +
                                    $"Следующий ранг - {UserRanks.NextRank(userID.ToString())}", userID, keyboard.Build());
                            }
                            else if (userMessage == "продолжить")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("QIWI", "");
                                Market.SetAgreement(userID.ToString());
                                SendMessage($"Выберите удобную для вас платежную систему", userID, keyboard.Build());
                            }
                            else if (userMessage == "отменить покупку")
                            {
                                KeyboardBuilder keyboard = new KeyboardBuilder();
                                keyboard.AddButton("Купить товар", "");
                                keyboard.AddLine();
                                keyboard.AddButton("Позвать саппорта", "");
                                keyboard.AddButton("Ранги", "");
                                Market.DelAgreement(userID.ToString());
                                SendMessage($"Покупка отменена", userID, keyboard.Build());
                            }
                            else if (userMessage == "qiwi")
                            {
                                if (Market.HasPayment(userID.ToString()))
                                {
                                    KeyboardBuilder keyboard = new KeyboardBuilder();
                                    keyboard.AddButton("Оплатил", "", KeyboardButtonColor.Positive);
                                    keyboard.AddLine();
                                    keyboard.AddButton("Отменить покупку", "", KeyboardButtonColor.Negative);

                                    var info = Market.PaymentInfo(userID.ToString());
                                    DateTime myDate = DateTime.ParseExact(info[2], "dd.MM.yyyy HH:mm:ss",
                                           System.Globalization.CultureInfo.InvariantCulture);
                                    SendMessage($"Для получения '{info[3]}', переведите {info[1]} руб на следующий номер QIWI: +{Qiwi.ContractInfo.contractId}\n В комментарий к платежу укажите: {info[5]}\nПлатеж будет действителен до {myDate.AddDays(Properties.Settings.Default.PaymentWait)}\n\nПосле оплаты нажмите на кнопку 'Оплатил'", userID, keyboard.Build());
                                }
                                else
                                    SendMessage("Запрос на покупку не найден!", userID, null);
                            }
                            else if (userMessage == "оплатил")
                            {
                                if (Properties.Settings.Default.PaymentCheck)
                                {
                                    System.Net.WebHeaderCollection headers = new System.Net.WebHeaderCollection();
                                    headers.Add("Authorization", $"Bearer {Properties.Settings.Default.TokenQIWI}");
                                    var response = GET($"https://edge.qiwi.com/payment-history/v2/persons/{Qiwi.ContractInfo.contractId}/payments?rows={Properties.Settings.Default.PaymentQueue}&operation=IN", null, headers, "application/json", "application/json");
                                    var paymentInfo = Market.PaymentInfo(userID.ToString());
                                    QIWIPayments.QIWIPayment qiwi = JsonConvert.DeserializeObject<QIWIPayments.QIWIPayment>(response);
                                    foreach (var payment in qiwi.Data)
                                    {
                                        if (payment.comment == paymentInfo[5] && payment.status.ToUpper() == "SUCCESS")
                                        {
                                            if (payment.total.currency == 643)
                                            {
                                                if (payment.total.amount >= Convert.ToDouble(paymentInfo[1]))
                                                {
                                                    DateTime lastDate = DateTime.ParseExact(payment.date.Replace("T", " ").Substring(0, payment.date.IndexOf("+")), "yyyy-MM-dd HH:mm:ss",
                                                      System.Globalization.CultureInfo.InvariantCulture);

                                                    DateTime createDate = DateTime.ParseExact(paymentInfo[2], "dd.MM.yyyy HH:mm:ss",
                                                      System.Globalization.CultureInfo.InvariantCulture);

                                                    if ((lastDate - createDate).TotalMilliseconds > 0)
                                                    {
                                                        KeyboardBuilder keyboard = new KeyboardBuilder();
                                                        keyboard.AddButton("Купить товар", "");
                                                        keyboard.AddLine();
                                                        keyboard.AddButton("Позвать саппорта", "");
                                                        keyboard.AddButton("Ранги", "");

                                                        SendMessage($"Платеж прошел успешно!\nСсылка на товар: {paymentInfo[4]}", userID, keyboard.Build());
                                                        Log.Payment(paymentInfo, userID);
                                                        Market.Success(userID.ToString(), paymentInfo[1]);
                                                        Market.DelAgreement(userID.ToString());
                                                    }
                                                    else
                                                        SendMessage($"Время на платеж истекло, обратитесь в тех поддержку для урегулирования вопроса.", userID, null);
                                                }
                                                else
                                                    SendMessage("Необходимо внести большую сумму. Обратитесь в тех поддержку для объединения платежей.", userID, null);
                                            }
                                            else
                                                SendMessage($"Неподдерживаемая валюта. Пожалуйста, обратитесь в тех поддержку.", userID, null);
                                        }
                                        else if (payment.status.ToUpper() == "WAITING")
                                            SendMessage($"Платеж находится в обработке", userID, null);
                                        else
                                            SendMessage($"Платеж не найден. Пожалуйста, обратитесь в тех поддержку, если это ошибка.", userID, null);
                                        break;
                                    }
                                }
                                else
                                {
                                    var paymentInfo = Market.PaymentInfo(userID.ToString());
                                    KeyboardBuilder keyboard = new KeyboardBuilder();
                                    keyboard.AddButton("Купить товар", "");
                                    keyboard.AddLine();
                                    keyboard.AddButton("Позвать саппорта", "");
                                    keyboard.AddButton("Ранги", "");

                                    SendMessage($"Платеж прошел успешно!\nСсылка на товар: {paymentInfo[4]}", userID, keyboard.Build());
                                    Log.Payment(paymentInfo, userID);
                                    Market.DelAgreement(userID.ToString());
                                }
                            }
                            else
                            {
                                string[] market = Market.GetList(userID.ToString(), UserVkapi);
                                string info = "";
                                List<string> products = new List<string>();

                                foreach (var m in market)
                                    if (m.ToLower().StartsWith(userMessage))
                                        info = m;

                                if (info != "")
                                {
                                    KeyboardBuilder keyboard = new KeyboardBuilder();
                                    keyboard.AddButton("Продолжить", "", KeyboardButtonColor.Positive);
                                    keyboard.AddLine();
                                    keyboard.AddButton("Отменить покупку", "", KeyboardButtonColor.Negative);

                                    var product = Market.MerketInfo(userID.ToString(), userMessage, UserVkapi);
                                    long? price = product.Price.Amount * (100 - UserRanks.Discount(userID.ToString())) / 100 / 100;
                                    SendMessage($"Выбранный продукт: {info.Split('|')[0]}\n" +
                                        $"Цена: {price} рублей\n" +
                                        $"Используемая скидка: {UserRanks.Discount(userID.ToString())}%\n" +
                                        $"Ссылка на товар: {info.Split('|')[1]}\n" +
                                        $"Уникальный ID платежа: {Market.CheckAgreement(userID.ToString(), product, price, info.Split('|')[2])}\n" +
                                        $"\n{user.FirstName}, уверены ли вы в покупке?", userID, keyboard.Build());
                                }
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
            }
            catch (Exception ex)
            {
                if (!Properties.Settings.Default.AutoStart)
                {
                    MessageBox.Show("Возникла ошибка во время работы бота. Необходим перезапуск!\n" + ex.Message);
                    main.Invoke(new MethodInvoker(() =>
                    {
                        main.button1.Text = "Запустить";
                    }));
                }
                else
                    goto Start;
            }
        }

        public static void SendMessage(string message, long? userID, MessageKeyboard keyboard)
        {
            Random rnd = new Random();
            vkapi.Messages.Send(new MessagesSendParams
            {
                RandomId = rnd.Next(),
                PeerId = userID,
                Message = message,
                Keyboard = keyboard
            });
            Log.Add(message, userID, true);
        }

        private void рангиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (BotVkThread == null || !BotVkThread.IsAlive)
                new Ranks().ShowDialog();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.TokenVk))
            {
                try
                {
                    using (VkApi vkApi = new VkApi())
                    {
                        vkApi.Authorize(new ApiAuthParams() { AccessToken = Properties.Settings.Default.TokenVk });
                        vkApi.Utils.ResolveScreenName("durov");
                        if (vkApi.IsAuthorized)
                        {
                            var id = vkApi.Utils.ResolveScreenName("club" + Properties.Settings.Default.IdVk);
                            if (id.Type != VkNet.Enums.VkObjectType.Group)
                            {
                                MessageBox.Show("Неверный GroupID");
                                Properties.Settings.Default.IdVk = "";
                                Properties.Settings.Default.TokenVk = "";
                                Properties.Settings.Default.Save();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Неверный токен ВКонтакте");
                            Properties.Settings.Default.IdVk = "";
                            Properties.Settings.Default.TokenVk = "";
                            Properties.Settings.Default.Save();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Неверный токен ВКонтакте");
                    Properties.Settings.Default.IdVk = "";
                    Properties.Settings.Default.TokenVk = "";
                    Properties.Settings.Default.Save();
                }
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.UserTokenVk))
            {
                //UserTokenVK
                try
                {
                    using (VkApi vkApi = new VkApi())
                    {
                        vkApi.Authorize(new ApiAuthParams() { AccessToken = Properties.Settings.Default.UserTokenVk, UserId = Convert.ToInt64(Properties.Settings.Default.UserVk) });
                        vkApi.Utils.ResolveScreenName("durov");
                        if (!vkApi.IsAuthorized)
                        {
                            MessageBox.Show("Неверный пользовательский токен ВКонтакте и/или UserID");
                            Properties.Settings.Default.UserTokenVk = "";
                            Properties.Settings.Default.UserVk = "";
                            Properties.Settings.Default.Save();
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Неверный пользовательский токен ВКонтакте и/или UserID");
                    Properties.Settings.Default.UserTokenVk = "";
                    Properties.Settings.Default.UserVk = "";
                    Properties.Settings.Default.Save();
                }
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.TokenQIWI))
            {
                //TokenQiWI
                try
                {
                    System.Net.WebHeaderCollection headers = new System.Net.WebHeaderCollection();
                    headers.Add("Authorization", $"Bearer {Properties.Settings.Default.TokenQIWI}");
                    var response = GET("https://edge.qiwi.com/person-profile/v1/profile/current", null, headers, "application/json", "application/json");

                    QIWI qiwi = JsonConvert.DeserializeObject<QIWI>(response);
                    Qiwi = qiwi;

                    long phone = qiwi.ContractInfo.contractId;
                }
                catch
                {
                    MessageBox.Show("Неверный токен QIWI");
                    Properties.Settings.Default.TokenQIWI = "";
                    Properties.Settings.Default.Save();
                }
            }

            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
                File.Create(@"Data\Ranks.txt").Dispose();
                File.Create(@"Data\Users.txt").Dispose();
                File.Create(@"Data\Payments.txt").Dispose();
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.UserTokenVk))
            {
                textBox1.Text = Properties.Settings.Default.UserTokenVk;
                textBox2.Text = Properties.Settings.Default.UserVk;
                textBox2.Enabled = false;
                textBox1.Enabled = false;
                button2.Text = "Редактировать";
            }
            else
            {
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                button2.Text = "Сохранить";
            }

        }
        private static string GET(string Url, string Data, System.Net.WebHeaderCollection headers, string accept, string ContentType)
        {
            HttpWebRequest req = ((!string.IsNullOrEmpty(Data)) ? System.Net.WebRequest.Create(Url + "?" + Data) : System.Net.WebRequest.Create(Url)) as HttpWebRequest;
            if (!string.IsNullOrEmpty(accept))
                req.Accept = accept;
            if (!string.IsNullOrEmpty(ContentType))
                req.ContentType = ContentType;
            req.Headers = headers;
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.Stream stream = resp.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Process.Start("https://oauth.vk.com/authorize?client_id=6121396&scope=1073737727&redirect_uri=https://oauth.vk.com/blank.html&display=page&response_type=token&revoke=1");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (BotVkThread == null || !BotVkThread.IsAlive)
            {
                if (button2.Text == "Сохранить")
                {
                    try
                    {
                        UserVkapi.Authorize(new ApiAuthParams() { AccessToken = textBox1.Text, UserId = Convert.ToInt64(textBox2.Text.StartsWith("id") ? textBox2.Text.Substring(2) : textBox2.Text) });
                        UserVkapi.Utils.ResolveScreenName("durov");
                        if (!UserVkapi.IsAuthorized)
                            MessageBox.Show("Неверный токен и/или UserId");
                        else
                        {
                            Properties.Settings.Default.UserTokenVk = textBox1.Text;
                            Properties.Settings.Default.UserVk = textBox2.Text;
                            Properties.Settings.Default.Save();
                            textBox1.Enabled = false;
                            textBox2.Enabled = false;
                            button2.Text = "Редактировать";
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Неверный токен и/или UserId");
                    }
                }
                else
                {
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    button2.Text = "Сохранить";
                }
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BotVkThread != null && BotVkThread.IsAlive)
                BotVkThread.Abort();
            System.Windows.Forms.Application.Exit();
        }

        private void платежиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ViewPayments().Show();
        }
    }
}
