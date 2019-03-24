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
    /// Interaction logic for ucSettings.xaml
    /// </summary>
    public partial class ucSettings : UserControl
    {
        protected TSqlModel Model { get; set; } = null;

        public ucSettings()
        {
            InitializeComponent();
        }

        public void Init(TSqlModel model)
        {
            Model = model;
            LoadRolesList();
        }

        private void LoadRolesList()
        {
            var roles = Model.GetAllRoles();
            lstRoles.ItemsSource = roles;
            lstRoles.DisplayMemberPath = "Name.Parts[0]";

            //select all by default
            lstRoles.SelectAll();
        }
    }
}
