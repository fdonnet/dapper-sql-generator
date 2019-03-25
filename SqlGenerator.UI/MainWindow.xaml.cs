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
        public IEnumerable<TSqlObject> Roles;

        public MainWindow()
        {
            InitializeComponent();
            Settings = new GeneratorSettings();
            DataContext = Settings;
        }


        /// <summary>
        /// Load the dacpac model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonLoadModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_dacpacPath))
                {
                    MessageBox.Show("No dacpac file selected.", "Error to load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    buttonLoadModel.IsEnabled = false;
                    IsEnabled = false;
                    Model = await Task.Run(() => TSqlModelHelper.LoadModel(_dacpacPath));
                    LoadTablesList();
                    Roles = Model.GetAllRoles();
                    ucGlobalSettings.InitGlobalSettings();
                    IsEnabled = true;
                    buttonLoadConfig.IsEnabled = true;
                    buttonSaveConfig.IsEnabled = true;
                    MessageBox.Show("Model loaded successfully", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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

        /// <summary>
        /// Load all the model tables
        /// </summary>
        private void LoadTablesList()
        {
            var tables = Model.GetAllTables();
            lstTables.ItemsSource = tables;
            lstTables.DisplayMemberPath = "Name.Parts[1]";
        }

        private void ButtonGenerateDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        //SqlDeleteGenerator gen = new SqlDeleteGenerator(table, Settings.GlobalSettings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SelectedRolesForDeleteSP);
                        // += gen.Generate();
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
                        SqlInsertGenerator gen = new SqlInsertGenerator(table, Settings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SqlInsertSettings.GrantExecuteToRoles);
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
                        SqlBulkInsertGenerator gen = new SqlBulkInsertGenerator(table, Settings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SqlBulkInsertSettings.GrantExecuteToRoles);
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
                        CsEntityClassGenerator gen = new CsEntityClassGenerator(table, classNamespace: Settings.GlobalSettings.CsEntitySettings.EntitiesNamespace);
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
                        var gen = new SqlSelectAllGenerator(table, author: Settings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SqlSelectAllSettings.GrantExecuteToRoles);
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
                        var gen = new SqlSelectByPKGenerator(table, author: Settings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SqlSelectByPKSettings.GrantExecuteToRoles);
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
                        var gen = new SqlSelectByUKGenerator(table, author: Settings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SqlSelectByUKSettings.GrantExecuteToRoles);
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
                        var gen = new SqlUpdateGenerator(table, author: Settings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SqlUpdateSettings.GrantExecuteToRoles
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
                        var gen = new SqlTableTypeGenerator(table, author: Settings.AuthorName, grantExecuteTo: Settings.GlobalSettings.SqlTableTypeSettings.GrantExecuteToRoles);

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


        /// <summary>
        /// Save settings in a JSON File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "json files (*.json)|*.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                Settings.ConfigPath = saveFileDialog.FileName;
                Settings.SaveConfig();
            }
           
        }

        /// <summary>
        /// Load all the settings from a JSON file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "json files (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Settings.ConfigPath = openFileDialog.FileName;
                Settings = Settings.LoadConfig();
                DataContext = Settings;
                ucGlobalSettings.InitGlobalSettings();

                tabPreview.IsSelected = true;
                lstTables.UnselectAll();
            }
        }


        /// <summary>
        /// When the table selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ucTableSettings.Visibility = Visibility.Hidden;
            chkOverrideSettings.Visibility = Visibility.Hidden;
            TabOverride.IsEnabled = false;

            //if one table selected
            if (lstTables.SelectedItems.Count == 1)
            {
                TabOverride.IsEnabled = true;
                chkOverrideSettings.Visibility = Visibility.Visible;

                //Check if the config already exists
                if (Settings.TablesSettings.Any(s => s.TableName == ((TSqlObject)lstTables.SelectedItems[0]).Name.Parts[1]))
                {
                    chkOverrideSettings.IsChecked = true;

                    //To be sure the correct settings are loaded
                    ucTableSettings.InitTableSettings(((TSqlObject)lstTables.SelectedItems[0]).Name.Parts[1]);
                    chkOverrideSettings.Content = $"Override global settings for table: " +
                        $"{((TSqlObject)lstTables.SelectedItems[0]).Name.Parts[1].ToUpper()}";

                    ucTableSettings.Visibility = Visibility.Visible;
                }
                else
                {
                    chkOverrideSettings.IsChecked = false;
                    ucTableSettings.Visibility = Visibility.Hidden;
                }

            }
        }

        /// <summary>
        /// Override with table settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOverrideSettings_Checked(object sender, RoutedEventArgs e)
        {
            var tableName = ((TSqlObject)lstTables.SelectedItems[0]).Name.Parts[1];
            ucTableSettings.Visibility = Visibility.Visible;
            ucTableSettings.InitTableSettings(tableName);
            chkOverrideSettings.Content = $"Override global settings for table: " +
                $"{tableName.ToUpper()}";

        }

        /// <summary>
        /// Cancel table settings and return to global settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOverrideSettings_Unchecked(object sender, RoutedEventArgs e)
        {
            var tableName = ((TSqlObject)lstTables.SelectedItems[0]).Name.Parts[1];

            var toBeRemoved = Settings.TablesSettings.Where(t => t.TableName == tableName).SingleOrDefault();
            if (toBeRemoved != null)
                Settings.TablesSettings.Remove(toBeRemoved);

            chkOverrideSettings.Content = "Override global settings";
            ucTableSettings.Visibility = Visibility.Hidden;
        }
    }
}
