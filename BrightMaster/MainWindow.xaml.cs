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
        //UAContorller uaController = null;
        Brightness brightness = null;
        ObservableCollection<PixelInfo> pixelInfos = new ObservableCollection<PixelInfo>();
        ObservableCollection<string> layoutFiles = new ObservableCollection<string>();
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutDefineWindow layoutDefWindow = new LayoutDefineWindow();
            layoutDefWindow.ShowDialog();
            EnumLayouts();
#if DEBUG
#else
           //uaController = new UAContorller();
#endif
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
            e.CanExecute = brightness.grayVals.Count != 0;
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

        private void Acquire_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Acquire_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //uaController.Acquire();
            List<float> xVals = new List<float>();
            List<float> yVals = new List<float>();
            List<float> zVals = new List<float>();
            List<List<PixelInfo>> allPixels = GenerateTestData(); //uaController.Acquire();
            brightness = new Brightness(allPixels);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            string sImgFile = FolderHelper.GetExeFolder() + "test.jpg";
            ImageHelper.SaveBitmapImageIntoFile(bmpImage, sImgFile);
            myCanvas.SetBkGroundImage(bmpImage);
        }

        private List<List<PixelInfo>> GenerateTestData()
        {
            List<List<PixelInfo>> allPixelInfos = new List<List<PixelInfo>>();
            for(int y = 0; y < 10; y++)
            {
                List<PixelInfo> linePixelInfos = new List<PixelInfo>();
                for(int x = 0; x < 10; x++)
                {
                    linePixelInfos.Add(new PixelInfo(x, y, 3));
                }
                allPixelInfos.Add(linePixelInfos);
            }
            return allPixelInfos;
            
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
