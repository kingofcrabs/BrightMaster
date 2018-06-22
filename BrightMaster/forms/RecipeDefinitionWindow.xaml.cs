using BrightMaster.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BrightMaster
{
    /// <summary>
    /// Interaction logic for RecipeDefinition.xaml
    /// </summary>
    public partial class RecipeDefinitionWindow : Window
    {
        
        public RecipeDefinitionWindow()
        {
            InitializeComponent();
            this.Loaded += RecipeDefinitionWindow_Loaded;
        }

        void RecipeDefinitionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = GlobalVars.Instance.RecipeCollection;
        }

        private void btnRemoveRecipe_Click(object sender, RoutedEventArgs e)
        {
            GlobalVars.Instance.RecipeCollection.Remove();
        }

        private void btnAddRecipe_Click(object sender, RoutedEventArgs e)
        {
            GlobalVars.Instance.RecipeCollection.AddNew();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckSettings();
                GlobalVars.Instance.RecipeCollection.SelectedRecipe.Save();
            }
            catch(Exception ex)
            {
                SetInfo(ex.Message, true);
                return;
            }
            SetInfo(string.Format("配置：{0}保存成功！", GlobalVars.Instance.RecipeCollection.SelectedRecipe.Name));
        }

        private void CheckSettings()
        {
            if (GlobalVars.Instance.RecipeCollection.SelectedRecipe == null)
                throw new Exception("请选中一个配置！");
            GlobalVars.Instance.RecipeCollection.SelectedRecipe.Layout.CheckSetting();
            GlobalVars.Instance.RecipeCollection.SelectedRecipe.Constrains.CheckSetting();
            GlobalVars.Instance.RecipeCollection.SelectedRecipe.CameraSettings.CheckSetting();
        }

        private void SetInfo(string msg, bool isError = false)
        {
            txtInfo.Text = msg;
            txtInfo.Foreground = isError ? Brushes.Red : Brushes.Black;
        }

       
        private BitmapImage Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, bitmap.RawFormat);
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.Default;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            canvas.SetBoundRect(new System.Drawing.SizeF((float)canvas.ActualWidth, (float)canvas.ActualHeight));
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAdjustRatios_Click(object sender, RoutedEventArgs e)
        {
            if( GlobalVars.Instance.RecipeCollection.SelectedRecipe == null)
            {
                SetInfo("当前Recipe为空！",true);
                return;
            }
            SetInfo("");
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = FolderHelper.GetCorrectionFactorFolder();
            dialog.Multiselect = false;
            dialog.Title = "请选择文件夹";
            dialog.Filter = "所有文件(*.*)|*.*";
            string correctionFactorFile = "";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                correctionFactorFile = dialog.FileName;
            }
            //string correctionFactorFile = FolderHelper.GetCorrectionFactorFile();
            if(correctionFactorFile == "")
            {
                 SetInfo("未选择文件！", true);
                 return;
            }
            List<string> strs = File.ReadAllLines(correctionFactorFile).ToList();
            strs.RemoveAt(0);
            List<AdjustRatio> adjustRatios = new List<AdjustRatio>();
            foreach (var str in strs)
            {
                List<string> adjustRatioStrs = str.Split(',').ToList();
                float x = float.Parse(adjustRatioStrs[1]);
                float y = float.Parse(adjustRatioStrs[2]);
                float z = float.Parse(adjustRatioStrs[3]);
                AdjustRatio adjustRatio = new AdjustRatio(x,y,z);
                adjustRatios.Add(adjustRatio);
            }
            
            var layout = GlobalVars.Instance.RecipeCollection.SelectedRecipe.Layout;
            int expectedCnt = layout.XCount * layout.YCount;
            if(adjustRatios.Count != expectedCnt)
            {
                SetInfo(string.Format("X*Y = {0},但是文件中有{1}条修正系数！", expectedCnt, adjustRatios.Count));
                return;
            }
            GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatios = adjustRatios;
            SetInfo("设置修正系数成功！");
         }

        private void btnClearRatios_Click(object sender, RoutedEventArgs e)
        {
            if (GlobalVars.Instance.RecipeCollection.SelectedRecipe == null)
            {
                SetInfo("当前Recipe为空！", true);
                return;
            }
            GlobalVars.Instance.RecipeCollection.SelectedRecipe.AdjustRatios.Clear();
            SetInfo("清除修正系数成功！");
        }
    }

    public class NegateBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
