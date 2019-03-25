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

namespace SqlGenerator.UI
{
    /// <summary>
    /// Interaction logic for ucSettings.xaml
    /// </summary>
    public partial class ucSettings : UserControl
    {
        private MainWindow _parent;
        public ucSettings()
        {
            InitializeComponent();
        }

        public void InitGlobalSettings()
        {
            _parent = (MainWindow)Application.Current.MainWindow;
            DataContext = _parent.Settings.GlobalSettings;
            tabCustomDeco.Visibility = Visibility.Hidden;
            tabCustomField.Visibility = Visibility.Hidden;

        }

        public void InitTableSettings(string tableName)
        {
            _parent = (MainWindow) Application.Current.MainWindow;
            var curTableSettings = _parent.Settings.TablesSettings.Where(t => t.TableName == tableName).SingleOrDefault();
            
            if(curTableSettings == null)
            {
                curTableSettings = curTableSettings.CopyFromGlobalSettings(_parent.Settings.GlobalSettings);
                curTableSettings.TableName = tableName;
                
               _parent.Settings.TablesSettings.Add(curTableSettings);
            }


            DataContext = curTableSettings;
            tabCustomDeco.Visibility = Visibility.Visible;
            tabCustomField.Visibility = Visibility.Visible;

        }

    }
}
