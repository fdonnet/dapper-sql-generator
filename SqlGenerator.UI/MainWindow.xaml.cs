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
        private GeneratorSettings _settings;
        private IEnumerable<TSqlObject> _roles;

        public MainWindow()
        {
            InitializeComponent();
            _settings = new GeneratorSettings();
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
                    LoadRolesLists();
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

        private void LoadRolesLists()
        {
            _roles = Model.GetAllRoles();

            //DeleteSP
            lstRolesForDeleteSp.ItemsSource = _roles;
            lstRolesForDeleteSp.DisplayMemberPath = "Name.Parts[0]";
            lstRolesForDeleteSp.SelectAll();

            //InsertSP
            lstRolesForInsertSp.ItemsSource = _roles;
            lstRolesForInsertSp.DisplayMemberPath = "Name.Parts[0]";
            lstRolesForInsertSp.SelectAll();

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
                        SqlDeleteGenerator gen = new SqlDeleteGenerator(table, _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForDeleteSP);
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
                        SqlInsertGenerator gen = new SqlInsertGenerator(table, _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForInsertSP);
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
                        SqlBulkInsertGenerator gen = new SqlBulkInsertGenerator(table, _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForBulkInsertSP);
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
                        CsEntityClassGenerator gen = new CsEntityClassGenerator(table, classNamespace: _settings.GlobalSettings.EntitiesNamespace);
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
                        var gen = new SqlSelectAllGenerator(table, author: _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForSelectAllSP);
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
                        var gen = new SqlSelectByPKGenerator(table, author: _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForSelectByPKSP);
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
                        var gen = new SqlSelectByUKGenerator(table, author: _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForSelectByUKSP);
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
                        var gen = new SqlUpdateGenerator(table, author: _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForUpdateSP
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
                        var gen = new SqlTableTypeGenerator(table, author: _settings.GlobalSettings.AuthorName, grantExecuteTo: _settings.GlobalSettings.SelectedRolesForTableType);

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
            _settings.GlobalSettings.AuthorName = txtAuthor.Text;
        }



        private void TxtEntitiesNamespace_TextChanged(object sender, TextChangedEventArgs e)
        {
            _settings.GlobalSettings.EntitiesNamespace = txtEntitiesNamespace.Text;
        }

        private void ButtonSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            _settings.SaveConfig();
        }

        private void ButtonLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            _settings = _settings.LoadConfig();
            DataContext = _settings;
        }

        private void LstRolesForInsertSp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstRolesForInsertSp.SelectedItems.Count == 0)
                _settings.GlobalSettings.SelectedRolesForInsertSP = null;
            else
            {
                List<string> roles = new List<string>();
                foreach (var item in lstRolesForInsertSp.SelectedItems)
                {
                    roles.Add(((TSqlObject)item).Name.Parts[0]);
                }

                _settings.GlobalSettings.SelectedRolesForInsertSP = roles.ToArray();
            }
        }

        private void LstRolesForDeleteSp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstRolesForDeleteSp.SelectedItems.Count == 0)
                _settings.GlobalSettings.SelectedRolesForDeleteSP = null;
            else
            {
                List<string> roles = new List<string>();
                foreach (var item in lstRolesForDeleteSp.SelectedItems)
                {
                    roles.Add(((TSqlObject)item).Name.Parts[0]);
                }

                _settings.GlobalSettings.SelectedRolesForDeleteSP = roles.ToArray();
            }
        }
    }
}
