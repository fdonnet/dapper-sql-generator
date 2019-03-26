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
        private MainWindow _parent;
        private bool _isGlobalSettings = true;
        private Settings _curGlobalSettings = null;
        private TableSettings _curTableSettings = null;
        private bool _initialLoading = true;


        public ucSettings()
        {
            InitializeComponent();
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

            DataContext = _curGlobalSettings;
            LoadRolesLists();
            LoadActualSettings();

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

            LoadRolesLists();
            DataContext = _curTableSettings;
            LoadActualSettings();

            tabCustomDeco.Visibility = Visibility.Visible;
            tabCustomField.Visibility = Visibility.Visible;

            _initialLoading = false;
        }

        /// <summary>
        /// Load all posibilities to give GRANT execute
        /// </summary>
        private void LoadRolesLists()
        {
            lstRolesForDeleteSp.ItemsSource = _parent.Roles;
            lstRolesForDeleteSp.DisplayMemberPath = "Name.Parts[0]";

            lstRolesForInsertSp.ItemsSource = _parent.Roles;
            lstRolesForInsertSp.DisplayMemberPath = "Name.Parts[0]";
        }

        /// <summary>
        /// Load the info from the settings at the initial load od the compo
        /// </summary>
        private void LoadActualSettings()
        {
            if(_isGlobalSettings)
            {
                PutSelectedRolesInListBox(lstRolesForDeleteSp, _curGlobalSettings.SqlDeleteSettings.GrantExecuteToRoles);
            }
            else
            {
                PutSelectedRolesInListBox(lstRolesForDeleteSp, _curTableSettings.SqlDeleteSettings.GrantExecuteToRoles);
            }
        }


        /// <summary>
        /// Update grant roles list on Delete SP 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstRolesForDeleteSp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialLoading)
            {
                var selectedRoles = ExtractSelectedRolesInListBox(lstRolesForDeleteSp);

                if (_isGlobalSettings)
                    _curGlobalSettings.SqlDeleteSettings.GrantExecuteToRoles =
                        (selectedRoles.Count() == 0) ? null : selectedRoles.ToArray();

                else
                    _curTableSettings.SqlDeleteSettings.GrantExecuteToRoles =
                        (selectedRoles.Count() == 0) ? null : selectedRoles.ToArray();
            }
       
        }



        /// <summary>
        /// Extract IEnumerable og string (role names) from listbox
        /// </summary>
        /// <param name="curList"></param>
        /// <returns></returns>
        private IEnumerable<string> ExtractSelectedRolesInListBox(ListBox curList)
        {
            foreach (var item in curList.SelectedItems)
            {
                yield return ((TSqlObject)item).Name.Parts[0];
            }
        }

        /// <summary>
        /// Put the selected roles in roles listbox based on settings
        /// </summary>
        /// <param name="curList"></param>
        private void PutSelectedRolesInListBox(ListBox curList, string[] roles)
        {
            curList.UnselectAll();
            if (roles !=null)
            {
                foreach (var item in curList.Items)
                {
                    if (roles.Any(r => r == ((TSqlObject)item).Name.Parts[0]))
                    {
                        curList.SelectedItems.Add(item);
                    }
                }
            }
        }
    }
}
