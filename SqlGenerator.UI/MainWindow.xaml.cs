﻿using Microsoft.SqlServer.Dac.Model;
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

        public MainWindow()
        {
            InitializeComponent();
        }


        private async void ButtonLoadModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonLoadModel.IsEnabled = false;
                if(txtPath.Text ==string.Empty)
                {
                    Model = await Task.Run(() => TSqlModelHelper.LoadModel("PensionLab.DB.dacpac"));
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


        private void ButtonShowTables_Click(object sender, RoutedEventArgs e)
        {
            var tables = Model.GetAllTables();
            lstTables.ItemsSource = tables;
            lstTables.DisplayMemberPath = "Name.Parts[1]";
        }

        private void ButtonGenerateDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                SqlDeleteGenerator gen = new SqlDeleteGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                SqlInsertGenerator gen = new SqlInsertGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }


        private void ButtonGenerateBulkInsert_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                SqlBulkInsertGenerator gen = new SqlBulkInsertGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateNetEntityClass_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                CsEntityClassGenerator gen = new CsEntityClassGenerator(table, classNamespace: "PensionLab.DTO");
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                var gen = new SqlSelectAllGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectByPK_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                var gen = new SqlSelectByPKGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateSelectByUK_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                var gen = new SqlSelectByUKGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });
                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                var gen = new SqlUpdateGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" }
                , doNotUpdateColumns: new string[] { "inserted_by", "inserted_on" });

                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }

        private void ButtonGenerateTableType_Click(object sender, RoutedEventArgs e)
        {
            if (lstTables.SelectedItem is TSqlObject table)
            {
                var gen = new SqlTableTypeGenerator(table, grantExecuteTo: new string[] { "role_admin", "role_user" });

                var output = gen.Generate();
                txtOutput.Text = output;
            }
        }


        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                txtPath.Text = openFileDialog.FileName;
        }

    }
}
