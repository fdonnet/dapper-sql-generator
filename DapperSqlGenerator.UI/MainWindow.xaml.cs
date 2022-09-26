using Microsoft.SqlServer.Dac.Model;
using Microsoft.Win32;
using DapperSqlGenerator.DotNetClient;
using DapperSqlGenerator.StoredProcedures;
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
using System.ComponentModel;
using Xceed.Wpf.Toolkit;
using MessageBox = System.Windows.MessageBox;

namespace DapperSqlGenerator.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public TSqlModel Model { get; set; } = null;

        public IEnumerable<TSqlObject> ModelTables { get; set; } = null;


        public string DacpacPath { get; set; } = string.Empty;

        private GeneratorSettings _generatorSettings = null;

        public GeneratorSettings GeneratorSettings { 
            get { return _generatorSettings; }  
            set { _generatorSettings = value; }
        }

        public IEnumerable<TSqlObject> Roles { get; set; } = null;


        /// <summary>
        /// Semaphore variable to check loading status. If > 0, then there's something loading.
        /// </summary>
        private int _loading = 0;


        public MainWindow(GeneratorSettings generatorSettings)
        {
            InitializeComponent();
            buttonBrowse.IsEnabled = true;
            ucGlobalSettings.IsEnabled = false;
            _generatorSettings = generatorSettings;
            DataContext = _generatorSettings;
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
                if (string.IsNullOrEmpty(DacpacPath))
                {
                    MessageBox.Show("No dacpac file selected.", "Error to load", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    _loading++;
                    buttonLoadModel.IsEnabled = false;
                    IsEnabled = false;

                    Model = await Task.Run(() => TSqlModelHelper.LoadModel(DacpacPath));
                    LoadTablesFromModel();
                    Roles = Model.GetAllRoles();

                    ucGlobalSettings.InitGlobalSettings();
                    ucGlobalSettings.IsEnabled = true;
                    IsEnabled = true;
                    buttonLoadConfig.IsEnabled = true;
                    buttonSaveConfig.IsEnabled = true;
                    _loading--;

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
        /// Load all tables from the model, and show them in the lstSelectedTables list
        /// </summary>
        private void LoadTablesFromModel()
        {
            ModelTables = Model.GetAllTables();

            lstSelectedTables.ItemsSource = ModelTables;
            lstSelectedTables.DisplayMemberPath = "Name.Parts[1]";
            lstSelectedTables.SelectAll();

            comboOverrideSettingsForTable.ItemsSource = ModelTables;
            comboOverrideSettingsForTable.DisplayMemberPath = "Name.Parts[1]";
        }

        private void chkGenerateForAllTables_Checked(object sender, RoutedEventArgs e)
        {
            LstSelectedTables_DataContextChanged(lstSelectedTables,
                new DependencyPropertyChangedEventArgs(CheckListBox.DataContextProperty, lstSelectedTables.DataContext, lstSelectedTables.DataContext));
        }

        private void chkGenerateForAllTables_Unchecked(object sender, RoutedEventArgs e)
        {
            LstSelectedTables_DataContextChanged(lstSelectedTables,
                new DependencyPropertyChangedEventArgs(CheckListBox.DataContextProperty, lstSelectedTables.DataContext, lstSelectedTables.DataContext));
        }


        /// <summary>
        /// Initializes the check list box, selecting (checking) the roles which are granted to the SQL procedure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstSelectedTables_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (lstSelectedTables.DataContext is GeneratorSettings _generatorSettings)
            {
                _loading++;
                if (!_generatorSettings.RunGeneratorForAllTables)
                {
                    lstSelectedTables.IsEnabled = true;
                    bool selectedTablesIsNullOrEmpty = _generatorSettings.RunGeneratorForSelectedTables?.Any() ?? false;
                    if (selectedTablesIsNullOrEmpty)
                    {
                        lstSelectedTables.UnSelectAll();
                        foreach (var item in lstSelectedTables.Items)
                        {
                            if (_generatorSettings.RunGeneratorForSelectedTables.Any(r => r == ((TSqlObject)item).Name.Parts[1]))
                            {
                                lstSelectedTables.SelectedItems.Add(item);
                            }
                        }
                    }
                    else
                    {
                        lstSelectedTables.SelectAll();
                        _generatorSettings.RunGeneratorForSelectedTables = GetSelectedTablesFromCheckListBox().OrderBy(tbl => tbl).ToList();
                    }
                }
                else
                {
                    lstSelectedTables.IsEnabled = false;
                    lstSelectedTables.SelectAll();
                }
                _loading--;
            }
        }


        private void LstSelectedTables_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            // Update the data source
            if (_loading == 0)
            {
                if (lstSelectedTables.DataContext is GeneratorSettings _generatorSettings)
                {
                    if (!_generatorSettings.RunGeneratorForAllTables)
                        _generatorSettings.RunGeneratorForSelectedTables = GetSelectedTablesFromCheckListBox().OrderBy(tbl => tbl).ToList();
                }
            }
        }



        /// <summary>
        /// Extract IEnumerable of strings (tables names) from the checklist box
        /// </summary>
        /// <param name="curList"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSelectedTablesFromCheckListBox()
        {
            foreach (var item in lstSelectedTables.SelectedItems)
            {
                yield return ((TSqlObject)item).Name.Parts[1];
            }
        }

        

        private void ButtonBrowseDacpac_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "dacpac files (*.dacpac)|*.dacpac"
            };

            if (openFileDialog.ShowDialog() == true)
                txtPath.Text = openFileDialog.FileName;

            DacpacPath = txtPath.Text;

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
                var configPath = saveFileDialog.FileName;
                _generatorSettings.SaveToFile(configPath);
                MessageBox.Show("Configuration(all settings) saved successfully",
                     "Info", MessageBoxButton.OK, MessageBoxImage.Information);

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
                _loading++;

                var configPath = openFileDialog.FileName;
                _generatorSettings = GeneratorSettings.LoadFromFile(configPath);
                DataContext = _generatorSettings;
                ucGlobalSettings.InitGlobalSettings();

                tabPreview.IsSelected = true;
                _loading--;

                MessageBox.Show("Configuration(all settings) loaded successfully",
                     "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void ComboOverrideSettingsForTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboOverrideSettingsForTable.SelectedItem is TSqlObject selectedTable)
            {
                string tableName = selectedTable.Name.Parts[1].ToLower();

                if (_generatorSettings.TablesSettings.ContainsKey(tableName))
                    chkOverrideSettings.IsChecked = true;
                else
                    chkOverrideSettings.IsChecked = false;
            }
        }


        /// <summary>
        /// Override with table settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOverrideSettings_Checked(object sender, RoutedEventArgs e)
        {
            // TODO : make current combo box item in bold
            ucTableSettings.Visibility = Visibility.Hidden;

            if (comboOverrideSettingsForTable.SelectedItem is TSqlObject selectedTable)
            {
                ucTableSettings.Visibility = Visibility.Visible;
                ucTableSettings.InitTableSettings(selectedTable);
                chkOverrideSettings.Content = $"Override global settings for table: {selectedTable.Name.Parts[1]}";
            }
        }


        /// <summary>
        /// Cancel table settings and return to global settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOverrideSettings_Unchecked(object sender, RoutedEventArgs e)
        {
            // TODO : make current combo box item not in bold
            var tableName = ((TSqlObject)lstSelectedTables.SelectedItems[0]).Name.Parts[1];

            bool toBeRemoved = _generatorSettings.TablesSettings.ContainsKey(tableName);
            if (toBeRemoved)
                _generatorSettings.TablesSettings.Remove(tableName);

            chkOverrideSettings.Content = "Override global settings";
            ucTableSettings.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Open a save dialog, to browse and select output path for SQL stored procedures
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBrowseOutputPath_SQLProcedures_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "T-SQL script file (*.sql)|*.sql"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var outPath = saveFileDialog.FileName;
                _generatorSettings.OutputPath_SqlScripts = outPath;
                txtOutputPath_SQLProcedures.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }


        /// <summary>
        /// Open a save dialog, to browse and select output path for C# entity classes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBrowseOutputPath_CsEntityClasses_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "C# source code file (*.cs)|*.cs"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var outPath = saveFileDialog.FileName;
                _generatorSettings.OutputPath_CsEntityClasses = outPath;
                txtOutputPath_CsEntityClasses.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }


        /// <summary>
        /// Open a save dialog, to browse and select output path for C# Dapper repositories
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBrowseOutputPath_CsRepositoryClasses_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "C# source code file (*.cs)|*.cs"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var outPath = saveFileDialog.FileName;
                _generatorSettings.OutputPath_CsRepositoryClasses = outPath;
                txtOutputPath_CsRepositoryClasses.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }


        private async void ButtonGenerateAllFiles_Click(object sender, RoutedEventArgs e)
        {
            var ans = MessageBox.Show("Generation output could overwrite already existing files. Are you sure to continue?",
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (ans == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                labelForButtonGenerateAllFiles.Visibility = Visibility.Hidden;
                labelForProgBarGenerateAllFiles.Visibility = Visibility.Visible;
                buttonGenerateAllFiles.Visibility = Visibility.Hidden;
                progBarGenerateAllFiles.Visibility = Visibility.Visible;
                progBarGenerateAllFiles.Value = 0;


                // Write SQL file containing all generated scripts and stored procedures     
                await Task.Run(() => FileGeneratorHelper.WriteSqlScriptsFileAsync(Model, _generatorSettings,
                    (progress) => Dispatcher.Invoke(() => progBarGenerateAllFiles.Value = progress * 50 / 100)));

                progBarGenerateAllFiles.Value = 50;

                // Write C# file containing all generated entity classes   
                await Task.Run(() => FileGeneratorHelper.WriteCsEntityClassesFileAsync(Model, _generatorSettings,
                    (progress) => Dispatcher.Invoke(() => progBarGenerateAllFiles.Value = 50 + progress * 10 / 100)));

                progBarGenerateAllFiles.Value = 60;


                // Write C# file containing all generated Dapper repositories
                await Task.Run(() => FileGeneratorHelper.WriteCsRepositoryClassesFileAsync(Model, _generatorSettings,
                    (progress) => Dispatcher.Invoke(() => progBarGenerateAllFiles.Value = 60 + progress * 40 / 100)));

                progBarGenerateAllFiles.Value = 100;

                MessageBox.Show("Generation completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                labelForButtonGenerateAllFiles.Visibility = Visibility.Visible;
                labelForProgBarGenerateAllFiles.Visibility = Visibility.Hidden;
                buttonGenerateAllFiles.Visibility = Visibility.Visible;
                progBarGenerateAllFiles.Visibility = Visibility.Hidden;

            }
            catch (Exception exc)
            {
                MessageBox.Show($"Error occurred: {exc.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #region PREVIEW METHODS


        private void ButtonGenerateDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        SqlDeleteGenerator gen = new SqlDeleteGenerator(_generatorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }

        }


        private void ButtonGenerateInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        SqlInsertGenerator gen = new SqlInsertGenerator(_generatorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateBulkInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        SqlBulkInsertGenerator gen = new SqlBulkInsertGenerator(_generatorSettings, table, true);
                        SqlBulkUpdateGenerator gen2 = new SqlBulkUpdateGenerator(_generatorSettings, table, true);
                        output += gen.Generate() + gen2.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateNetEntityClass_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        CsEntityClassGenerator gen = new CsEntityClassGenerator(_generatorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlSelectAllGenerator(_generatorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectByPK_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlSelectByPKGenerator(_generatorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateSelectByUK_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlSelectByUKGenerator(_generatorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlUpdateGenerator(_generatorSettings, table, true);

                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateTableType_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlTableTypeGenerator(_generatorSettings, table, true);

                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateBaseRepo_Click(object sender, RoutedEventArgs e)
        {

            string output = string.Empty;
            var gen = new CsDbContextGenerator(_generatorSettings, Model);
            output = gen.Generate();

            txtOutput.Text = output;

        }


        private void ButtonGenerateClassRepo_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new CsRepositoryClassGenerator(_generatorSettings, table, true);

                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateSelectByPKList_Click(object sender, RoutedEventArgs e)
        {
            if (lstSelectedTables.SelectedItems.Count > 0)
            {
                string output = string.Empty;
                foreach (var item in lstSelectedTables.SelectedItems)
                {
                    if (item is TSqlObject table)
                    {
                        var gen = new SqlSelectByPKListGenerator(_generatorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        #endregion

    }
}
