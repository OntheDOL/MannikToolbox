﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MannikToolbox.Controls;
using MannikToolbox.Services;

namespace MannikToolbox
{
    public partial class MainForm : Form
    {
	    public MainForm()
        {
			InitializeComponent();
        }

		private void MainForm_Load(object sender, EventArgs e)
		{
            // do not open app until db connection is valid
		    bool isDbConnected = false;
		    while (!isDbConnected)
		    {
		        MySqlConnectionStringBuilder sb = ConnectionStringService.ConnectionString;
		        MySqlConnection testConnection = new MySqlConnection(sb.ConnectionString);

		        try
		        {
		            testConnection.Open();
		        }
		        catch (MySqlException ex)
		        {
		            var response = MessageBox.Show($@"DB connection error: {ex.Message}", @"DB Connection Error", MessageBoxButtons.RetryCancel);

		            if (response == DialogResult.Cancel)
		            {
		                Application.Exit();
		                return;
		            }

		            new MySqlConfig().ShowDialog(this);
		        }

		        isDbConnected = testConnection.State == System.Data.ConnectionState.Open;
		    }

            var loading = new LoadingForm();
            loading.Show();
		    var progress = new Progress<int>(percent =>
		    {
		        loading.ProgressBar.Value = percent;

		        var item = percent / (100 / DatabaseManager.RegisteredObjects.Length);

		        if (item < DatabaseManager.RegisteredObjects.Length)
		        {
		            var type = DatabaseManager.RegisteredObjects[item].Name;
		            loading.ProgressText.Text = $@"Loading: {type}";
		        }

		        if (percent == 100)
		        {
		            loading.Close();
		        }
		    });

            // doing this to start loading the db
            Task.Run(() =>
            {
                DatabaseManager.SetDatabaseConnection(progress);
            });

		    Text = $@"Mannik/Loki's Toolbox ({ConnectionStringService.ConnectionString.Server})";
		    LoadTabForms();
		}
        
        private void LoadTabForms()
        {
            UserControl control = new SpellControl { Dock = DockStyle.Fill };
            tabSpell.Controls.Add(control);

            control = new MobControl { Dock = DockStyle.Fill };
            tabMob.Controls.Add(control);

            control = new ServerDetailsControl { Dock = DockStyle.Fill };
            tabServerDetails.Controls.Add(control);

            control = new NpcTemplateControl { Dock = DockStyle.Fill };
            tabNPCTemplate.Controls.Add(control);

            control = new ItemTemplateControl { Dock = DockStyle.Fill };
            tabItem.Controls.Add(control);
        }

		private void Menu_DB_Click(object sender, EventArgs e)
        {
			new MySqlConfig().ShowDialog(this);
			Application.Restart();
			Environment.Exit(0);
		}

        
    }
}