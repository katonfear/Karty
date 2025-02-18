using Karty.Model;
using Microsoft.Data.SqlClient;
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
using System.Windows.Shapes;

namespace Karty
{
    /// <summary>
    /// Interaction logic for BazaDanych.xaml
    /// </summary>
    public partial class BazaDanych : Window
    {
        public bool WasSaved { get; set; } = false;
        public BazaDanych()
        {
            InitializeComponent();
            if (DbConnection.Configuration != null && !string.IsNullOrWhiteSpace(DbConnection.Configuration.Password))
            {
                DbConnection.Configuration.CopyCurrentVersion();
                password.Password = DbConnection.Configuration.Password;
            }
        }

        private void user_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(user.Text))
            {
                if (!user.IsFocused)
                {
                    HideShow(user, true);
                }
            }
            else 
            {
                HideShow(user, false);
            }
        }


        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(password.Password))
            {
                if (!password.IsFocused)
                {
                    HideShow(password, true);
                }
            }
            else
            {
                HideShow(password, false);
            }
        }

        private void server_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(server.Text))
            {
                if (!server.IsFocused)
                {
                    HideShow(server, true);
                }
            }
            else
            {
                HideShow(server, false);
            }
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(password.Password) && !string.IsNullOrWhiteSpace(user.Text) && !string.IsNullOrWhiteSpace(server.Text)) 
            {
                DbConnection.Configuration = new DbConfiguration();
                DbConnection.Configuration.Server = server.Text;
                DbConnection.Configuration.User = user.Text;
                DbConnection.Configuration.Password = password.Password;
                try
                {
                    SqlConnection sqlConnection = DbConnection.GetConnection();
                    sqlConnection.Close();
                    MessageBox.Show(this, "Połączono.");
                }
                catch (Exception ex) 
                {
                    MessageBox.Show(this, ex.Message);
                }
            }
        }

        private void zapisz_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                DbConnection.Configuration = new DbConfiguration();
                DbConnection.Configuration.Server = server.Text;
                DbConnection.Configuration.User = user.Text;
                DbConnection.Configuration.Password = password.Password;
                byte[]? data = PayCard.EncryptPin(DbConnection.Serialize());
                if (data != null)
                {
                    System.IO.File.WriteAllBytes(DbConnection.FileName, data);
                    MessageBox.Show(this,"Plik został zapisany.");
                    WasSaved = true;
                    this.Close();
                }
                else 
                {
                    MessageBox.Show(this, "Błąd zapisu danych.");
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void anuluj_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            HideShow(sender, false);
        }

        private void LostFocus(object sender, RoutedEventArgs e)
        {
            HideShow(sender, true);
        }

        private void HideShow(object sender, bool isLostFocuse) 
        {
            string? name = (sender as dynamic).Name as string;
            if (isLostFocuse)
            {
                switch (name)
                {
                    case "user":
                        if (string.IsNullOrEmpty(user.Text))
                        {
                            userBlock.Visibility = Visibility.Visible;
                        }
                        break;
                    case "password":
                        if (string.IsNullOrEmpty(password.Password))
                        {
                            passwordBlock.Visibility = Visibility.Visible;
                        }
                        break;
                    case "server":
                        if (string.IsNullOrEmpty(server.Text))
                        {
                            serverBlock.Visibility = Visibility.Visible;
                        }
                        break;
                }
            }
            else 
            {
                switch (name)
                {
                    case "user":
                        userBlock.Visibility = Visibility.Collapsed;
                        break;
                    case "password":
                        passwordBlock.Visibility = Visibility.Collapsed;
                        break;
                    case "server":
                        serverBlock.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        private void init_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string db = "CREATE DATABASE [Apator];";
                string[] scripts = {
                    @"CREATE TABLE [dbo].[PayCard](
	[CardId] [nvarchar](128) NULL,
	[AccountNumber] [nvarchar](26) NULL,
	[SerialNumber] [nvarchar](30) NULL,
	[Pin] [varbinary](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];",
                    @"CREATE PROCEDURE [dbo].[ADD_CARDS] 
	                        @Account nvarchar(26)
	                        ,@SerialNumber nvarchar(30)
	                        ,@CardId nvarchar(128)
	                        ,@Pin varbinary(max)
                        AS
                        BEGIN
	                        SET NOCOUNT ON;
	                        INSERT INTO PayCard
	                        (
		                        CardId
		                        ,AccountNumber 
		                        ,SerialNumber
		                        ,Pin
	                        )
	                        SELECT 
		                        @CardId
		                        ,@Account
		                        ,@SerialNumber
		                        ,@Pin
                        END",
                    @"CREATE PROCEDURE [dbo].[GET_CARDS] 
	                    @Account nvarchar(26)
	                    ,@SerialNumber nvarchar(30)
	                    ,@CardId nvarchar(128)
                    AS
                    BEGIN
	                    SET NOCOUNT ON;
                    
	                    SELECT 
		                    CardId
		                    ,AccountNumber
		                    ,SerialNumber
		                    ,Pin
	                    FROM
		                    PayCard
	                    WHERE
		                    AccountNumber = @Account
		                    or SerialNumber = @SerialNumber
		                    or CardId = @CardId
		                    or (@Account = '' and @SerialNumber = '' and @CardId = '')
                    END",
                    @"CREATE PROCEDURE [dbo].[REMOVE_CARDS]
	                    @CardId nvarchar(128)
                    AS
                    BEGIN
	                    SET NOCOUNT ON;
                    
	                    DELETE 
	                    FROM
		                    PayCard
	                    WHERE
		                    CardId = @CardId;
                    END"
                };

                DbConnection.Configuration = new DbConfiguration();
                DbConnection.Configuration.Server = server.Text;
                DbConnection.Configuration.User = user.Text;
                DbConnection.Configuration.Password = password.Password;

                bool isError = false;

                using (SqlConnection conn = DbConnection.GetConnectionWithoutDb())
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = db;
                        cmd.ExecuteNonQuery();
                    }
                }
                using (SqlConnection conn = DbConnection.GetConnection())
                {
                    foreach (var script in scripts)
                    {
                        try
                        {

                            using (SqlCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = script;
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            isError = true;
                            MessageBox.Show(this, ex.Message);
                        }
                    }
                }

                if (!isError) 
                {
                    MessageBox.Show(this, "Dodano bazę danych.");
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show(this, ex.Message);
            }
        }
    }
}
