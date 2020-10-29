using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VKBot
{
    public partial class Payments : Form
    {
        string Date;
        ViewPayments Main;
        public Payments(string date, ViewPayments main)
        {
            InitializeComponent();
            Date = date;
            Main = main;
        }

        private void Payments_Load(object sender, EventArgs e)
        {
            string path = $@"Logs\Payments\{Date}.txt";

            if (!File.Exists(path))
            {
                MessageBox.Show("Запись не найдена");
                this.Close();
            }
            else
            {
                var payments = File.ReadAllLines(path);
                foreach (var line in payments)
                {
                    var info = line.Split(new string[] { ";;;5" }, StringSplitOptions.None);
                    dataGridView1.Rows.Add(info);
                }
            }
        }

        private void Payments_FormClosed(object sender, FormClosedEventArgs e)
        {
            Main.Close();
        }
    }
}
