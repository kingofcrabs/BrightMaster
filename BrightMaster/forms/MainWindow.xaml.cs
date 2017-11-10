using BrightMaster.settings;
using BrightMaster.Settings;
using EngineDll;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BrightMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UAContorller uaController = null;
        Brightness brightness = null;
        ObservableCollection<PixelInfo> pixelInfos = new ObservableCollection<PixelInfo>();
        double zoomRatio = 1;
        Point? lastDragPoint;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            scrollViewer.PreviewMouseWheel += scrollViewer_PreviewMouseWheel;
            scrollViewer.PreviewMouseLeftButtonDown += scrollViewer_PreviewMouseLeftButtonDown;
            scrollViewer.PreviewMouseMove += scrollViewer_PreviewMouseMove;
            scrollViewer.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
        }

       

        async void Initialize()
        {
            await GlobalVars.Instance.UAController.Initialize();
            uaController = new UAContorller();
            this.IsEnabled = true;
            SetInfo("初始化成功！", false);
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            SetInfo("初始化，请等待！", false);
            this.IsEnabled = false;
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                SetInfo("初始化失败，原因是：" + ex.Message, true);
            }
            cmbRecipes.DataContext = GlobalVars.Instance.RecipeCollection;
        }

        private void Zoom()
        {
            ScaleTransform scaler = new ScaleTransform(zoomRatio, zoomRatio);
            myCanvas.LayoutTransform = scaler;
        }

        private void SetInfo(string str, bool error)
        {
            txtInfo.Text = str;
            txtInfo.Foreground = error ? Brushes.Red : Brushes.Black;
        }

       
        #region commands & events

        void scrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((bool)btnSetROI.IsChecked)
            { 
                if(myCanvas.IsValidMove)
                {
                    var pts = myCanvas.GeneratePtsImageCoord();
                    var results = brightness.GetResults(pts);
                    lstviewResult.ItemsSource = results;
                }
                myCanvas.OnLeftButtonUp();
                return;
            }

            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }
     

        void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(myCanvas);
            if ((bool)btnSetROI.IsChecked)
            {
                myCanvas.OnLeftButtonDown(mousePos);
                return;
            }

            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y <
                scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        void scrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point posNow = e.GetPosition(myCanvas);
            if ((bool)btnSetROI.IsChecked && myCanvas.IsValidMove)
            {
                myCanvas.OnLeftButtonMove(posNow);
                return;
            }

            if (lastDragPoint.HasValue)
            {
                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        private void scrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                zoomRatio *= 1.2;
            }
            else
            {
                zoomRatio /= 1.2;
            }
            Zoom();
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalVars.Instance.UAController.UnInitialize();
        }

        private void FakeColor_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = brightness != null &&  brightness.grayVals.Count != 0;
        }
        private void btnSetROI_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.UserSelectROI = (bool)btnSetROI.IsChecked;
        }
        private void FakeColor_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BitmapImage bmpImage;
            if ((bool)btnFakeColor.IsChecked)
            {
                bmpImage = ImageHelper.CreateImage(brightness.jpgFilePath);
            }
            else
            {
                bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            }
            myCanvas.SetBkGroundImage(bmpImage,null,true);            
        }

        private void RecipeDef_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RecipeDefinitionWindow recipeDefWindow = new RecipeDefinitionWindow();
            recipeDefWindow.ShowDialog();
        }

        private void RecipeDef_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

       
        private void Acquire_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = cmbRecipes.SelectedItem != null;
        }
#endregion

        async Task  DoAcquisition()
        {
            
            await Task.Run(() =>
            {

                List<List<PixelInfo>> allPixels = GlobalVars.Instance.UAController.Acquire();
                try
                {
                    brightness = new Brightness(allPixels);
                    this.Dispatcher.Invoke(() =>
                    {
                        var bmpImage = ImageHelper.CreateImage(brightness.grayVals);
                        IEngine iEngine = new IEngine();
                        string sImgFile = FolderHelper.GetImageFolder() + "latest.jpg";

                        ImageHelper.SaveBitmapImageIntoFile(bmpImage, sImgFile);
                        var pts = iEngine.FindRect(sImgFile);
                        myCanvas.SetBkGroundImage(bmpImage, pts);
                        var results = brightness.GetResults(pts);
                        lstviewResult.ItemsSource = results;
                    });
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        SetInfo("采集失败："+ex.Message, true);
                        return;
                    });
                    
                }
                this.Dispatcher.Invoke(() =>
                {
                    SetInfo("采集完成。", false);
                    this.IsEnabled = true;
                    InvalidateVisual();
                });
            });
        }

        private  async void Acquire_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetInfo("开始采集。",false);
            btnFakeColor.IsChecked = false;
            this.IsEnabled = false;
            await DoAcquisition();
        }

    

      

        private List<List<PixelInfo>> GenerateTestData()
        {
            List<List<PixelInfo>> allPixelInfos = new List<List<PixelInfo>>();
            Random rnd = new Random(23771);
            int ID = 1;
            for(int y = 0; y < 400; y++)
            {
                List<PixelInfo> linePixelInfos = new List<PixelInfo>();
                for(int x = 0; x < 400; x++)
                {
                    if (x < 50 || y < 50 || x > 250 || y > 250)
                    {
                        linePixelInfos.Add(new PixelInfo(ID++,0, 0, 0));
                    }
                    else
                    {
                        float tVal = 800 + rnd.Next(1,200);
                        linePixelInfos.Add(new PixelInfo(ID++,tVal, tVal, tVal));
                    }
                        
                    
                }
                allPixelInfos.Add(linePixelInfos);
            }
            return allPixelInfos;
            
        }

        private void LiveFocus_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void LiveFocus_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LiveFocusView livewFocusView = new LiveFocusView();
            livewFocusView.ShowDialog();
        }

        private void CommandHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void CommandHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        

       

       
    }

    public static class ExtensionMethods
    {
        private static Action EmptyDelegate = delegate() { };

        public static void Refresh(this UIElement uiElement)
        {

            uiElement.Dispatcher.Invoke(DispatcherPriority.ContextIdle, EmptyDelegate);

        }

    }
}
