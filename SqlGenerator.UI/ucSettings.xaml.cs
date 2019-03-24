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

        public ucSettings()
        {
            InitializeComponent();
        }

        public void InitGlobalSettings(MainWindow parent)
        {

            DataContext = parent.Settings.GlobalSettings;

        }

        public void InitTableSettings(MainWindow parent)
        {
            //_settings = settings;
            //DataContext

        }
    }
}
