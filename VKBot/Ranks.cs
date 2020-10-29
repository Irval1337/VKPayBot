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
    public partial class Ranks : Form
    {
        List<string> infoLines = new List<string>();

        public Ranks()
        {
            InitializeComponent();
        }

        private void Ranks_Load(object sender, EventArgs e)
        {
            UpdateList();
            checkBox2.Checked = Properties.Settings.Default.MarketInheritance;
        }

        void UpdateList()
        {
            listBox1.Items.Clear();
            var _lines = File.ReadAllLines(@"Data\Ranks.txt");
            foreach (string line in _lines)
                listBox1.Items.Add(line.Split(new string[] { ";;;5" }, StringSplitOptions.None)[0]);

            infoLines = _lines.ToList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
                File.AppendAllLines(@"Data\Ranks.txt", new string[] { $"{textBox1.Text};;;5null;;;5null;;;5null" });
            UpdateList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var lines = infoLines;
                List<string> newLines = new List<string>();
                for (int i = 0; i < lines.Count; i++)
                    if (i != listBox1.SelectedIndex)
                        newLines.Add(lines[i]);
                File.WriteAllLines(@"Data\Ranks.txt", newLines);

                UpdateList();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0 && !string.IsNullOrEmpty(textBox1.Text))
            {
                var lines = infoLines;
                List<string> newLines = new List<string>();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (i != listBox1.SelectedIndex)
                        newLines.Add(lines[i]);
                    else
                    {
                        var spl = lines[i].Split(new string[] { ";;;5" }, StringSplitOptions.None);
                        newLines.Add($"{textBox1.Text};;;5{string.Join(";;;5", spl.Skip(1))}");
                    }
                }
                File.WriteAllLines(@"Data\Ranks.txt", newLines);

                UpdateList();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var lines = infoLines;

                lines.Insert(listBox1.SelectedIndex - 1, lines[listBox1.SelectedIndex]);
                lines.RemoveAt(listBox1.SelectedIndex + 1);
                File.WriteAllLines(@"Data\Ranks.txt", lines);

                UpdateList();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var lines = infoLines;

                lines.Insert(listBox1.SelectedIndex + 1, lines[listBox1.SelectedIndex]);
                lines.RemoveAt(listBox1.SelectedIndex);
                File.WriteAllLines(@"Data\Ranks.txt", lines);

                UpdateList();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button5.Enabled = listBox1.SelectedIndex > 0;
            button6.Enabled = listBox1.SelectedIndex < listBox1.Items.Count - 1;

            checkBox1.Checked = listBox1.SelectedIndex == 0;

            var info = infoLines[listBox1.SelectedIndex].Split(new string[] { ";;;5" }, StringSplitOptions.None);
            textBox2.Text = info[1];
            textBox3.Text = info[2];
            richTextBox1.Lines = info[3].Split(new string[] { ";0;" }, StringSplitOptions.None);

            listBox2.Items.Clear();
            if (checkBox2.Checked)
            {
                for (int i = 0; i < listBox1.SelectedIndex; i++)
                {
                    var inf = infoLines[i].Split(new string[] { ";;;5" }, StringSplitOptions.None)[3].Split(new string[] { ";0;" }, StringSplitOptions.None);
                    foreach (var str in inf)
                        listBox2.Items.Add(str);
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var info = infoLines[listBox1.SelectedIndex].Split(new string[] { ";;;5" }, StringSplitOptions.None);

                if (listBox1.SelectedIndex > 0 || textBox2.Text == "0")
                    infoLines[listBox1.SelectedIndex] = $"{info[0]};;;5{textBox2.Text};;;5{textBox3.Text};;;5{info[3]}";
                else
                {
                    infoLines[listBox1.SelectedIndex] = $"{info[0]};;;5{textBox2.Text};;;5{0};;;5{info[3]}";
                    MessageBox.Show("Не удается установить сумму, большую 0 рублей для первого ранга!");
                }
                File.WriteAllLines(@"Data\Ranks.txt", infoLines);
                UpdateList();
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!tb2_isBack)
                tb2_isBack = Char.IsDigit(e.KeyChar);

            e.Handled = !tb2_isBack;
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!tb3_isBack)
                tb3_isBack = Char.IsDigit(e.KeyChar);

            e.Handled = !tb3_isBack;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MarketInheritance = checkBox2.Checked;
            Properties.Settings.Default.Save();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                richTextBox1.Text = "";
                var info = infoLines[listBox1.SelectedIndex].Split(new string[] { ";;;5" }, StringSplitOptions.None);

                infoLines[listBox1.SelectedIndex] = $"{info[0]};;;5{textBox2.Text};;;5{textBox3.Text};;;5null";
                File.WriteAllLines(@"Data\Ranks.txt", infoLines);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var info = infoLines[listBox1.SelectedIndex].Split(new string[] { ";;;5" }, StringSplitOptions.None);

                infoLines[listBox1.SelectedIndex] = $"{info[0]};;;5{textBox2.Text};;;5{textBox3.Text};;;5{string.Join(";0;", richTextBox1.Lines)}";
                File.WriteAllLines(@"Data\Ranks.txt", infoLines);
            }
        }

        bool tb2_isBack = false;
        bool tb3_isBack = false;

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            tb2_isBack = e.KeyCode == Keys.Back;
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            tb3_isBack = e.KeyCode == Keys.Back;
        }
    }
}
