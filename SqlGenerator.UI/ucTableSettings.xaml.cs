using Microsoft.SqlServer.Dac.Model;
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
    /// Interaction logic for ucTableSettings.xaml
    /// </summary>
    public partial class ucTableSettings : ucSettings
    {
        public ucTableSettings()
        {
             InitializeComponent();
        }

        public void Init(TSqlModel model)
        {
            base.Init();

        }
    }
}
