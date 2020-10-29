using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VKBot
{
    public partial class ViewPayments : Form
    {
        public ViewPayments()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            new Payments(dateTimePicker1.Value.ToString().Split(' ')[0], this).Show();
        }
    }
}
