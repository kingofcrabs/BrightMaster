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
    /// Interaction logic for HistoryInfoView.xaml
    /// </summary>
    public partial class HistoryInfoView : Window
    {
        public HistoryInfoView()
        {
            InitializeComponent();
            this.Loaded += HistoryInfoView_Loaded;
        }

        void HistoryInfoView_Loaded(object sender, RoutedEventArgs e)
        {
            lvHistory.ItemsSource = GlobalVars.Instance.RegionsHistoryInfoCollection.AllInfos;
        }
    }
}
