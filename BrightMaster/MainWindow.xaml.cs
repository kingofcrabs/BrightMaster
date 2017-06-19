using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        UAContorller uaController = new UAContorller();
        Brightness brightness = null;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
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
            var allPixels = uaController.Acquire();
            brightness = new Brightness(allPixels);
            BitmapImage bmpImage = ImageHelper.CreateImage(brightness.grayVals);
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
