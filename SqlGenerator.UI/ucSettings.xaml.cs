using Microsoft.SqlServer.Dac.Model;
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
using Xceed.Wpf.Toolkit;

namespace SqlGenerator.UI
{
    /// <summary>
    /// Interaction logic for ucSettings.xaml
    /// </summary>
    public partial class ucSettings : UserControl
    {

        private MainWindow _parent;
        private bool _isGlobalSettings = true;
        private Settings _curGlobalSettings = null;
        private TableSettings _curTableSettings = null;
        private bool _initialLoading = true;


        // IEnumerable<CheckListBox> _roleCheckListBoxes;


        public ucSettings()
        {
            InitializeComponent();

            //_roleCheckListBoxes = new CheckListBox[]
            //{
            //    lstRolesForSqlInsert,
            //    lstRolesForSqlBulkInsert,
            //    lstRolesForSqlUpdate,
            //    lstRolesForSqlDelete,
            //    lstRolesForSqlSelectAll,
            //    lstRolesForSqlSelectByPK,
            //    lstRolesForSqlSelectByUK,
            //    lstRolesForSqlTableType
            //};
        }

        /// <summary>
        /// Init if global settings component
        /// </summary>
        public void InitGlobalSettings()
        {
            _isGlobalSettings = true;
            _initialLoading = true;

            _parent = (MainWindow)Application.Current.MainWindow;
            _curGlobalSettings = _parent.Settings.GlobalSettings;

            // Set data context using global settings
            DataContext = _curGlobalSettings;

            tabCustomDeco.Visibility = Visibility.Hidden;
            tabCustomField.Visibility = Visibility.Hidden;

            _initialLoading = false;
        }

        /// <summary>
        /// Init if table settings component (override settings at table lvl)
        /// </summary>
        /// <param name="tableName"></param>
        public void InitTableSettings(string tableName)
        {
            _isGlobalSettings = false;
            _initialLoading = true;

            _parent = (MainWindow)Application.Current.MainWindow;

            if (_parent.Settings.TablesSettings.TryGetValue(tableName, out _curTableSettings) == false)
                _curTableSettings = null;

            if (_curTableSettings == null)
            {
                _curTableSettings = _curTableSettings.CopyFromGlobalSettings(_parent.Settings.GlobalSettings);
                _curTableSettings.TableName = tableName;

                _parent.Settings.TablesSettings.Add(_curTableSettings.TableName, _curTableSettings);
            }

            // Set data context using table settings (thus, overriding global settings)
            DataContext = _curTableSettings;

            tabCustomDeco.Visibility = Visibility.Visible;
            tabCustomField.Visibility = Visibility.Visible;

            _initialLoading = false;
        }


        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }


                             
        /// <summary>
        /// Extract IEnumerable of strings (role names) from a checklist box
        /// </summary>
        /// <param name="curList"></param>
        /// <returns></returns>
        private IEnumerable<string> GetSelectedRolesFromCheckListBox(CheckListBox roleLst)
        {
            foreach (var item in roleLst.SelectedItems)
            {
                yield return ((TSqlObject)item).Name.Parts[0];
            }
        }

        /// <summary>
        /// Selects the input roles in a checklist box
        /// </summary>
        /// <param name="curList"></param>
        private void SetSelectedRolesIntoCheckListBox(CheckListBox roleLst, string[] roles)
        {
            roleLst.UnSelectAll();
            if (roles != null)
            {
                foreach (var item in roleLst.Items)
                {
                    if (roles.Any(r => r == ((TSqlObject)item).Name.Parts[0]))
                    {
                        roleLst.SelectedItems.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the check list box, selecting (checking) the roles which are granted to the SQL procedure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RolesCheckListBox_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is CheckListBox roleLst)
            {
                roleLst.ItemsSource = _parent.Roles;
                roleLst.DisplayMemberPath = "Name.Parts[0]";

                if (roleLst.DataContext is SqlGeneratorSettings sqlSettings)
                {
                    SetSelectedRolesIntoCheckListBox(roleLst, sqlSettings.GrantExecuteToRoles);
                }
            }
        }


        /// <summary>
        /// Handles when a role is added or removed from the grants for a SQL procedure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RolesCheckListBox_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            if (!_initialLoading)
            {
                if (sender is CheckListBox roleLst)
                {
                    if (roleLst.DataContext is SqlGeneratorSettings sqlSettings)
                    {
                        sqlSettings.GrantExecuteToRoles = GetSelectedRolesFromCheckListBox(roleLst).ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// Check the table type when bulk insert SP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkGenerateBulkInsertSp_Checked(object sender, RoutedEventArgs e)
        {
            chkGenerateTableType.IsChecked = true;
        }

        /// <summary>
        /// Uncheck the table type when bulk insert SP unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkGenerateBulkInsertSp_Unchecked(object sender, RoutedEventArgs e)
        {
            chkGenerateTableType.IsChecked = false;
        }
    }
}
