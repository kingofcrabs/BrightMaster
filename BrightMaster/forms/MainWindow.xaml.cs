using BrightMaster.data;
using BrightMaster.forms;
using BrightMaster.Settings;
using BrightMaster.utility;
using EngineDll;
using Microsoft.Win32;
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
        ProgressForm progressForm;
        PowerControl powerControl;
        Stopwatch watch = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            myCanvas.PreviewMouseLeftButtonDown += myCanvas_PreviewMouseLeftButtonDown;
            myCanvas.PreviewMouseMove += myCanvas_PreviewMouseMove;
            myCanvas.PreviewMouseLeftButtonUp += myCanvas_PreviewMouseLeftButtonUp;
        }

        async void Initialize()
        {
            GlobalVars.Instance.UAController.onInitialFinished += UAController_onInitialFinished;
            await GlobalVars.Instance.UAController.Initialize();
        }

        void UAController_onInitialFinished(bool bok, string errMsg)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.IsEnabled = true;
                if (bok)
                    SetInfo("初始化成功！", false);
                else
                    SetInfo("初始化失败：" + errMsg, true);
            });
            
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            regionsHistoryPanel.DataContext = GlobalVars.Instance.RegionsHistoryInfoCollection;
            wholePanelHistoryPanel.DataContext = GlobalVars.Instance.WholePanelHistoryInfoCollection;
            SetInfo("初始化，请等待！", false);
            this.IsEnabled = false;
            try
            {
                lblSelectedRecipe.DataContext = GlobalVars.Instance.RecipeCollection;
                powerControl = new PowerControl();
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

        void myCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((bool)btnSetROI.IsChecked)
            { 
                if(myCanvas.IsValidMove)
                {
                    var pts = myCanvas.OnLeftButtonUp();
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
                
                return;
            }

            //myCanvas.Cursor = Cursors.Arrow;
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
        void myCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(myCanvas);
            if ((bool)btnSetROI.IsChecked)
            {
                myCanvas.OnLeftButtonDown(mousePos);
                return;
            }
        }

        void myCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
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
                //scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                //scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveHelper saveHelper = new SaveHelper();
            saveHelper.Save();
            GlobalVars.Instance.UAController.UnInitialize();
        }

        private void FakeColor_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = brightness != null &&  brightness.grayVals.GetLength(0) != 0;
        }
        private void btnSetROI_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.UserSelectROI = (bool)btnSetROI.IsChecked;
            if (myCanvas.UserSelectROI)
                GlobalVars.Instance.MiscSettings.HullPts.Clear();
        }


        void  AdjustLayoutForFakeColor()
        {
            
            bool isFakeColor = (bool)btnFakeColor.IsChecked;
            int columnSpan = isFakeColor ? 1 : 2;
            Grid.SetColumnSpan(myCanvas, columnSpan);
            colorBar.SetMinMax(brightness.Min, brightness.Max);
            colorBar.Visibility = isFakeColor ? Visibility.Visible : Visibility.Collapsed;
            
        }
     
        private void FakeColor_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AdjustLayoutForFakeColor();
            BitmapImage bmpImage;
            if ((bool)btnFakeColor.IsChecked)
            {
                brightness.SaveImage();
                bmpImage = ImageHelper.CreatePseudoColorImage(brightness.ImagePath);
            }
            else
            {
                bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            }
            myCanvas.SetBkGroundImage(bmpImage, null,true);    
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
            e.CanExecute = GlobalVars.Instance.RecipeCollection.SelectedRecipe != null;
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
                GlobalVars.Instance.Barcode = DateTime.Now.ToString("hhmmss");
                var allPixels = GlobalVars.Instance.UAController.LoadXYZImage(fileName);
                this.Dispatcher.Invoke(() =>
                {
                    List<System.Drawing.PointF> pts = new List<System.Drawing.PointF>();
                    List<System.Drawing.PointF> hullPts = new List<System.Drawing.PointF>();
                    FindBoundingPts(allPixels,ref pts,ref hullPts);
                    if (pts == null || pts.Count != 4)
                    {
                        SetInfo("无法找到外框", true);
                    }
                    else
                    {
                        SetInfo("采集完成。", false);
                        UpdateResults(pts,hullPts);
                    }
                   
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
        private void PseduoColorDef_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void PseduoColorDef_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PseudoColorSetting settingForm = new PseudoColorSetting();
            settingForm.ShowDialog();
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
            CommandManager.InvalidateRequerySuggested();
        }

        private void Power_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Power_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool isChecked = (bool)chkPower.IsChecked;
            if(!isChecked)
            {
                powerControl.PowerOff();
            }
            else
            {
                powerControl.PowerOn();
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalVars.Instance.RecipeCollection.SelectedRecipe != null;
        }

        private void Curve_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = brightness != null;
            e.CanExecute =true;
        }
        private void Save2Excel_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalVars.Instance.AnalysisSuccess && SaveHelper.newFilePath != "";
        }

        private void Save2Excel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveHelper saveHelper = new SaveHelper();
            SetInfo("正在保存Excel！", false);
            this.IsEnabled = false;
            this.Refresh();
            try
            {
                saveHelper.Save2Excel(brightness);
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, true);
                this.IsEnabled = true;
                return;
            }
            SetInfo("保存Excel成功！", false);
            this.IsEnabled = true;
        }

        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = GlobalVars.Instance.AnalysisSuccess ;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = FolderHelper.GetDefaultSaveFolder();
            saveFileDialog.Filter = "Excel file (*.xlsx)|*.xlsx";
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveHelper.CreateNewFile(saveFileDialog.FileName);
                SetInfo("新建excel成功！",false);
            }
                
        }

        private void Curve_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            XYCurves curveForm = new XYCurves(brightness);
            curveForm.ShowDialog();
        }


        private void btnRegionMoreInfo_Click(object sender, RoutedEventArgs e)
        {
            HistoryInfoView historyForm = new HistoryInfoView();
            historyForm.ShowDialog();
        }
        private void btnWholePanelMoreInfo_Click(object sender, RoutedEventArgs e)
        {
            WholePanelHistoryInfoView historyForm = new WholePanelHistoryInfoView();
            historyForm.ShowDialog();
        }
        private async void Acquire_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalVars.Instance.NeedBarcode)
            {
                QueryBarcode queryBarcode = new QueryBarcode();
                queryBarcode.ShowDialog();
                
            }
            else
            {
                GlobalVars.Instance.Barcode = DateTime.Now.ToString("hhmmss");
            }
            if (GlobalVars.Instance.Barcode == "")
            {
                SetInfo("条码为空", true);
                return;
            }
            progressForm = new ProgressForm();
            progressForm.Show();
            CheckPowerSetting();
            btnSetROI.IsChecked = false;
            myCanvas.UserSelectROI = false;
            SetInfo("开始采集。", false);
            //btnFakeColor.IsChecked = false;
            this.IsEnabled = false;
            GlobalVars.Instance.AnalysisSuccess = false;
            watch.Reset();
            watch.Start();
            myCanvas.ClearMinMaxPoint();
            await DoAcquisition();
        }
#endregion

        async Task  DoAcquisition()
        {
            await Task.Run(() =>
            {
                watch.Restart();
                if(brightness != null)
                {
                    brightness.ClearMinMaxPosition();
                    brightness.ClearGrayImage();
                }
                    
                var allPixels = GlobalVars.Instance.UAController.Acquire();

                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        List<System.Drawing.PointF> hullPts = new List<System.Drawing.PointF>();
                        List<System.Drawing.PointF> pts = new List<System.Drawing.PointF>();
                        FindBoundingPts(allPixels,ref pts, ref hullPts);
                        watch.Stop();
                        string elapsed = (watch.ElapsedMilliseconds/1000).ToString();
                        if (pts== null || pts.Count != 4)
                        {
                            SetInfo(string.Format("无法找到外框，用时:{0}秒", elapsed), false);
                        }
                        else
                        {
                            SetInfo(string.Format("采集完成，用时:{0}秒",elapsed), false);
                            UpdateResults(pts,hullPts);
                        }
                        this.IsEnabled = true;
                        progressForm.Close();
                        InvalidateVisual();
                    });
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        SetInfo("采集失败："+ex.Message, true);
                        if(progressForm.Visible)
                            progressForm.Close();
                        this.IsEnabled = true;
                        return;
                    });
                    
                }
              
            });
        }

        private void FindBoundingPts(LightPixelInfo[,] allPixels,ref List<System.Drawing.PointF> pts,ref List<System.Drawing.PointF> hullPts)
        {
            brightness = new Brightness(allPixels);
            var bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            bool isFakeColor = (bool)btnFakeColor.IsChecked;
            if (isFakeColor)
            {
                AdjustLayoutForFakeColor();
                brightness.SaveImage();
                bmpImage = ImageHelper.CreatePseudoColorImage(brightness.ImagePath);
            }
            if((bool)btnSetROI.IsChecked)
            {
                pts = GlobalVars.Instance.MiscSettings.BoundaryPts;
                myCanvas.SetBkGroundImage(bmpImage, pts);
                return;
            }

            if(!GlobalVars.Instance.MiscSettings.AutoFindBoundary)
            {
                pts = GlobalVars.Instance.MiscSettings.BoundaryPts;
                myCanvas.SetBkGroundImage(bmpImage, pts);
                return;
            }
            
            if(!isFakeColor) //we save image for auto find boundary
            {
                brightness.SaveImage();
            }

            
            List<MPoint> hullMPts = new List<MPoint>();
            IEngine iEngine = new IEngine();
            bool first = GlobalVars.Instance.IsFirst;
            string savePath = first ? FolderHelper.GetImageFolder() + "first.jpg" : "";


            var mpts = iEngine.FindRect(brightness.ImagePath,
                ref GlobalVars.Instance.MiscSettings.thresholdVal, 
                GlobalVars.Instance.MiscSettings.MannualThreshold,
                hullMPts,savePath);
            if (mpts.Count == 0)
                return;
            if (mpts.Count != 4)
            {
                throw new Exception("找不到外框！");
            }
            pts = Layout.SortPosition(mpts);
            foreach(var mpt in hullMPts)
            {
                hullPts.Add(new System.Drawing.Point(mpt.x, mpt.y));
            }
            GlobalVars.Instance.MiscSettings.BoundaryPts = pts;
            GlobalVars.Instance.MiscSettings.HullPts = hullPts;
            GlobalVars.Instance.MiscSettings.Save();
            myCanvas.SetBkGroundImage(bmpImage, pts);
        }

        private void UpdateResults(List<System.Drawing.PointF> pts,List<System.Drawing.PointF> hullPts = null)
        {
            if (pts == null || pts.Count == 0)
                return;

            if (hullPts == null || hullPts.Count == 0)
                brightness.UpdateROI(pts);
            else
                brightness.UpdateConvexHull(hullPts);
                
            myCanvas.SetMaxMinPosition(brightness.MaxPosition, brightness.MinPosition);
            if(GlobalVars.Instance.AnalysisRegions)
                pts = Layout.Convert2ROI(pts);
            var pixelInfos = brightness.GetPixelInfos(pts);
            lstviewResult.ItemsSource = pixelInfos;
            PixelInfo.Save2File(pixelInfos);
            var regionResult = TestResult.GetRegionResult(pixelInfos);
            var wholePanelResult = TestResult.GetWholePanelResult(brightness);
            regionsResultPanel.DataContext = regionResult;
            wholePanelResultPanel.DataContext = wholePanelResult;
            if (!regionResult.IsOk || !wholePanelResult.IsOk)
            {
                PlaySound.OnError();
            }
            GlobalVars.Instance.RegionsHistoryInfoCollection.AddNew(new HistoryInfo(GlobalVars.Instance.Barcode, regionResult));
            GlobalVars.Instance.WholePanelHistoryInfoCollection.AddNew(new HistoryInfo(GlobalVars.Instance.Barcode, wholePanelResult));
            SaveHelper saveHelper = new SaveHelper();
            saveHelper.Save();
            GlobalVars.Instance.AnalysisSuccess = true;
            
        }

       

        private void CheckPowerSetting()
        {
            bool isChecked = (bool)chkPower.IsChecked;
            if(!isChecked)
            {
                chkPower.IsChecked = true;
            }
            powerControl.PowerOn();
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
                        string sID = ID.ToString();
                        ID++;
                        linePixelInfos.Add(new PixelInfo(sID, 0, 0, 0));
                    }
                    else
                    {
                        float tVal = 800 + rnd.Next(1,200);
                        string sID = ID.ToString();
                        ID++;
                        linePixelInfos.Add(new PixelInfo(sID, tVal, tVal, tVal));
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
