using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VkNet;
using VkNet.Model;
using static VKBot.QIWIUserInfo;

namespace VKBot
{
    public partial class Settings : Form
    {
        public static VkApi vkapi = new VkApi();

        public Settings()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.TokenVk) || string.IsNullOrEmpty(Properties.Settings.Default.IdVk))
            {
                textBox1.Enabled = true;
                textBox3.Enabled = true;

                textBox1.Text = Properties.Settings.Default.TokenVk;
                textBox3.Text = Properties.Settings.Default.IdVk;

                button1.Text = "Сохранить";
            }
            else
            {
                textBox1.Enabled = false;
                textBox3.Enabled = false;
                textBox1.Text = Properties.Settings.Default.TokenVk;
                textBox3.Text = Properties.Settings.Default.IdVk;
                button1.Text = "Редактировать";
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.TokenQIWI))
            {
                textBox2.Enabled = true;
                button2.Text = "Сохранить";
            }
            else
            {
                textBox2.Enabled = false;
                textBox2.Text = Properties.Settings.Default.TokenQIWI;
                button2.Text = "Редактировать";
            }

            radioButton1.Checked = Properties.Settings.Default.PaymentCheck;
            textBox5.Text = Properties.Settings.Default.PaymentWait.ToString();
            textBox4.Text = Properties.Settings.Default.PaymentQueue.ToString();
            radioButton2.Checked = Properties.Settings.Default.AutoStart;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Сохранить")
            {
                try
                {
                    vkapi.Authorize(new ApiAuthParams() { AccessToken = textBox1.Text });
                    vkapi.Utils.ResolveScreenName("durov");
                    if (vkapi.IsAuthorized)
                    {
                        var id = vkapi.Utils.ResolveScreenName(int.TryParse(textBox3.Text, out int int32) ? $"club{textBox3.Text}" : textBox3.Text);
                        if (id.Type == VkNet.Enums.VkObjectType.Group)
                        {
                            Properties.Settings.Default.IdVk = id.Id.Value.ToString();
                            Properties.Settings.Default.TokenVk = textBox1.Text;
                            Properties.Settings.Default.Save();
                            textBox1.Enabled = false;
                            textBox3.Enabled = false;
                            button1.Text = "Редактировать";
                        }
                        else
                            MessageBox.Show("Неверный GroupID");
                    }
                    else
                        MessageBox.Show("Неверный токен");
                }
                catch
                {
                    MessageBox.Show("Неверный токен");
                }
            }
            else
            {
                textBox1.Enabled = true;
                textBox3.Enabled = true;
                button1.Text = "Сохранить";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Сохранить")
            {
                try
                {
                    System.Net.WebHeaderCollection headers = new System.Net.WebHeaderCollection();
                    headers.Add("Authorization", $"Bearer {textBox2.Text}");
                    var response = GET("https://edge.qiwi.com/person-profile/v1/profile/current", null, headers, "application/json", "application/json");

                    QIWI qiwi = JsonConvert.DeserializeObject<QIWI>(response);

                    long phone = qiwi.ContractInfo.contractId;

                    Properties.Settings.Default.TokenQIWI = textBox2.Text;
                    Properties.Settings.Default.Save();
                    textBox2.Enabled = false;
                    button2.Text = "Редактировать";

                }
                catch
                {
                    MessageBox.Show("Неверный токен");
                }
            }
            else
            {
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

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!tb5_isBack)
                tb5_isBack = Char.IsDigit(e.KeyChar);

            e.Handled = !tb5_isBack;
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!tb4_isBack)
                tb4_isBack = Char.IsDigit(e.KeyChar);

            e.Handled = !tb4_isBack;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int PaymentWait = Convert.ToInt32(textBox5.Text);
            int PaymentQueue = Convert.ToInt32(textBox4.Text);

            if (PaymentWait > 7 || PaymentWait < 0)
                MessageBox.Show("Ошибка. Невозможно установить данный лимит для ожидания оплаты!");
            else if (PaymentQueue > 50 || PaymentQueue < 0)
                MessageBox.Show("Ошибка. Невозможно установить данный лимит для проверяемых операций!");
            else
            {
                Properties.Settings.Default.PaymentCheck = radioButton1.Checked;
                Properties.Settings.Default.PaymentQueue = PaymentQueue;
                Properties.Settings.Default.PaymentWait = PaymentWait;
                Properties.Settings.Default.Save();
            }
        }

        bool tb5_isBack = false;
        bool tb4_isBack = false;

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            tb5_isBack = e.KeyCode == Keys.Back;
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            tb4_isBack = e.KeyCode == Keys.Back;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoStart = radioButton2.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
