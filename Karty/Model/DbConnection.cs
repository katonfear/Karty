using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Karty.Model
{
    public static class DbConnection
    {
        public static DbConfiguration? Configuration { get; set; } = new DbConfiguration();
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
                    if (string.IsNullOrWhiteSpace(SqlCon.ConnectionString))
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

    public class DbConfiguration
    {
        [JsonProperty("user", Required = Required.Always)]
        public string User { get; set; } = string.Empty;
        [JsonProperty("password", Required = Required.Always)]
        public string Password { get; set; } = string.Empty;
        [JsonProperty("server", Required = Required.Always)]
        public string Server { get; set; } = string.Empty;
        [JsonProperty("database", Required = Required.Always)]
        public string DataBase { get; set; } = "Apator";
    }
}
