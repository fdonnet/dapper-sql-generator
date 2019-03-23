using Microsoft.SqlServer.Dac.Model;
using Microsoft.Win32;
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
        private string _dacpacPath = string.Empty;
        private GeneratorGlobalSettings _settings;

        public MainWindow()
        {
            InitializeComponent();
            _settings = new GeneratorGlobalSettings();
            DataContext = _settings;
        }


        private async void ButtonLoadModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonLoadModel.IsEnabled = false;
                if(_dacpacPath !=string.Empty)
                {
                    IsEnabled = false;
                    Model = await Task.Run(() => TSqlModelHelper.LoadModel(_dacpacPath));
                    LoadTablesList();
                    LoadRolesList();
                    IsEnabled = true;

                }
                
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


        private void LoadTablesList()
        {
            var tables = Model.GetAllTables();
            lstTables.ItemsSource = tables;
            lstTables.DisplayMemberPath = "Name.Parts[1]";
        }

        private void LoadRolesList()
        {
            var roles = Model.GetAllRoles();
            lstRoles.ItemsSource = roles;
            lstRoles.DisplayMemberPath = "Name.Parts[0]";

            //select all by default
            lstRoles.SelectAll();
        }

        private void ButtonGenerateDelete_Click(object sender, RoutedEventArgs e)
        {
            if(lstTables.SelectedItems.Count>0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        SqlDeleteGenerator gen = new SqlDeleteGenerator(table, _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
           
        }

        private void ButtonGenerateInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        SqlInsertGenerator gen = new SqlInsertGenerator(table, _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
         }


        private void ButtonGenerateBulkInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        SqlBulkInsertGenerator gen = new SqlBulkInsertGenerator(table, _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateNetEntityClass_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        CsEntityClassGenerator gen = new CsEntityClassGenerator(table, classNamespace: _settings.EntitiesNamespace);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlSelectAllGenerator(table, author: _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectByPK_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlSelectByPKGenerator(table, author: _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectByUK_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlSelectByUKGenerator(table, author: _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlUpdateGenerator(table, author: _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute
                            , doNotUpdateColumns: new string[] { "inserted_by", "inserted_on" });

                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateTableType_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlTableTypeGenerator(table, author: _settings.AuthorName, grantExecuteTo: _settings.SelectedMSSQLRolesForExecute);

                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "dacpac files (*.dacpac)|*.dacpac"
            };

            if (openFileDialog.ShowDialog() == true)
                txtPath.Text = openFileDialog.FileName;

            _dacpacPath = txtPath.Text;
                
        }

        private void TxtAuthor_TextChanged(object sender, TextChangedEventArgs e)
        {
            _settings.AuthorName = txtAuthor.Text;
        }

        private void LstRoles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstRoles.SelectedItems.Count == 0)
                _settings.SelectedMSSQLRolesForExecute = null;
            else
            {
                List<string> roles = new List<string>();
                foreach(var item in lstRoles.SelectedItems)
                {
                    roles.Add(((TSqlObject)item).Name.Parts[0]);
                }

                _settings.SelectedMSSQLRolesForExecute = roles.ToArray();
            }

        }

        private void TxtEntitiesNamespace_TextChanged(object sender, TextChangedEventArgs e)
        {
            _settings.EntitiesNamespace = txtEntitiesNamespace.Text;
        }
    }
}
