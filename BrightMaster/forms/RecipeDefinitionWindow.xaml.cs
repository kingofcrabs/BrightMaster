using BrightMaster.Settings;
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
    }
}
