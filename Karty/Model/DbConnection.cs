using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Karty.Model
{
    public static class DbConnection
    {
        public static DbConfiguration? Configuration { get; set; } = 
            new DbConfiguration();
        public static SqlConnection? SqlCon { get; set; } = null;

        public static string FileName { get; set; } = "json.config";

        public static SqlConnection GetConnection() 
        {
            if (Configuration != null)
            {
                if (SqlCon == null)
                {
                    SqlCon = new SqlConnection(new SqlConnectionStringBuilder() { DataSource = Configuration.Server, UserID = Configuration.User, Password = Configuration.Password, InitialCatalog = Configuration.DataBase, TrustServerCertificate = true }.ConnectionString);
                    SqlCon.Open();
                    return SqlCon;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(SqlCon.ConnectionString.ToString()) || string.IsNullOrWhiteSpace(SqlCon.DataSource) || string.IsNullOrWhiteSpace(SqlCon.Database))
                    {
                        SqlCon.ConnectionString = new SqlConnectionStringBuilder() { DataSource = Configuration.Server, UserID = Configuration.User, Password = Configuration.Password, InitialCatalog = Configuration.DataBase, TrustServerCertificate = true }.ConnectionString;
                    }
                    if (SqlCon.State == System.Data.ConnectionState.Broken || SqlCon.State == System.Data.ConnectionState.Closed)
                    {
                        SqlCon.Open();
                    }
                    return SqlCon;
                }
            }
            else
            {
                throw new Exception("Brak konfiguracji.");
            }
        }

        public static SqlConnection GetConnectionWithoutDb()
        {
            if (Configuration != null)
            {
                SqlCon = new SqlConnection(new SqlConnectionStringBuilder() { DataSource = Configuration.Server, UserID = Configuration.User, Password = Configuration.Password, InitialCatalog = "master", TrustServerCertificate = true }.ConnectionString);
                SqlCon.Open();
                return SqlCon;
            }
            else 
            {
                throw new Exception("Brak konfiguracji.");
            }
        }

        public static void CheckConnection() 
        {
            GetConnection();
        }

        public static string Serialize() 
        {
            return JsonConvert.SerializeObject(Configuration);
        }

        public static void Deserialize(string @value) 
        {
            Configuration = JsonConvert.DeserializeObject<DbConfiguration>(@value);
        }

    }

    public class DbConfiguration : INotifyPropertyChanged
    {
        [JsonProperty("user", Required = Required.Always)]
        public string User { get { 
                return _user; } set { _user = value; OnPropertyChanged("User"); } }
        [JsonIgnore]
        private string _user = string.Empty;
        [JsonProperty("password", Required = Required.Always)]
        public string Password { get { return _password; } set { _password = value; OnPropertyChanged("Password"); } }
        private string _password = string.Empty;
        [JsonProperty("server", Required = Required.Always)]
        public string Server { get { return _server; } set { _server = value; OnPropertyChanged("Server"); } }
        private string _server = string.Empty;
        [JsonProperty("database", Required = Required.Always)]
        public string DataBase { get { return _database; } set { _database = value; OnPropertyChanged("DataBase"); } }
        private string _database = "Apator";

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
