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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestSlider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResetSlider(discreteSlider1, 8);
        }

        private void discreteSlider1_ValueChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            slider1Text.Text = "New Value: " + discreteSlider1.Value;
        }

        private void ResetSlider(ExtControls.DiscreteSlider slider, int count)
        {
            int[] values = new int[count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (int)Math.Pow(2,i+1);
            }
            slider.Reset(values.OrderByDescending(x => x).ToArray());
        }
    }
}
