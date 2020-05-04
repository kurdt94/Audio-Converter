using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

// NOTES

namespace MushRoom
{
    public partial class Form1 : Form
    {
        public string customTarget;
        public bool cancel;
        public List<string> doneList;

        public Form1()
        {
            InitializeComponent();
            toolStripStatusLabel2.Text = "@" + Properties.Settings.Default.quality + " > ";
            doneList = new List<string>();
            // Attach Event Handlers to BackgroundWorker
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);

            // Set Drop Functionality
            this.cancel = false;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter); 
            this.DragDrop += new DragEventHandler(Form1_DragDrop); 
        }

        // On Enter
        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        // On Drop
        void Form1_DragDrop(object sender, DragEventArgs e)
        {

            // Don't drop on running progress
            if (backgroundWorker1.IsBusy == true)
            {
                MessageBox.Show("Can't drop files while running", "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) { this.canAdd(file); }
            ResizeColumns();

        }

        private void ResizeColumns() {
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void canAdd(string item) {
            // check extension of the file

            string ext = Path.GetExtension(item).ToLower();
            string[] supp = {
                ".flac", ".wav", ".aac", ".ac3", ".ape",".alac",".wma",".ogg",".mogg",".mp3",
                ".flv",".mp4",".vob",".avi",".mkv",".wmv",".mpg",".mpeg",".mov",".qt"};

            if (supp.Contains(ext))
            {
                ListViewItem itemlv = new ListViewItem();
                itemlv.Text = Path.GetFileName(item); //filename
                itemlv.SubItems.Add(Path.GetExtension(item)); //extension
                itemlv.SubItems.Add("Pending"); // status
                itemlv.SubItems.Add(item); // path
                itemlv.UseItemStyleForSubItems = false;
                listView1.Items.Add(itemlv);

                button1.Enabled = true;
                button4.Enabled = true;
                listBox1.Items.Add(item);
            }
            
        } 

        // Click CONVERT button
        private void button1_Click(object sender, EventArgs e)
        {
            // check for Ask for folder
            if (Properties.Settings.Default.askfolder) { 

                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    customTarget = folderBrowserDialog1.SelectedPath + "\\";
                }
                else
                {
                    return;
                }

            }

            // check if field is empty and askfolder is also unchecked
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.target) && Properties.Settings.Default.samefolder == false)
            {
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.target = folderBrowserDialog1.SelectedPath + "\\";
                    customTarget = folderBrowserDialog1.SelectedPath + "\\";
                }
                else {
                    return;
                }

            }
            

            // RunWorker
            if (backgroundWorker1.IsBusy != true)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = true;
                button4.Enabled = false;
                this.AllowDrop = false;
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        // Parse the FLAC (any audio) file
        private void parseFlac(string file, int row) {

            // if (listView1.Items[row].SubItems[2].Text == "Done" ) { return; }

            StreamReader FFOutput = null;

            string flac_file = file; // any supported audio file
            string mp3_file = Path.ChangeExtension(file, ".mp3");
            string quality = Properties.Settings.Default.quality;
         

            if(Properties.Settings.Default.samefolder == false && Directory.Exists(Properties.Settings.Default.target.ToString())){
                mp3_file = Properties.Settings.Default.target + "\\" + Path.GetFileName(file);
                mp3_file = Path.ChangeExtension(mp3_file, ".mp3");
            }

            if (Properties.Settings.Default.askfolder && Directory.Exists(customTarget)) {
                mp3_file = customTarget + Path.GetFileName(file);
                mp3_file = Path.ChangeExtension(mp3_file, ".mp3");
            }

            if (File.Exists(mp3_file)) {
                mp3_file = Path.ChangeExtension(mp3_file, quality + ".mp3");
                mp3_file = mp3_file.Replace('.' + quality, " (" + quality + ")");

            }

            // Process Setup
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Application.StartupPath + @"\common\ffmpeg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // configuration for other formats
            // need to redo this stuff
            if (quality == "FLAC") {
               
                mp3_file = Path.ChangeExtension(flac_file, ".flac");
                toolStripStatusLabel3.Text = "Creating " + Path.GetFileName(mp3_file) + " ... ";
                startInfo.Arguments = $"-y -i \"{flac_file}\" -c:a flac -compression_level 12 \"{mp3_file}\"";
            }

            // wav 
            if (quality == "WAV 44-16")
            {
                mp3_file = Path.ChangeExtension(flac_file, ".wav");
                toolStripStatusLabel3.Text = "Creating " + Path.GetFileName(mp3_file) + " ... ";
                startInfo.Arguments = $"-y -i \"{flac_file}\" \"{mp3_file}\"";
            }
            else {
                // default configuration -320k -map_metadata 0 -id3v2_version 3
                toolStripStatusLabel3.Text = "Creating " + Path.GetFileName(mp3_file) + " ... ";
                quality = quality.Replace("MP3 ", "");
                startInfo.Arguments = $"-y -i \"{flac_file}\" -b:a {quality}k -map_metadata 0 -id3v2_version 3 \"{mp3_file}\"";
            }
            // Run FFMPEG
            try
            {
                Process exeProcess = Process.Start(startInfo);
                FFOutput = exeProcess.StandardError;

                // progression
                int total_seconds = 0;
                int current_second = 0;
                int subprogress = 0;

                // Read FFMPEG Output and update Progression
                do
                {
                    string s = FFOutput.ReadLine();
                  
                    // catch total duration
                    if (s.Contains("Duration: "))
                    {
                      total_seconds = this.ConverttoSeconds(s.Substring(s.LastIndexOf("Duration: "), 18).Replace("Duration: ", ""));
                    }

                    // check for progression
                    if (s.Contains("size= ") && s.Contains("time="))
                    {
                        current_second = this.ConverttoSeconds(s.Substring(s.LastIndexOf("time="), 13).Replace("time=", ""));

                        if (total_seconds != 0) {
                            subprogress = (100 * current_second) / total_seconds;
                            backgroundWorker1.ReportProgress(subprogress);
                        }

                    }

                } while (!FFOutput.EndOfStream);

                // wait for exit and close process
                exeProcess.WaitForExit();
                exeProcess.Close();
                exeProcess.Dispose(); // needed ?  ?
                
            }
            catch (Exception ex)
            {
                   Console.WriteLine(ex);
            }

        }

        // Convert Track lenght to seconds
        private int ConverttoSeconds(string s)
        {
            //we can just parse a time format "00:00:11" and return an integer
            double seconds = TimeSpan.Parse(s).TotalSeconds;
            return Convert.ToInt32(seconds); 
        }

        // Settings
        private void button2_Click(object sender, EventArgs e)
        {
            // Open Settings
            var Settings = new Form2(this);
            Settings.ShowDialog();
        }

        // Reload Settings 
        public void refreshing() {
            
            Properties.Settings.Default.Reload();
            toolStripStatusLabel2.Text = "@" + Properties.Settings.Default.quality + " > ";
            this.Refresh();
        }

        // Background Worker
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            string convert = (string)e.Argument;

            // Update status bar 
            toolStripStatusLabel3.Text = "Starting conversion !";
            int i = 0;
            foreach (var listBoxItem in listBox1.Items)
            {

                // check cancel
                if (worker.CancellationPending == true) {
                    e.Cancel = true;
                    break;
                }
                        
                if (listView1.InvokeRequired && doneList.Contains(i + "_done") == false)
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                     
                        listView1.Items[i].SubItems[2].ForeColor = Color.Orange;
                        listView1.Items[i].SubItems[2].Text = "Busy";
                       
                    });
                }
                if (doneList.Contains(i+"_done")) { i++; continue; }
                this.parseFlac(listBoxItem.ToString(),i);

                if (listView1.InvokeRequired)
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        listView1.Items[i].SubItems[2].ForeColor = Color.Green;
                        listView1.Items[i].SubItems[2].Text = "Done";
                        doneList.Add(i+"_done");
                    });
                }
                // Parse the FLAC file
                i++;
               
            }
            e.Result = true;

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.cancel)
            {
                toolStripStatusLabel3.Text = "Stopping conversion ...";
            }
            else {
                progressBar1.Value = e.ProgressPercentage;
            }
           
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true || this.cancel)
            {
                toolStripStatusLabel3.Text = "Last conversion canceled !";
                button3.Enabled = false;
                button1.Enabled = true;
                button4.Enabled = false;
                progressBar1.Value = 0;
                this.cancel = false;
                this.AllowDrop = true;
            }
            else if (e.Error != null)
            {
                toolStripStatusLabel3.Text = "Error while converting: " + e.Error.Message;

            }
            else
            {
                toolStripStatusLabel3.Text = "Finished converting files !";
                button3.Enabled = false;
                button2.Enabled = true;
                progressBar1.Value = 0;
                this.AllowDrop = true;

                // AutoClear Setting
                if (Properties.Settings.Default.clearlist)
                {
                    listBox1.Items.Clear();
                    listView1.Items.Clear();
                    doneList.Clear();
                }
                else {
                    button4.Enabled = true;
                }

            }
        }
        
        // Cancel Button
        private void button3_Click(object sender, EventArgs e)
        {
            this.cancel = true;
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                backgroundWorker1.CancelAsync();
            }
        }

        // Nothing ?
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // clear list
        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listView1.Items.Clear();
            doneList.Clear();
            button4.Enabled = false;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open About
            var About = new Form3();
            About.ShowDialog();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);

                foreach (var file in files)
                {
                    canAdd(file);
                }
                ResizeColumns();
            }
            else
            {
                return;
            }
        }

        private void advancedConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open Settings
            var Settings = new Form2(this);
            Settings.ShowDialog();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // Open About
            var About = new Form3();
            About.ShowDialog();
        }
    }
}
