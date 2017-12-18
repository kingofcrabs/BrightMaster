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
    /// Interaction logic for WholePanelHistoryInfoView.xaml
    /// </summary>
    public partial class WholePanelHistoryInfoView : Window
    {
        public WholePanelHistoryInfoView()
        {
            InitializeComponent();
            this.Loaded += WholePanelHistoryInfoView_Loaded;
        }

        void WholePanelHistoryInfoView_Loaded(object sender, RoutedEventArgs e)
        {
            lvHistory.ItemsSource = GlobalVars.Instance.WholePanelHistoryInfoCollection.AllInfos;
        }
    }
}
