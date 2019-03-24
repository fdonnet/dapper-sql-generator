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
        public GeneratorSettings Settings;
        private IEnumerable<TSqlObject> _roles;

        public MainWindow()
        {
            InitializeComponent();
            Settings = new GeneratorSettings();
            DataContext = Settings;
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
                    ucGlobalSettings.InitGlobalSettings();
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
            //lstRolesForDeleteSp.ItemsSource = _roles;
            //lstRolesForDeleteSp.DisplayMemberPath = "Name.Parts[0]";
            //lstRolesForDeleteSp.SelectAll();

            ////InsertSP
            //lstRolesForInsertSp.ItemsSource = _roles;
            //lstRolesForInsertSp.DisplayMemberPath = "Name.Parts[0]";
            //lstRolesForInsertSp.SelectAll();

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
                        SqlDeleteGenerator gen = new SqlDeleteGenerator(table, Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForDeleteSP);
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
                        SqlInsertGenerator gen = new SqlInsertGenerator(table, Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForInsertSP);
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
                        SqlBulkInsertGenerator gen = new SqlBulkInsertGenerator(table, Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForBulkInsertSP);
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
                        CsEntityClassGenerator gen = new CsEntityClassGenerator(table, classNamespace: Settings.GlobalSettings.EntitiesNamespace);
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
                        var gen = new SqlSelectAllGenerator(table, author: Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForSelectAllSP);
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
                        var gen = new SqlSelectByPKGenerator(table, author: Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForSelectByPKSP);
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
                        var gen = new SqlSelectByUKGenerator(table, author: Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForSelectByUKSP);
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
                        var gen = new SqlUpdateGenerator(table, author: Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForUpdateSP
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
                        var gen = new SqlTableTypeGenerator(table, author: Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForTableType);

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

        //private void TxtAuthor_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Settings.GlobalSettings.AuthorName = txtAuthor.Text;
        //}



        //private void TxtEntitiesNamespace_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Settings.GlobalSettings.EntitiesNamespace = txtEntitiesNamespace.Text;
        //}

        private void ButtonSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            Settings.SaveConfig();
        }

        private void ButtonLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            Settings = Settings.LoadConfig();
            DataContext = Settings;
        }

        private void ChkOverrideSettings_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ChkOverrideSettings_Click(object sender, RoutedEventArgs e)
        {
            if(chkOverrideSettings.IsChecked == true)
            {
                //Check if only one table is selected
                if (lstTables.SelectedItems.Count != 1)
                {
                    MessageBox.Show("Select at least and at max one table to override settings.");
                    chkOverrideSettings.IsChecked = false;
                }
                else
                {
                    ucTableSettings.IsEnabled = true;
                    ucTableSettings.InitTableSettings(((TSqlObject)lstTables.SelectedItems[0]).Name.Parts[1]);
                    chkOverrideSettings.Content = $"Override global settings for table: " +
                        $"{((TSqlObject)lstTables.SelectedItems[0]).Name.Parts[1].ToUpper()}";

                }
            }
            else
            {
                chkOverrideSettings.Content = "Override global settings";
                ucTableSettings.IsEnabled = false;
            }
            
        }

        //private void LstRolesForInsertSp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (lstRolesForInsertSp.SelectedItems.Count == 0)
        //        Settings.GlobalSettings.SelectedRolesForInsertSP = null;
        //    else
        //    {
        //        List<string> roles = new List<string>();
        //        foreach (var item in lstRolesForInsertSp.SelectedItems)
        //        {
        //            roles.Add(((TSqlObject)item).Name.Parts[0]);
        //        }

        //        Settings.GlobalSettings.SelectedRolesForInsertSP = roles.ToArray();
        //    }
        //}

        //private void LstRolesForDeleteSp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (lstRolesForDeleteSp.SelectedItems.Count == 0)
        //        Settings.GlobalSettings.SelectedRolesForDeleteSP = null;
        //    else
        //    {
        //        List<string> roles = new List<string>();
        //        foreach (var item in lstRolesForDeleteSp.SelectedItems)
        //        {
        //            roles.Add(((TSqlObject)item).Name.Parts[0]);
        //        }

        //        Settings.GlobalSettings.SelectedRolesForDeleteSP = roles.ToArray();
        //    }
        //}
    }
}
