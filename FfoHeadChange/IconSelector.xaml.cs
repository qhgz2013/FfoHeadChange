using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FfoHeadChange
{
    /// <summary>
    /// IconSelector.xaml 的交互逻辑
    /// </summary>
    public partial class IconSelector : UserControl
    {
        private static ManualResetEventSlim backgroundLoaded = new ManualResetEventSlim();
        private static Mutex mutex = new Mutex();
        private static volatile int initializedResult = -1;
        private static Dictionary<int, byte[]> iconCache = new Dictionary<int, byte[]>();
        
        private void InitializerCallback(object state)
        {
            if (backgroundLoaded.IsSet)
                return;
            using (mutex)
            {
                try
                {
                    MessageMgr.SendMessage(this, "Initializing servant icon");
                    var iconInfo = new DirectoryInfo(StartupConfig.ICON_DIRECTORY);
                    foreach (var file in iconInfo.GetFiles())
                    {
                        var iconPattern = Regex.Match(file.Name, StartupConfig.ICON_PARSE_PATTERN);
                        if (iconPattern.Success)
                        {
                            var svtID = int.Parse(iconPattern.Groups[1].Value);
                            if (svtID == 999)
                                continue; // skip this
                            var stream = file.OpenRead();
                            var data = new byte[stream.Length];
                            stream.Read(data, 0, data.Length);
                            stream.Close();
                            var memoryStream = new MemoryStream(data);
                            iconCache.Add(svtID, data);
                            Dispatcher.Invoke(new ThreadStart(delegate
                            {
                                // creating images ui element (make sure this code is running at main thread)
                                var img = new Image();
                                var bmp = new BitmapImage();
                                bmp.BeginInit();
                                bmp.StreamSource = memoryStream;
                                bmp.EndInit();
                                img.Source = bmp;
                                img.Width = 60;
                                img.Height = 60;
                                img.Tag = svtID;
                                img.Cursor = Cursors.Hand;
                                img.MouseLeftButtonDown += Img_MouseLeftButtonDown;
                                iconPanel.Children.Add(img);
                            }));
                        }
                    }
                    MessageMgr.SendMessage(this, "Initialized servant icon successfully");
                    initializedResult = 0;
                }
                catch (Exception ex)
                {
                    MessageMgr.SendMessage(this, "Initialize servant icon failed: " + ex.ToString());
                    initializedResult = 1;
                }
                finally
                {
                    backgroundLoaded.Set();
                }
            }
        }

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // a stupid yet efficient way to avoid dead lock
            if (!backgroundLoaded.IsSet)
                return;
            var selectedServantID = (int)((Image)sender).Tag;
            if (!iconCache.TryGetValue(selectedServantID, out var selectedServantIconData))
            {
                MessageMgr.SendMessage(this, "Failed to get icon of servant" + selectedServantID);
            }
            else
            {
                OnServantIconClicked?.Invoke(this, new ServantIconClickedEventArgs(selectedServantID, selectedServantIconData));
            }
            Visibility = Visibility.Hidden;
        }
        
        public IconSelector()
        {
            InitializeComponent();
        }

        public event EventHandler<ServantIconClickedEventArgs> OnServantIconClicked;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(InitializerCallback));
        }
    }

    public class ServantIconClickedEventArgs: EventArgs
    {
        public int ServantID { get; private set; }
        public byte[] ServantIconData { get; private set; }
        public ServantIconClickedEventArgs(int servantId, byte[] iconData)
        {
            ServantID = servantId;
            ServantIconData = iconData;
        }
    }
}
