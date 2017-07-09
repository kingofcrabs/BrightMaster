using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
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

namespace BrightMaster
{
    /// <summary>
    /// Interaction logic for LayoutDefineWindow.xaml
    /// </summary>
    public partial class LayoutDefineWindow : Window
    {
        ObservableCollection<string> layoutFiles = new ObservableCollection<string>();
        public LayoutDefineWindow()
        {
            InitializeComponent();
            EnumLayouts();
            if(layoutFiles.Count > 0)
            {
                LoadLayout(layoutFiles.First());
                this.DataContext = GlobalVars.Instance.Layout;
                cmbLayouts.SelectedIndex = 0;
            }
            cmbLayouts.SelectionChanged += cmbLayouts_SelectionChanged;
        }

        void cmbLayouts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLayouts.SelectedIndex == -1)
                return;
            string sName = cmbLayouts.SelectedItem.ToString();
            LoadLayout(sName);
        }

        private void EnumLayouts()
        {
            string layoutFolder = FolderHelper.GetLayoutFolder();
            DirectoryInfo dirInfo = new DirectoryInfo(layoutFolder);
            var files = dirInfo.EnumerateFiles("*.xml").ToList();
            files.ForEach(x => layoutFiles.Add(x.Name.Replace(".xml","")));
            cmbLayouts.ItemsSource = layoutFiles;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            this.DataContext = GlobalVars.Instance.Layout;
            canvas.Layout = GlobalVars.Instance.Layout;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
               
        }

 
        private void SetHint(string sText, bool isError = false)
        {
            txtInfo.Text = sText;
            txtInfo.Foreground = isError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Black;
        }

        //private void btnLoad_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cmbLayouts.SelectedIndex == -1)
        //    {
        //        SetHint("请选择一个layout！", true);
        //        return;
        //    }
        //    string sName = cmbLayouts.SelectedItem.ToString();
        //    LoadLayout(sName);
           
        //}

        private void LoadLayout(string sName)
        {
           
            string sFile = FolderHelper.GetLayoutFolder() + sName + ".xml";
            SerializeHelper.LoadSettings(ref GlobalVars.Instance.Layout, sFile);
            Refresh();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string fileName = txtSavePath.Text;
            if (fileName == "")
            {
                SetHint("保存路径不得为空！", true);
                return;
            }
            SerializeHelper.SaveSettings(GlobalVars.Instance.Layout, FolderHelper.GetLayoutFolder() + fileName + ".xml");
            layoutFiles.Add(fileName);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string fileName = cmbLayouts.SelectedItem.ToString();
            string fullPath = FolderHelper.GetLayoutFolder() + fileName + ".xml";
            SerializeHelper.SaveSettings(GlobalVars.Instance.Layout, fullPath);
            SetHint("文件已经保存到：" + fullPath);
        }
    }
}
