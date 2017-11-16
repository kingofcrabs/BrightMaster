using BrightMaster.data;
using BrightMaster.forms;
using BrightMaster.Settings;
using BrightMaster.Settings;
using EngineDll;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
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
        Brightness brightness = null;
        ObservableCollection<PixelInfo> pixelInfos = new ObservableCollection<PixelInfo>();
        double zoomRatio = 1;
        Point? lastDragPoint;
        static SerialPort serialPort = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            scrollViewer.PreviewMouseWheel += scrollViewer_PreviewMouseWheel;
            scrollViewer.PreviewMouseLeftButtonDown += scrollViewer_PreviewMouseLeftButtonDown;
            scrollViewer.PreviewMouseMove += scrollViewer_PreviewMouseMove;
            scrollViewer.PreviewMouseLeftButtonUp += scrollViewer_PreviewMouseLeftButtonUp;
            scrollViewer.ScrollChanged += scrollViewer_ScrollChanged;
            
        }

        void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            myCanvas.InvalidateVisual();
        }

       

        async void Initialize()
        {
            await GlobalVars.Instance.UAController.Initialize();
            this.IsEnabled = true;
            
            SetInfo("初始化成功！", false);
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            historyPanel.DataContext = GlobalVars.Instance.HistoryInfos;
            SetInfo("初始化，请等待！", false);
            this.IsEnabled = false;
            try
            {
                cmbRecipes.DataContext = GlobalVars.Instance.RecipeCollection;
                string portName = ConfigurationManager.AppSettings["ComPort"];
                if(portName != "")
                {
                    serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                    serialPort.Open();
                }
                Initialize();
            }
            catch (Exception ex)
            {
                SetInfo("初始化失败，原因是：" + ex.Message, true);
                this.IsEnabled = true;
            }
            
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
                    try
                    {
                        UpdateResults(pts);
                    }
                    catch(Exception ex)
                    {
                        SetInfo(ex.Message,true);
                    }
                    SetInfo("", false);
                }
                myCanvas.OnLeftButtonUp();
                return;
            }

            scrollViewer.Cursor = Cursors.Arrow;
            //scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        private parentItem FindVisualParent<parentItem>(DependencyObject obj) where parentItem : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && !parent.GetType().Equals(typeof(parentItem)))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as parentItem;
        }
        void scrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(myCanvas);
            object original = e.OriginalSource;

            if (!original.GetType().Equals(typeof(ScrollViewer)))
            {
                if (FindVisualParent<System.Windows.Controls.Primitives.ScrollBar>(original as DependencyObject) != null)
                {
                    return;
                }

            }


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
                //Mouse.Capture(scrollViewer);
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
            if(serialPort != null && serialPort.IsOpen)
                serialPort.Close();
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
            bool isFakeColor = (bool)btnFakeColor.IsChecked;
            int columnSpan = isFakeColor ? 1 : 2;
            Grid.SetColumnSpan(scrollViewer, columnSpan);
            colorBar.SetMinMax(brightness.Min, brightness.Max);
            colorBar.Visibility = isFakeColor ? Visibility.Visible : Visibility.Collapsed;
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

        private void LiveFocus_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void LiveFocus_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LiveFocusView livewFocusView = new LiveFocusView();
            livewFocusView.ShowDialog();
        }

     
        private void MiscDef_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MiscDef_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MiscForm saveFolderForm = new MiscForm();
            saveFolderForm.ShowDialog();
        }


        async Task  OpenFile(string fileName)
        {
            await Task.Run(() =>
            {
                var allPixels = GlobalVars.Instance.UAController.LoadXYZImage(fileName);
                this.Dispatcher.Invoke(() =>
                {
                    List<System.Drawing.Point> pts = FindBoundingPts(allPixels);
                    if (pts == null || pts.Count != 4)
                    {
                        SetInfo("无法找到外框", true);
                    }
                    else
                    {
                        SetInfo("采集完成。", false);
                        UpdateResults(pts);
                    }
                    InvalidateVisual();
                });
            });
        }
     


        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void Help_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "msr";
                saveFileDialog.InitialDirectory = FolderHelper.GetDefaultSaveFolder();
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    this.IsEnabled = false;
                    GlobalVars.Instance.UAController.SaveXYZImage(fileName);
                }
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message, true);
                this.IsEnabled = true;
                return;
            }
            this.IsEnabled = true;
            SetInfo("保存成功", false);
        }
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalVars.Instance.UAController.HaveXYZImage;
        }
        private async void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.InitialDirectory = FolderHelper.GetDefaultSaveFolder();
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = openFileDialog.FileName;
                    this.IsEnabled = false;
                    await OpenFile(fileName);
                }
            }
            catch (Exception ex)
            {
                SetInfo(ex.Message, true);
                this.IsEnabled = true;
                return;
            }
            this.IsEnabled = true;
            SetInfo("打开成功", false);
        }



        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

     

        private void Power_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Power_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool open = (bool)btnPower.IsChecked;
            char val = open ? 'A' : 'a';
            byte[] buffer = new byte[1]{ (byte)val};
            serialPort.Write(buffer, 0, buffer.Length);
        }

#endregion

        async Task  DoAcquisition()
        {
            
            await Task.Run(() =>
            {

                List<List<PixelInfo>> allPixels = GlobalVars.Instance.UAController.Acquire();
                try
                {
                    
                    this.Dispatcher.Invoke(() =>
                    {
                        List<System.Drawing.Point> pts = FindBoundingPts(allPixels);
                        if (pts== null || pts.Count != 4)
                        {
                            SetInfo("无法找到外框", true);
                        }
                        else
                        {
                            SetInfo("采集完成。", false);
                            UpdateResults(pts);
                        }
                        this.IsEnabled = true;
                        InvalidateVisual();
                    });
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        SetInfo("采集失败："+ex.Message, true);
                        this.IsEnabled = true;
                        return;
                    });
                    
                }
              
            });
        }

        private List<System.Drawing.Point> FindBoundingPts(List<List<PixelInfo>> allPixels)
        {
            brightness = new Brightness(allPixels);
            var bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            IEngine iEngine = new IEngine();
            string sImgFile = FolderHelper.GetImageFolder() + "latest.jpg";

            ImageHelper.SaveBitmapImageIntoFile(bmpImage, sImgFile);
            var mpts = iEngine.FindRect(sImgFile, ref GlobalVars.Instance.MiscSettings.thresholdVal, GlobalVars.Instance.MiscSettings.AutoFindBoundary);
            if (mpts.Count == 0)
                return new List<System.Drawing.Point>();
            List<System.Drawing.Point> pts = AdjustPosition(mpts);
            GlobalVars.Instance.MiscSettings.Save();
            myCanvas.SetBkGroundImage(bmpImage, pts);
            return pts;
        }

        private void UpdateResults(List<System.Drawing.Point> pts)
        {
            var results = brightness.GetResults(pts);
            lstviewResult.ItemsSource = results;
            PixelInfo.Save2File(results);
            var result = PixelInfo.GetResult(results);
            resultPanel.DataContext = result;
            GlobalVars.Instance.HistoryInfos.AddNew(new HistoryInfo(GlobalVars.Instance.Barcode, result.IsOk));
            
        }

        private List<System.Drawing.Point> AdjustPosition(List<MPoint> mpts)
        {
            List<System.Drawing.Point> pts = new List<System.Drawing.Point>();
            int avgX = mpts.Sum(pt => pt.x) / 4;
            int avgY = mpts.Sum(pt => pt.y) / 4;
            Point ptMassCenter = new Point(avgX, avgY);
            MPoint topLeft = mpts.Where(pt => pt.x < avgX && pt.y < avgY).First();
            MPoint topRight = mpts.Where(pt => pt.x > avgX && pt.y < avgY).First();
            MPoint bottomRight = mpts.Where(pt => pt.x > avgX && pt.y > avgY).First();
            MPoint bottomLeft = mpts.Where(pt => pt.x < avgX && pt.y > avgY).First();
            pts.Add(new System.Drawing.Point(topLeft.x, topLeft.y));
            pts.Add(new System.Drawing.Point(topRight.x, topRight.y));
            pts.Add(new System.Drawing.Point(bottomRight.x, bottomRight.y));
            pts.Add(new System.Drawing.Point(bottomLeft.x, bottomLeft.y));
            pts = Layout.Convert2ROI(pts);
            
            return pts;
        }

      

        private  async void Acquire_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(GlobalVars.Instance.NeedBarcode)
            {
                QueryBarcode queryBarcode = new QueryBarcode();
                queryBarcode.ShowDialog();
            }
            btnSetROI.IsChecked = false;
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

        private void SetColumnsVisibility(object sender, RoutedEventArgs e)
        {
            ColumnVisibilityForm columnVisibilityForm = new ColumnVisibilityForm();
            columnVisibilityForm.ShowDialog();
            for (int i = 0; i < gridView1.Columns.Count; i++)
                gridView1.Columns[i].Width = GlobalVars.Instance.GridColumnWidth[i];
            InvalidateVisual();

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
