using BrightMaster.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BrightMaster.forms
{
    /// <summary>
    /// Interaction logic for PseudoColorSetting.xaml
    /// </summary>
    public partial class PseudoColorSetting : Window
    {
        MyVisualHost visualHost;
        public PseudoColorSetting()
        {
            InitializeComponent();
            this.Loaded += PseudoColorSetting_Loaded;
        }

        void PseudoColorSetting_Loaded(object sender, RoutedEventArgs e)
        {

            visualHost = new MyVisualHost(myCanvas);
            myCanvas.Children.Add(visualHost); 
            myCanvas.InvalidateVisual();
        }
    }
}
