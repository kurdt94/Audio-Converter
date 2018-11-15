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

        public Form1()
        {
            InitializeComponent();

            // Attach Event Handlers to BackgroundWorker
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(
            backgroundWorker1_RunWorkerCompleted);

            toolStripStatusLabel2.Text = " > (" + Properties.Settings.Default.quality + "Kbps)";
            // label1.Text = Properties.Settings.Default.key01;
            // Set Drop Functionality
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter); // <--
            this.DragDrop += new DragEventHandler(Form1_DragDrop); // <--
        }

        // On Enter
        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        // On Drop
        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) { this.canAdd(file); }
        }

        private void canAdd(string item) {
            // check extension of the file

            if (Path.GetExtension(item) == ".flac")
            {
                listBox1.Items.Add(item);
                button1.Enabled = true;
            }
            else {
               // listBox1.Items.Add(Path.GetExtension(item));
            }
        } 

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

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
                // Start the asynchronous operation.
                backgroundWorker1.RunWorkerAsync();
            }
        }

        // Parse the FLAC file
        private void parseFlac(string file) {

            // Set
            string flac_file = file;
            string mp3_file = Path.ChangeExtension(file, ".mp3");
            string quality = Properties.Settings.Default.quality;
            toolStripStatusLabel1.Text = "creating " + Path.GetFileName(mp3_file) + " ... ";

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
            startInfo.UseShellExecute = false;
            startInfo.FileName = Application.StartupPath + @"\common\ffmpeg.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            // configuration -320k -map_metadata 0 -id3v2_version 3
            startInfo.Arguments = $"-y -i \"{flac_file}\" -b:a {quality}k -map_metadata 0 -id3v2_version 3 \"{mp3_file}\"";
         
            // Run FFMPEG
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                   Console.WriteLine(ex);
            }

            // convert the file
            // ffmpeg -i input.flac -ab 320k -map_metadata 0 -id3v2_version 3 output.mp3


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
            toolStripStatusLabel2.Text = " > (" + Properties.Settings.Default.quality + "Kbps)";
            this.Refresh();
            //label1.Text = Properties.Settings.Default.quality;

        }

        // Background Worker
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            BackgroundWorker worker = sender as BackgroundWorker;

            string convert = (string)e.Argument;

            // Update status bar 
            toolStripStatusLabel1.Text = "Starting conversion !";
            int i = 1;
            foreach (var listBoxItem in listBox1.Items)
            {
                // check cancel
                if (worker.CancellationPending == true) {
                    e.Cancel = true;
                    break;
                }
        
                // Parse the FLAC file
                i++;
                this.parseFlac(listBoxItem.ToString());
            }
            //
            e.Result = true;
            //e.Cancel = true;
         
        }


    // This event handler deals with the results of the background operation.
    private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                toolStripStatusLabel1.Text = "Conversion canceled!";
            }
            else if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "Error while converting: " + e.Error.Message;
            }
            else
            {
                toolStripStatusLabel1.Text = "Finished conversion !";
                button3.Enabled = false;
                button2.Enabled = true;
                listBox1.Items.Clear();

            }
        }
        
        // Cancel Button
        private void button3_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.WorkerSupportsCancellation == true)
            {
                // Cancel the asynchronous operation.
                backgroundWorker1.CancelAsync();
             
            }
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.Graphics.DrawString(listBox1.Items[e.Index].ToString(), listBox1.Font, Brushes.Green, e.Bounds);
        }

    }
}
