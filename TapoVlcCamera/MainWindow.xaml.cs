using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using static TapoCamera.BaseLogRecord;
using System.Windows.Media.Media3D;


namespace TapoCamera
{
    #region Config Class
    public class SerialNumber
    {
        [JsonProperty("Parameter1_val")]
        public string Parameter1_val { get; set; }
        [JsonProperty("Parameter2_val")]
        public string Parameter2_val { get; set; }
    }

    public class Model
    {
        [JsonProperty("SerialNumbers")]
        public SerialNumber SerialNumbers { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("Models")]
        public List<Model> Models { get; set; }
    }
    #endregion

    public partial class MainWindow : System.Windows.Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        #region Config
        private SerialNumber SerialNumberClass()
        {
            SerialNumber serialnumber_ = new SerialNumber
            {
                Parameter1_val = Parameter1.Text,
                Parameter2_val = Parameter2.Text
            };
            return serialnumber_;
        }

        private void LoadConfig(int model, int serialnumber, bool encryption = false)
        {
            List<RootObject> Parameter_info = Config.Load(encryption);
            if (Parameter_info != null)
            {
                Parameter1.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter1_val;
                Parameter2.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Parameter2_val;
            }
            else
            {
                // 結構:2個Models、Models下在各2個SerialNumbers
                SerialNumber serialnumber_ = SerialNumberClass();
                List<Model> models = new List<Model>
                {
                    new Model { SerialNumbers = serialnumber_ },
                    new Model { SerialNumbers = serialnumber_ }
                };
                List<RootObject> rootObjects = new List<RootObject>
                {
                    new RootObject { Models = models },
                    new RootObject { Models = models }
                };
                Config.SaveInit(rootObjects, encryption);
            }
        }
       
        private void SaveConfig(int model, int serialnumber, bool encryption = false)
        {
            Config.Save(model, serialnumber, SerialNumberClass(), encryption);
        }
        #endregion

        #region Dispatcher Invoke 
        public string DispatcherGetValue(TextBox control)
        {
            string content = "";
            this.Dispatcher.Invoke(() =>
            {
                content = control.Text;
            });
            return content;
        }

        public void DispatcherSetValue(string content, TextBox control)
        {
            this.Dispatcher.Invoke(() =>
            {
                control.Text = content;
            });
        }
        #endregion

        public void ContinueAcquisition()
        {
            _libVlcDirectory = new DirectoryInfo(@"D:\vlc-3.0.21");
            // 初始化 VLC 控制
            vlcControl.SourceProvider.CreatePlayer(_libVlcDirectory);
            // 設定 RTSP 串流 URL
            string rtspUrl = "rtsp://admin0930:Asher19910930@192.168.0.130/stream2";
            //// Set the path where the video will be recorded
            //string outputFilePath = @"C:\Users\user\source\repos\WpfApp1\WpfApp1\bin\Debug\video.mp4";
            //// Set VLC options to record the stream to the specified file
            //var mediaOptions = new string[] {
            //     $":sout=#file{{dst={outputFilePath}}}"
            //};
            //// Start playing and recording the stream to the local file
            //vlcControl.SourceProvider.MediaPlayer.Play(new Uri(rtspUrl), mediaOptions);
            vlcControl.SourceProvider.MediaPlayer.Play(new Uri(rtspUrl));
        }

        public void GrabImage()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "\\snapshot\\";
            string imgName = DateTime.Now.Ticks + ".png";
            string filePath = dirPath + imgName;
            vlcControl.SourceProvider.MediaPlayer.TakeSnapshot(0, filePath, (uint)vlcControl.Width, (uint)vlcControl.Height);
        }
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig(0, 0);
        }
        BaseConfig<RootObject> Config = new BaseConfig<RootObject>();
        private DirectoryInfo _libVlcDirectory;
        #region Log
        BaseLogRecord Logger = new BaseLogRecord();
        //Logger.WriteLog("儲存參數!", LogLevel.General, richTextBoxGeneral);
        #endregion
        #endregion

        #region Main Screen
        private void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Continue_Acquisition):
                    {
                        ContinueAcquisition();
                        break;
                    }
                case nameof(Grab_Image):
                    {
                        GrabImage();
                        break;
                    }
                case nameof(Pause):
                    {
                        vlcControl.SourceProvider.MediaPlayer.Pause();
                        break;
                    }
                case nameof(Continue):
                    {
                        vlcControl.SourceProvider.MediaPlayer.Play();
                        break;
                    }
            }
        }
        #endregion
        

    }
}
