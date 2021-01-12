using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ConvertVideoFilesToMP4UsingVLC
{
    /// <summary>
    /// Convert Video Files To MP4 Using VLC
    /// </summary>
    public partial class MainWindow : Window
    {
        const string appName = "ConvertVideoFilesToMP4UsingVLC";
        const string appVer = "0.0.1";
        private string vlcPath = "C:\\Program Files\\VideoLAN\\VLC\\vlc.exe";
        private string vlcPathDoubleQuoted = "\"C:\\Program Files\\VideoLAN\\VLC\\vlc.exe\"";
        private string[] files;
        private bool isBusy;

        public MainWindow()
        {
            InitializeComponent();

            Title = appName + " v" + appVer;

            consoleTextBox.AppendText("#########################################################################################" + Environment.NewLine);
            consoleTextBox.AppendText(string.Format("{0} - v{1}", appName, appVer) + Environment.NewLine);
            consoleTextBox.AppendText(Environment.NewLine);
            consoleTextBox.AppendText("It's a fake! Yes, this is not a console (commandline) application. It's a GUI video batch converter." + Environment.NewLine);
            consoleTextBox.AppendText("This application takes video files such as AVI and converts to MP4 using VLC." + Environment.NewLine);
            consoleTextBox.AppendText(Environment.NewLine);
            consoleTextBox.AppendText("Usage:" + Environment.NewLine);
            consoleTextBox.AppendText("  (a) Drag and Drop video files here" + Environment.NewLine);
            consoleTextBox.AppendText("  (b) Create a shortcut file of this exe file and place it to \"SendTo\" folder." + Environment.NewLine);
            consoleTextBox.AppendText("      You can just type \"shell:sendto\" in the Windows Explorer's address bar to open the foloder." + Environment.NewLine);
            consoleTextBox.AppendText("#########################################################################################" + Environment.NewLine);
            consoleTextBox.AppendText(Environment.NewLine + Environment.NewLine);

            if (System.IO.File.Exists(vlcPath) == false)
            {
                if (System.IO.File.Exists(@"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe") == true)
                {
                    vlcPath = @"C:\Program Files (x86)\VideoLAN\VLC\vlc.exe";
                    vlcPathDoubleQuoted = "\"C:\\C:\\Program Files (x86)\\VideoLAN\\VLC\\vlc.exe\"";
                }
                else
                {
                    statusBarMessage.Text = "No VLC?";

                    statusBarErrorMessage.Text = string.Format("VLC could not be found at {0}", vlcPath);

                    consoleTextBox.AppendText(string.Format("Error: VLC could not be found at {0}", vlcPath) + Environment.NewLine);

                    vlcPath = "";
                }
            }

            if (vlcPath != "")
            {
                files = Environment.GetCommandLineArgs();

                if (files.Length > 1)
                {
                    isBusy = true;

                    var list = new List<string>(files);
                    list.Remove(list[0]);
                    files = list.ToArray();

                    Start();
                }
                else
                {
                    statusBarMessage.Text = "Ready";
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (isBusy)
                return;

            isBusy = true;

            files = e.Data.GetData(DataFormats.FileDrop) as string[];

            Start();
        }

        private void Window_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private async void Start()
        {
            await Task.Run(() => ProcessFiles());
        }

        private void ProcessFiles()
        {
            if (files == null)
                return;
            if (System.IO.File.Exists(vlcPath) == false)
                return;

            SetStatusMessageText("Working...");

            SetTextBoxText(string.Format("Received following {0} file(s).", files.Length) + Environment.NewLine);

            foreach (var hoge in files)
            {
                if (System.IO.File.Exists(hoge) == false)
                {
                    SetStatusErrorMessageText(string.Format("File {0} does not exists. Skipping.", System.IO.Path.GetFileName(hoge)));

                    continue;
                }

                SetTextBoxText("   " + hoge + Environment.NewLine);
            }

            foreach (var filePath in files)
            {
                if (System.IO.File.Exists(filePath) == false)
                    continue;

                string newFilePath = System.IO.Path.ChangeExtension(filePath, ".mp4");

                SetTextBoxText(string.Format(Environment.NewLine + "Processing {0}...", System.IO.Path.GetFileName(filePath)) + Environment.NewLine + Environment.NewLine);

                try
                {
                    var vlc = new System.Diagnostics.ProcessStartInfo();
                    vlc.Arguments = vlcPathDoubleQuoted + " \"" + filePath + "\" " + "--sout=\"#transcode{acodec=mp4a,ab=128,channels=2,samplerate=44100,vcodec=h264,deinterlace}:duplicate{dst=std{access=file,mux=mp4,dst=" + newFilePath + "}}\"" + " " + "--play-and-exit\"";
                    vlc.FileName = "vlc";
                    vlc.UseShellExecute = true;

                    SetTextBoxText(string.Format("Executing... {0}", vlc.Arguments) + Environment.NewLine + Environment.NewLine);

                    var process = System.Diagnostics.Process.Start(vlc);

                    process.WaitForExit();

                    SetTextBoxText(string.Format("Finished 1 file: {0}", newFilePath) + Environment.NewLine + Environment.NewLine);
                }
                catch (Exception e) 
                {
                    SetStatusErrorMessageText(string.Format("An application error occured: {0}", e.Message));

                    break;
                }
            }

            SetTextBoxText("All done." + Environment.NewLine + Environment.NewLine);

            SetStatusMessageText("Done");

            isBusy = false;
        }

        private void SetTextBoxText(string txt)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    consoleTextBox.AppendText(txt);
                });
            }
        }

        private void SetStatusMessageText(string txt)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    statusBarMessage.Text = txt;
                });
            }
        }

        private void SetStatusErrorMessageText(string txt)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    statusBarErrorMessage.Text = txt;
                });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isBusy)
            {
                MessageBoxResult result = MessageBox.Show("VLC is still processing." + Environment.NewLine + "Do you wish to close this window now?",
                                          "Confirmation",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;

                    return;
                }
            }
        }
    }
}
