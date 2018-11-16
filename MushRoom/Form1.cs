using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing;


// NOTES
// @TODO : progressbar, highlight current file state, settings for file save, allow directory drop (ifDirectory --> scan directory funct.) 
// 
//
namespace MushRoom
{
    public partial class Form1 : Form
    {
        public string customTarget;
        public bool cancel;

        public Form1()
        {
            InitializeComponent();

            // Attach Event Handlers to BackgroundWorker
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);

            toolStripStatusLabel2.Text = "@" + Properties.Settings.Default.quality + " Kbps >";

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
        }

        private void canAdd(string item) {
            // check extension of the file

            if (Path.GetExtension(item) == ".flac")
            {
                listBox1.Items.Add(item);
                button1.Enabled = true;
                button4.Enabled = true;
            }
            else {
             
            }
        } 

        // Click CONVERT button
        private void button1_Click(object sender, EventArgs e)
        {
            // check for 
            if (Properties.Settings.Default.askfolder) { 

                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    customTarget = folderBrowserDialog1.SelectedPath + "\\";
                }

            }

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

        // Parse the FLAC file
        private void parseFlac(string file) {

            StreamReader SROutput = null;

            // Set
            string flac_file = file;
            string mp3_file = Path.ChangeExtension(file, ".mp3");
            string quality = Properties.Settings.Default.quality;
            toolStripStatusLabel3.Text = "Creating " + Path.GetFileName(mp3_file) + " ... ";

            if(Properties.Settings.Default.samefolder == false && Directory.Exists(Properties.Settings.Default.target.ToString())){
                mp3_file = Properties.Settings.Default.target + "\\" + Path.GetFileName(file);
                mp3_file = Path.ChangeExtension(mp3_file, ".mp3");
            }

            if (Properties.Settings.Default.askfolder && Directory.Exists(customTarget)) {
                mp3_file = customTarget + Path.GetFileName(file);
                mp3_file = Path.ChangeExtension(mp3_file, ".mp3");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Application.StartupPath + @"\common\ffmpeg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // configuration -320k -map_metadata 0 -id3v2_version 3
            startInfo.Arguments = $"-y -i \"{flac_file}\" -b:a {quality}k -map_metadata 0 -id3v2_version 3 \"{mp3_file}\"";
         
            // Run FFMPEG
            try
            {
                Process exeProcess = Process.Start(startInfo);
                SROutput = exeProcess.StandardError;

                //ffoutput = SROutput.ReadToEnd();

                // progression
                int total_seconds = 0;
                int current_second = 0;
                int subprogress = 0;

                // Read FFMPEG Output and update Progression
                do
                {
                    string s = SROutput.ReadLine();
                  
                    // get total duration
                    if (s.Contains("Duration: "))
                    {
                      total_seconds = this.ConverttoSeconds(s.Substring(s.LastIndexOf("Duration: "), 18).Replace("Duration: ", ""));
                    //Console.WriteLine(total_seconds);
                    }

                    // check for progression
                    if (s.Contains("size= ") && s.Contains("time="))
                    {
                        current_second = this.ConverttoSeconds(s.Substring(s.LastIndexOf("time="), 13).Replace("time=", ""));

                        if (total_seconds != 0) {
                            subprogress = (100 * current_second) / total_seconds;
                            //Console.WriteLine(subprogress + " " + mp3_file);
                            backgroundWorker1.ReportProgress(subprogress);
                        }

                    }

                } while (!SROutput.EndOfStream);

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
            Settings.Show();
        }

        // Reload Settings 
        public void refreshing() {
            
            Properties.Settings.Default.Reload();
            toolStripStatusLabel2.Text = "@" + Properties.Settings.Default.quality + " Kbps >";
            this.Refresh();
        }

        // Background Worker
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;
            string convert = (string)e.Argument;

            // Update status bar 
            toolStripStatusLabel3.Text = "Starting conversion !";
            int i = 1; // no need for this really
            foreach (var listBoxItem in listBox1.Items)
            {
                // check cancel
                if (worker.CancellationPending == true) {
                    e.Cancel = true;
                    break;
                }
        
                // Parse the FLAC file
                i++;  // no need for this really 
                this.parseFlac(listBoxItem.ToString());
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
                listBox1.Items.Clear();
                progressBar1.Value = 0;
                this.AllowDrop = true;
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

        // DrawItem ( currently not used, need to convert listbox to objects and such (map coloring class) ) 
        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), listBox1.Font, Brushes.Green, e.Bounds);
        }

        // Nothing ?
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // clear list
        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            button4.Enabled = false;
        }
    }
}
