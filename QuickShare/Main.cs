using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickShare
{
    public partial class Main : Form
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }


        public Main()
        {
            InitializeComponent();
            notifyIcon1.Visible = true;
            textBox1.Text = Properties.Settings.Default["folderPath"].ToString();

            int id = 2;     // The id of the hotkey. 
            RegisterHotKey(this.Handle, id, (int)KeyModifier.Shift, Keys.F4.GetHashCode());
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            
            if (FormWindowState.Minimized == this.WindowState)
            {

                ShowBalloonAndUpdate(notifyIcon1, 500, "Minimized", "Press Shift+F4 to take a screenshot.");
                this.Hide();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {   
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            DialogResult result = fbd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textBox1.Text = fbd.SelectedPath;
                Properties.Settings.Default["folderPath"] = fbd.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                try
                {
                    Bitmap bitmap = ScreenCapturer.Capture(enmScreenCaptureMode.Window);
                    Properties.Settings.Default["folderPath"] = textBox1.Text;
                    Properties.Settings.Default.Save();

                    String folderPath = textBox1.Text + "\\";
                    String imageName = bitmap.GetHashCode() + RandomString(4) + ".jpeg";
                    

                    while (File.Exists(folderPath + imageName))
                    {
                        imageName = bitmap.GetHashCode() + RandomString(4) + ".jpeg";
                    }

                    bitmap.Save(folderPath + imageName, ImageFormat.Jpeg);
                    ShowBalloonAndUpdate(notifyIcon1, 500, "Screenshot", folderPath + imageName);
                }
                catch (Exception)
                {
                    ShowBalloonAndUpdate(notifyIcon1, 500, "Error", "Invalid folder path...");
                }

            }
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            NotifyIcon ni = sender as NotifyIcon;

            try
            {
                Process.Start(@ni.BalloonTipText);
            }
            catch (Exception)
            {
                
            }

        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void ShowBalloonAndUpdate(NotifyIcon ni, int timeout, string title, string text)
        {
            ni.BalloonTipTitle = title;
            ni.BalloonTipText = text;
            ni.ShowBalloonTip(timeout);
        }

        
    }
}
