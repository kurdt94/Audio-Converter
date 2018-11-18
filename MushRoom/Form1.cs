using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Linq;


// NOTES
// @TODO : highlight current state, allow directory drop? 
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
            toolStripStatusLabel2.Text = "@" + Properties.Settings.Default.quality + " Kbps >";

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

            // @TODO: autodrop
            // this.button1.PerformClick();
        }

        private void canAdd(string item) {
            // check extension of the file
            bool confirmed = false;
            string ext = Path.GetExtension(item).ToLower();

            // support list
            if (ext == ".flac") { confirmed = true; } // FLAC Free Lossless Audio Codec
            if (ext == ".wav") { confirmed = true; } // WAV WavPack
            if (ext == ".aac") { confirmed = true; } // AAC Advanced Audio Coding
            if (ext == ".ac3") { confirmed = true; } // AC3 ATSC A/52A (AC-3)
            if (ext == ".ape") { confirmed = true; } // APE Monkey's Audio
            if (ext == ".alac") { confirmed = true; } // ALAC Apple Lossless Audio Codec
            if (ext == ".wma") { confirmed = true; } // WMA Windows Media Audio
            if (ext == ".ogg") { confirmed = true; } // Ogg Xiph.Org
            if (ext == ".mogg") { confirmed = true; } // Multitrack Ogg file
            if (ext == ".mp3") { confirmed = true; } // Mp3
            // support list video 
            if (ext == ".flv") { confirmed = true; } // FLV
            if (ext == ".mp4") { confirmed = true; } // MP4
            if (ext == ".vob") { confirmed = true; } // Vob
            if (ext == ".avi") { confirmed = true; } // AVI
            if (ext == ".mkv") { confirmed = true; } // Matroska
            if (ext == ".wmv") { confirmed = true; } // Windows Media Video
            if (ext == ".mpg") { confirmed = true; } // MPEG Video
            if (ext == ".mpeg") { confirmed = true; } // MPEG Video
            if (ext == ".mov") { confirmed = true; } // QuickTime File Format
            if (ext == ".qt") { confirmed = true; } // QuickTime File Format

            if (confirmed)
            {
                ListViewItem itemlv = new ListViewItem();
                itemlv.Text = Path.GetFileName(item); //filename
                itemlv.SubItems.Add(Path.GetExtension(item)); //extension
                itemlv.SubItems.Add("Pending"); // status
                itemlv.SubItems.Add(item); // path
                itemlv.UseItemStyleForSubItems = false;
                listView1.Items.Add(itemlv);
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                button1.Enabled = true;
                button4.Enabled = true;
                listBox1.Items.Add(item);
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

            StreamReader FFOutput = null;

            // Set
            string flac_file = file;
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

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = Application.StartupPath + @"\common\ffmpeg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // configuration -320k -map_metadata 0 -id3v2_version 3
            startInfo.Arguments = $"-y -i \"{flac_file}\" -b:a {quality}k -map_metadata 0 -id3v2_version 3 \"{mp3_file}\"";
            toolStripStatusLabel3.Text = "Creating " + Path.GetFileName(mp3_file) + " ... ";
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
            int i = 0; // no need for this really
            foreach (var listBoxItem in listBox1.Items)
            {
                // check cancel
                if (worker.CancellationPending == true) {
                    e.Cancel = true;
                    break;
                }
                if (listView1.InvokeRequired)
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        listView1.Items[i].SubItems[2].ForeColor = Color.Orange;
                        listView1.Items[i].SubItems[2].Text = "Busy";

                    });
                }
                this.parseFlac(listBoxItem.ToString());
                if (listView1.InvokeRequired)
                {
                    listView1.Invoke((MethodInvoker)delegate ()
                    {
                        listView1.Items[i].SubItems[2].ForeColor = Color.Green;
                        listView1.Items[i].SubItems[2].Text = "Done";

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
                listView1.Items.Clear();
                progressBar1.Value = 0;
                this.AllowDrop = true;
                 
                // AutoClear Setting?

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
            button4.Enabled = false;
        }
    }
}
