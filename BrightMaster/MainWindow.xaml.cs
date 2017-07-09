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
            //uaController.Acquire();
            List<float> xVals = new List<float>();
            List<float> yVals = new List<float>();
            List<float> zVals = new List<float>();
            List<List<PixelInfo>> allPixels = GenerateTestData(); //uaController.Acquire();
            IEngine iEngine = new IEngine();
            
            brightness = new Brightness(allPixels);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.grayVals);
            string sImgFile = FolderHelper.GetExeFolder() + "test.jpg";
            ImageHelper.SaveBitmapImageIntoFile(bmpImage, sImgFile);
            var pts = iEngine.FindRect(sImgFile);
            myCanvas.SetBkGroundImage(bmpImage,pts);
        }

        private List<List<PixelInfo>> GenerateTestData()
        {
            List<List<PixelInfo>> allPixelInfos = new List<List<PixelInfo>>();
            for(int y = 0; y < 400; y++)
            {
                List<PixelInfo> linePixelInfos = new List<PixelInfo>();
                for(int x = 0; x < 400; x++)
                {
                    if (x < 50 || y < 50 || x > 250 || y > 250)
                    {
                        linePixelInfos.Add(new PixelInfo(0, 0, 0));
                    }
                    else
                        linePixelInfos.Add(new PixelInfo(150, 150, 150));
                    
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
