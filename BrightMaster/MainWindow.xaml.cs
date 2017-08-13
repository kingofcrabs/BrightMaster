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
        ObservableCollection<string> layoutFiles = new ObservableCollection<string>();
        double zoomRatio = 1;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
            this.PreviewMouseWheel +=MainWindow_PreviewMouseWheel;
        }

        private void MainWindow_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
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

        async Task Initialize()
        {
            await GlobalVars.Instance.UAController.Initialize();
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetInfo("初始化，请等待！",false);
            this.IsEnabled = false;
            try
            {
                GlobalVars.Instance.UAController.Initialize();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, true);
            }
            
            EnumLayouts();
            uaController = new UAContorller();
            this.IsEnabled = true;
        }

        private void Zoom()
        {
            ScaleTransform scaler = new ScaleTransform(zoomRatio, zoomRatio);
            containerGrid.LayoutTransform = scaler;
        }

        private void SetInfo(string str, bool error)
        {
            txtInfo.Text = str;
            txtInfo.Foreground = error ? Brushes.Red : Brushes.Black;
        }

        private void EnumLayouts()
        {
            layoutFiles.Clear();
            string layoutFolder = FolderHelper.GetLayoutFolder();
            DirectoryInfo dirInfo = new DirectoryInfo(layoutFolder);
            var files = dirInfo.EnumerateFiles("*.xml").ToList();
            files.ForEach(x => layoutFiles.Add(x.Name.Replace(".xml", "")));
            cmbLayouts.ItemsSource = layoutFiles;
        }

        private void FakeColor_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = brightness != null &&  brightness.grayVals.Count != 0;
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
            myCanvas.SetBkGroundImage(bmpImage);            
        }

        private void LayoutDef_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void LayoutDef_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            LayoutDefineWindow layoutDefWin = new LayoutDefineWindow();
            layoutDefWin.ShowDialog();
        }

        private void Acquire_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool bok = true;
            if(cmbLayouts.SelectedItem == null)
            {
                bok = false;
            }
            else
            {
                string file = FolderHelper.GetLayoutFolder() + cmbLayouts.SelectedItem.ToString() + ".xml";
                if(!File.Exists(file))
                    bok = false;
                else
                {
                    GlobalVars.Instance.Layout = SerializeHelper.Load<Layout>(file);
                    myCanvas.Layout = GlobalVars.Instance.Layout;
                }
            }

            e.CanExecute = bok;
        }

        private void Acquire_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<List<PixelInfo>> allPixels = GlobalVars.Instance.UAController.Acquire();
            //List<float> xVals = new List<float>();
            //List<float> yVals = new List<float>();
            //List<float> zVals = new List<float>();
            //List<List<PixelInfo>> allPixels = GenerateTestData(); //uaController.Acquire();
            IEngine iEngine = new IEngine();
            
            brightness = new Brightness(allPixels);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            string sImgFile = FolderHelper.GetImageFolder() + "latest.jpg";
            ImageHelper.SaveBitmapImageIntoFile(bmpImage, sImgFile);
            var pts = iEngine.FindRect(sImgFile);
            myCanvas.SetBkGroundImage(bmpImage,pts);
            Rect rc = GetRect(pts);
            var results = brightness.GetResults(rc);
            lstviewResult.ItemsSource = results;

        }

        private Rect GetRect(List<MPoint> pts)
        {
            double left, right, top, bottom;
            left = pts.Min(pt => pt.x) + 4;
            top = pts.Min(pt => pt.y) + 4;
            right = pts.Max(pt => pt.x) - 3;
            bottom = pts.Max(pt => pt.y) - 3;
            return new Rect(left, top, right - left, bottom - top);
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
            LivewFocusView livewFocusView = new LivewFocusView();
            livewFocusView.ShowDialog();
        }

        //private void CommandHelp_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    AboutBox aboutBox = new AboutBox();
        //    aboutBox.ShowDialog();
        //}

        //private void CommandHelp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = true;
        //}

       
    }
}
