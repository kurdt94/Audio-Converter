using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace MushRoom
{
    public partial class Form2 : Form
    {

        private Form1 main;

        public Form2(Form1 mainForm)
        {
            InitializeComponent();

            main = mainForm;

            //Properties.Settings.Default.key01 = DateTime.Now.ToString();
            //Properties.Settings.Default.Save();
            //Properties.Settings.Default.Reload();

            comboBox1.SelectedItem = Properties.Settings.Default.quality;
            textBox1.Text = Properties.Settings.Default.target;
            checkBox3.Checked = Properties.Settings.Default.clearlist;
            checkBox2.Checked = Properties.Settings.Default.askfolder;
            checkBox1.Checked = Properties.Settings.Default.samefolder;


        }

        // Save
        private void button1_Click(object sender, EventArgs e)
        {

            // set Quality
            Properties.Settings.Default.quality = comboBox1.SelectedItem.ToString();

            // set Location properties
            Properties.Settings.Default.target = textBox1.Text.ToString();
            Properties.Settings.Default.clearlist = checkBox3.Checked;
            Properties.Settings.Default.askfolder = checkBox2.Checked;
            Properties.Settings.Default.samefolder = checkBox1.Checked;
            Properties.Settings.Default.Save();


            main.refreshing();
            this.Close();
        }

        // Cancel
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        // Save in same folder ?
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox1.Enabled = false;
                checkBox2.Checked = false;
                button3.Enabled = false;
            }
            else {
                textBox1.Enabled = true;
                button3.Enabled = true;
            }
        }

        // ask for target folder checkbox
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked) {
                checkBox1.Checked = false;
            }
            if (checkBox2.Checked)
            {
                textBox1.Enabled = false;
                checkBox1.Checked = false;
                button3.Enabled = false;
            }
            else {
                textBox1.Enabled = false;
                checkBox1.Checked = true;
                button3.Enabled = false;
            }
        }


        // Target Location textBox Changed
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // browse for folder button
        private void button3_Click(object sender, EventArgs e)
        {

            string defaultPath = "";
            // browse

            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                defaultPath = folderBrowserDialog1.SelectedPath;
            }

            textBox1.Text = defaultPath;

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
