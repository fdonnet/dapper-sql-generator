using Microsoft.SqlServer.Dac.Model;
using SqlGenerator.DotNetClient;
using SqlGenerator.StoredProcedures;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        TSqlModel Model { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();
        }


        private async void ButtonLoadModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonLoadModel.IsEnabled = false;
                Model = await Task.Run(() => TSqlModelHelper.LoadModel("PensionLab.DB.dacpac"));
                MessageBox.Show("Model loaded successfully", "Info", MessageBoxButton.OK, MessageBoxImage.Information);               
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                buttonLoadModel.IsEnabled = true;
            }
        }


        private void ButtonShowTables_Click(object sender, RoutedEventArgs e)
        {
            var tables = Model.GetAllTables();
            lstTables.ItemsSource = tables;
            lstTables.DisplayMemberPath = "Name.Parts[1]";
        }

        private void ButtonGenerateDelete_Click(object sender, RoutedEventArgs e)
        {
            TSqlObject table = lstTables.SelectedItem as TSqlObject;
            if (table != null)
            {
                SqlDeleteGenerator gen = new SqlDeleteGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateInsert_Click(object sender, RoutedEventArgs e)
        {
            TSqlObject table = lstTables.SelectedItem as TSqlObject;
            if (table != null)
            {
                SqlInsertGenerator gen = new SqlInsertGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateNetEntityClass_Click(object sender, RoutedEventArgs e)
        {
            TSqlObject table = lstTables.SelectedItem as TSqlObject;
            if (table != null)
            {
                CsEntityClassGenerator gen = new CsEntityClassGenerator(table, classNamespace: "PensionLab.DTO");
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectAll_Click(object sender, RoutedEventArgs e)
        {
            TSqlObject table = lstTables.SelectedItem as TSqlObject;
            if (table != null)
            {
                var gen = new SqlSelectAllGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectByPK_Click(object sender, RoutedEventArgs e)
        {
            TSqlObject table = lstTables.SelectedItem as TSqlObject;
            if (table != null)
            {
                var gen = new SqlSelectByPKGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateUpdate_Click(object sender, RoutedEventArgs e)
        {
            TSqlObject table = lstTables.SelectedItem as TSqlObject;
            if (table != null)
            {
                var gen = new SqlUpdateGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" }, doNotUpdateColumns: new string[] { "inserted_by", "inserted_on" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }
    }
}
