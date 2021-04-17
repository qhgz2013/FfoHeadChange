using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public enum ServantPart
    {
        Undefined = 0,
        Head = 1,
        Body = 2,
        Background = 3
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private ServantPart selectingPart;
        private int headSvtID = -1, bodySvtID = -1, backgroundSvtID = -1;
        public MainWindow()
        {
            InitializeComponent();
            MessageMgr.OnMessageSent += OutputMsg;
            selectingPart = ServantPart.Undefined;
        }

        public void OutputMsg(object sender, StringEventArgs e)
        {
            Dispatcher.Invoke(new ThreadStart(delegate
            {
                statusOutput.Text = e.Message;
            }));
        }

        private void SelectBody_Click(object sender, RoutedEventArgs e)
        {
            selectingPart = ServantPart.Body;
            iconSelectorPanel.Visibility = Visibility.Visible;
        }

        private void SelectBackground_Click(object sender, RoutedEventArgs e)
        {
            selectingPart = ServantPart.Background;
            iconSelectorPanel.Visibility = Visibility.Visible;
        }

        private void Update()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                int tmpHeadSvtID = headSvtID, tmpBodySvtID = bodySvtID, tmpBackgroundSvtID = backgroundSvtID;
                string svtName, svtDesc;
                ServantFusionMachine.GetFusionServantInfo(tmpHeadSvtID, tmpBodySvtID, tmpBackgroundSvtID, out svtName, out svtDesc);
                var waitEvent = new ManualResetEventSlim();
                Dispatcher.Invoke(new ThreadStart(delegate
                {
                    servantName.Inlines.Clear();
                    servantName.Inlines.Add(svtName);
                    servantInfo.Inlines.Clear();
                    servantInfo.Inlines.Add(svtDesc);
                    waitEvent.Set();
                }));
                // ensure gui is updated correctly
                waitEvent.Wait();
                ServantFusionMachine.GenerateFusionServant(this, imageFusionOutput, tmpHeadSvtID, tmpBodySvtID, tmpBackgroundSvtID);
            });
        }

        private void ClearHead_Click(object sender, RoutedEventArgs e)
        {
            headSelectedIcon.Source = null;
            headSvtID = -1;
            Update();
        }

        private void ClearBody_Click(object sender, RoutedEventArgs e)
        {
            bodySelectedIcon.Source = null;
            bodySvtID = -1;
            Update();
        }

        private void ClearBackground_Click(object sender, RoutedEventArgs e)
        {
            backgroundSelectedIcon.Source = null;
            backgroundSvtID = -1;
            Update();
        }

        private void SelectHead_Click(object sender, RoutedEventArgs e)
        {
            selectingPart = ServantPart.Head;
            iconSelectorPanel.Visibility = Visibility.Visible;
        }

        private void IconSelector_OnServantIconClicked(object sender, ServantIconClickedEventArgs e)
        {
            if (e.ServantIconData != null)
            {
                var src = new BitmapImage();
                src.BeginInit();
                src.StreamSource = new System.IO.MemoryStream(e.ServantIconData);
                src.EndInit();
                switch (selectingPart)
                {
                    case ServantPart.Head:
                        headSelectedIcon.Source = src;
                        headSvtID = e.ServantID;
                        break;
                    case ServantPart.Body:
                        bodySelectedIcon.Source = src;
                        bodySvtID = e.ServantID;
                        break;
                    case ServantPart.Background:
                        backgroundSelectedIcon.Source = src;
                        backgroundSvtID = e.ServantID;
                        break;
                    default:
                        break;
                }
            }
            Update();
        }
    }
}
