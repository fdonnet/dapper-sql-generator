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

        public string DacpacPath { get; set; } = string.Empty;

        public GeneratorSettings GeneratorSettings { get; set; } = null;

        public IEnumerable<TSqlObject> Roles { get; set; } = null;

        /// <summary>
        /// Semaphore variable to check loading status. If > 0, then there's something loading.
        /// </summary>
        private int _loading = 0;


        public MainWindow()
        {
            InitializeComponent();
            buttonBrowse.IsEnabled = true;
            ucGlobalSettings.IsEnabled = false;
            GeneratorSettings = new GeneratorSettings();
            DataContext = GeneratorSettings;
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
            var tables = Model.GetAllTables();
            lstSelectedTables.ItemsSource = tables;
            lstSelectedTables.DisplayMemberPath = "Name.Parts[1]";
            lstSelectedTables.SelectAll();
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
            if (lstSelectedTables.DataContext is GeneratorSettings generatorSettings)
            {
                if (!generatorSettings.GenerateForAllTables)
                {
                    lstSelectedTables.IsEnabled = true;

                    if (generatorSettings.SelectedTables?.Any() ?? false)
                    {
                        _loading++;
                        lstSelectedTables.UnSelectAll();
                        foreach (var item in lstSelectedTables.Items)
                        {
                            if (generatorSettings.SelectedTables.Any(r => r == ((TSqlObject)item).Name.Parts[1]))
                            {
                                lstSelectedTables.SelectedItems.Add(item);
                            }
                        }
                        _loading--;
                    }
                    else
                    {
                        lstSelectedTables.SelectAll();
                    }
                }
                else
                {
                    lstSelectedTables.IsEnabled = false;
                    lstSelectedTables.SelectAll();
                }
            }
        }


        private void LstSelectedTables_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            // Update the data source
            if (_loading == 0)
            {
                if (lstSelectedTables.DataContext is GeneratorSettings generatorSettings)
                {
                    if (!generatorSettings.GenerateForAllTables)
                        generatorSettings.SelectedTables = GetSelectedTablesFromCheckListBox().OrderBy(tbl => tbl).ToList();
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
            if (lstSelectedTables.SelectedItems.Count == 1)
            {
                TabOverride.IsEnabled = true;
                chkOverrideSettings.Visibility = Visibility.Visible;

                //Check if the config already exists
                if (GeneratorSettings.TablesSettings.ContainsKey(((TSqlObject)lstSelectedTables.SelectedItems[0]).Name.Parts[1]))
                {
                    chkOverrideSettings.IsChecked = true;

                    //To be sure the correct settings are loaded
                    ucTableSettings.InitTableSettings((TSqlObject)lstSelectedTables.SelectedItems[0]);
                    chkOverrideSettings.Content = $"Override global settings for table: " +
                        $"{((TSqlObject)lstSelectedTables.SelectedItems[0]).Name.Parts[1].ToUpper()}";

                    ucTableSettings.Visibility = Visibility.Visible;
                }
                else
                {
                    chkOverrideSettings.IsChecked = false;
                    ucTableSettings.Visibility = Visibility.Hidden;
                }

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
                GeneratorSettings.SaveToFile(configPath);
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
                GeneratorSettings = GeneratorSettings.LoadFromFile(configPath);
                DataContext = GeneratorSettings;
                ucGlobalSettings.InitGlobalSettings();

                tabPreview.IsSelected = true;
                _loading--;

                MessageBox.Show("Configuration(all settings) loaded successfully",
                     "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        /// <summary>
        /// Override with table settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOverrideSettings_Checked(object sender, RoutedEventArgs e)
        {
            ucTableSettings.Visibility = Visibility.Visible;
            ucTableSettings.InitTableSettings(((TSqlObject)lstSelectedTables.SelectedItems[0]));
            chkOverrideSettings.Content = $"Override global settings for table: " +
                $"{((TSqlObject)lstSelectedTables.SelectedItems[0]).Name.Parts[1].ToUpper()}";

        }


        /// <summary>
        /// Cancel table settings and return to global settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkOverrideSettings_Unchecked(object sender, RoutedEventArgs e)
        {
            var tableName = ((TSqlObject)lstSelectedTables.SelectedItems[0]).Name.Parts[1];

            bool toBeRemoved = GeneratorSettings.TablesSettings.ContainsKey(tableName);
            if (toBeRemoved)
                GeneratorSettings.TablesSettings.Remove(tableName);

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
                GeneratorSettings.OutputPath_SqlScripts = outPath;
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
                GeneratorSettings.OutputPath_CsEntityClasses = outPath;
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
                GeneratorSettings.OutputPath_CsRepositoryClasses = outPath;
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
                await Task.Run(() => FileGeneratorHelper.WriteSqlScriptsFileAsync(Model, GeneratorSettings,
                    (progress) => Dispatcher.Invoke(() => progBarGenerateAllFiles.Value = progress * 50 / 100)));

                progBarGenerateAllFiles.Value = 50;

                // Write C# file containing all generated entity classes   
                await Task.Run(() => FileGeneratorHelper.WriteCsEntityClassesFileAsync(Model, GeneratorSettings,
                    (progress) => Dispatcher.Invoke(() => progBarGenerateAllFiles.Value = 50 + progress * 10 / 100)));

                progBarGenerateAllFiles.Value = 60;


                // Write C# file containing all generated Dapper repositories
                await Task.Run(() => FileGeneratorHelper.WriteCsRepositoryClassesFileAsync(Model, GeneratorSettings,
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
                        SqlDeleteGenerator gen = new SqlDeleteGenerator(GeneratorSettings, table, true);
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
                        SqlInsertGenerator gen = new SqlInsertGenerator(GeneratorSettings, table, true);
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
                        SqlBulkInsertGenerator gen = new SqlBulkInsertGenerator(GeneratorSettings, table, true);
                        output += gen.Generate();
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
                        CsEntityClassGenerator gen = new CsEntityClassGenerator(GeneratorSettings, table, true);
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
                        var gen = new SqlSelectAllGenerator(GeneratorSettings, table, true);
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
                        var gen = new SqlSelectByPKGenerator(GeneratorSettings, table, true);
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
                        var gen = new SqlSelectByUKGenerator(GeneratorSettings, table, true);
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
                        var gen = new SqlUpdateGenerator(GeneratorSettings, table, true);

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
                        var gen = new SqlTableTypeGenerator(GeneratorSettings, table, true);

                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateBaseRepo_Click(object sender, RoutedEventArgs e)
        {

            string output = string.Empty;
            var gen = new CsDbContextGenerator(GeneratorSettings, Model);
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
                        var gen = new CsRepositoryClassGenerator(GeneratorSettings, table, true);

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
                        var gen = new SqlSelectByPKListGenerator(GeneratorSettings, table, true);
                        output += gen.Generate();
                    }
                }
                txtOutput.Text = output;
            }
        }

        #endregion

    }
}
