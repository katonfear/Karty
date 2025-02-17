using Karty.Model;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;

namespace Karty
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<PayCard> payCards = new ObservableCollection<PayCard>();
        private HttpListener listener;
        public MainWindow()
        {
            InitializeComponent();
            cardList.ItemsSource = payCards;
        }

        public void ShowConfiguration() 
        {
            try 
            {
                BazaDanych db = new BazaDanych();
                db.Show();
                db.Closed += (a, b) => { 
                    try
                    {
                        payCards.Clear();
                        using (var conn = Karty.Model.DbConnection.GetConnection())
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.CommandText = "GET_CARDS";
                                cmd.Parameters.AddWithValue("@Account", "");
                                cmd.Parameters.AddWithValue("@SerialNumber", "");
                                cmd.Parameters.AddWithValue("@CardId", "");
                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        payCards.Add(new PayCard()
                                        {
                                            AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber"))
                                            ,
                                            Id = reader.GetString(reader.GetOrdinal("CardId"))
                                            ,
                                            SerialNumber = reader.GetString(reader.GetOrdinal("SerialNumber"))
                                            ,
                                            PinEncrypt = reader["Pin"] as byte[]
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (SqlException)
                    {
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(Karty.Model.DbConnection.FileName))
                {
                    var data = System.IO.File.ReadAllBytes(Karty.Model.DbConnection.FileName);
                    string? json = Model.PayCard.DecryptPin(data);
                    if (json != null)
                    {
                        bool isConnected = false;
                        Karty.Model.DbConnection.Deserialize(json);
                        try
                        {
                            using (var conn = Karty.Model.DbConnection.GetConnection())
                            {
                                conn.Close();
                                isConnected = true;
                            }
                        }
                        catch 
                        {
                            ShowConfiguration();
                        }
                        if (isConnected)
                        {
                            try
                            {
                                payCards.Clear();
                                using (var conn = Karty.Model.DbConnection.GetConnection())
                                {
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                        cmd.CommandText = "GET_CARDS";
                                        cmd.Parameters.AddWithValue("@Account", "");
                                        cmd.Parameters.AddWithValue("@SerialNumber", "");
                                        cmd.Parameters.AddWithValue("@CardId", "");
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                payCards.Add(new PayCard()
                                                {
                                                    AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber"))
                                                    ,
                                                    Id = reader.GetString(reader.GetOrdinal("CardId"))
                                                    ,
                                                    SerialNumber = reader.GetString(reader.GetOrdinal("SerialNumber"))
                                                    ,
                                                    PinEncrypt = reader["Pin"] as byte[]
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            catch (SqlException)
                            {
                                ShowConfiguration();
                            }
                        }
                    }
                    else
                    {
                        ShowConfiguration();
                    }
                }
                else
                {
                    ShowConfiguration();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string? GenerateGuid() 
        {
            string @return = string.Empty;
            try
            {
                string? path = Properties.Settings.Default.Path;
                string? @class = Properties.Settings.Default.Class;
                string? method = Properties.Settings.Default.Method;

                if (path != null && @class != null) 
                {
                    Assembly dll = Assembly.LoadFrom(path);
                    object? MyObj = dll.CreateInstance(@class);
                    Type? classType = dll.GetType(@class);
                    if (MyObj != null && classType != null && method != null)
                    {
                        MethodInfo? methodInfo = classType.GetMethod(method);
                        if (methodInfo != null)
                        {
                            object? result = null;
                            result = methodInfo.Invoke(MyObj, null);
                            if (result != null)
                            {
                                return result.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(this, ex.Message);
            }
            return @return;
        }

        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tbPin.Password) && !string.IsNullOrWhiteSpace(tbid.Text) && !string.IsNullOrWhiteSpace(tbaccount.Text) && !string.IsNullOrWhiteSpace(tbserial.Text)) 
                {
                    bool isAdded = false;   
                    string pin = tbPin.Password;
                    string id = tbid.Text;
                    string account = tbaccount.Text;
                    string serial = tbserial.Text;
                    var card = new PayCard()
                    {
                        AccountNumber = account,
                        Id = id,
                        SerialNumber =  serial,
                        Pin = pin
                    };

                    using (var conn = Karty.Model.DbConnection.GetConnection())
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"SELECT 
		                        Count(*)
	                        FROM
		                        PayCard
	                        WHERE
		                        CardId = @CardId;";
                            cmd.Parameters.AddWithValue("@CardId", card.Id);
                            isAdded = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                        }
                    }

                    if (!isAdded)
                    {

                        using (var conn = Karty.Model.DbConnection.GetConnection())
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.CommandText = "ADD_CARDS";
                                cmd.Parameters.AddWithValue("@Account", card.AccountNumber);
                                cmd.Parameters.AddWithValue("@SerialNumber", card.SerialNumber);
                                cmd.Parameters.AddWithValue("@CardId", card.Id);
                                cmd.Parameters.AddWithValue("@Pin", card.PinEncrypt);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else 
                    {
                        MessageBox.Show(this, "Karta o podanym identyfikatorze lub numerze seryjnym już istnieje.");
                    }

                    payCards.Add(card);
                    tbPin.Password = "";
                    tbid.Text = "";
                    tbaccount.Text = "";
                    tbserial.Text = "";
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(this, ex.Message);
            }
            
        }

        private void btGen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var guid = GenerateGuid();
                if (guid != null)
                {
                    guid = guid.Replace("-", "");
                    tbid.Text = guid;
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cardList.SelectedIndex >= 0)
                {
                    var item = cardList.SelectedItem as PayCard;
                    if (item != null) 
                    {
                        using (var conn = Karty.Model.DbConnection.GetConnection())
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                cmd.CommandText = "REMOVE_CARDS";
                                cmd.Parameters.AddWithValue("@CardId", item.Id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        cardList.SelectedIndex = -1;
                        payCards.Remove(item);
                    }
                }
                else 
                {
                    MessageBox.Show(this, "Proszę wybrać element do usunięcia.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                var text = string.IsNullOrWhiteSpace(etsearch.Text) ? "" : etsearch.Text;
                payCards.Clear();
                using (var conn = Karty.Model.DbConnection.GetConnection())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "GET_CARDS";
                        cmd.Parameters.AddWithValue("@Account", text);
                        cmd.Parameters.AddWithValue("@SerialNumber", text);
                        cmd.Parameters.AddWithValue("@CardId", text);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                payCards.Add(new PayCard()
                                {
                                    AccountNumber = reader.GetString(reader.GetOrdinal("AccountNumber"))
                                    ,
                                    Id = reader.GetString(reader.GetOrdinal("CardId"))
                                    ,
                                    SerialNumber = reader.GetString(reader.GetOrdinal("SerialNumber"))
                                    ,
                                    PinEncrypt = reader["Pin"] as byte[]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                if (!string.IsNullOrEmpty(portText.Text))
                {
                    if (listener == null)
                    {
                        listener = new HttpListener();
                        listener.Prefixes.Add(portText.Text);
                        listener.Start();
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += (a, b) => 
                        {
                            try
                            {
                                while (true) 
                                {
                                    try
                                    {
                                        HttpListenerContext ctx = listener.GetContext();

                                        // Peel out the requests and response objects
                                        HttpListenerRequest req = ctx.Request;
                                        HttpListenerResponse resp = ctx.Response;
                                        resp.ContentType = "application/json";
                                        try
                                        {
                                            if (req.HttpMethod != "POST")
                                            {
                                                string respMsg = "{\"error\":\"Obsługuję tylko POST.\"}";
                                                byte[] data = System.Text.Encoding.UTF8.GetBytes(respMsg);
                                                resp.OutputStream.Write(data, 0, data.Length);
                                                resp.OutputStream.Close();
                                            }
                                            else
                                            {
                                                var body = new StreamReader(req.InputStream).ReadToEnd();
                                                var query = JsonConvert.DeserializeObject<Card>(body);
                                                if (query != null)
                                                {
                                                    if (query.AccessCode == "1234")
                                                    {
                                                        var card = payCards.FirstOrDefault(a => a.SerialNumber == query.SerialNumber);
                                                        if (card != null)
                                                        {
                                                            var answer = JsonConvert.SerializeObject(card);
                                                            if (answer != null)
                                                            {
                                                                byte[] data = System.Text.Encoding.UTF8.GetBytes(answer);
                                                                resp.OutputStream.Write(data, 0, data.Length);
                                                                resp.OutputStream.Close();
                                                            }
                                                            else
                                                            {
                                                                string respMsg = "{\"error\":\"Brak danych.\"}";
                                                                byte[] data = System.Text.Encoding.UTF8.GetBytes(respMsg);
                                                                resp.OutputStream.Write(data, 0, data.Length);

                                                                resp.OutputStream.Close();
                                                            }
                                                        }
                                                        else
                                                        {
                                                            string respMsg = "{\"error\":\"Brak danych.\"}";
                                                            byte[] data = System.Text.Encoding.UTF8.GetBytes(respMsg);
                                                            resp.OutputStream.Write(data, 0, data.Length);
                                                            resp.OutputStream.Close();
                                                        }
                                                    }
                                                    else 
                                                    {
                                                        string respMsg = "{\"error\":\"Brak dostępu.\"}";
                                                        byte[] data = System.Text.Encoding.UTF8.GetBytes(respMsg);
                                                        resp.OutputStream.Write(data, 0, data.Length);
                                                        resp.OutputStream.Close();
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            string respMsg = "{\"error\":\"Problem z pobraniem danych.\"}";
                                            byte[] data = System.Text.Encoding.UTF8.GetBytes(respMsg);
                                            resp.OutputStream.Write(data, 0, data.Length);
                                            resp.OutputStream.Close();
                                        }
                                    }
                                    catch (Exception) 
                                    {
                                    }
                                }
                            }
                            catch (Exception ex) 
                            {
                                
                            }
                        };
                        worker.RunWorkerAsync();
                    }
                    else
                    {
                        MessageBox.Show(this, "Serwer jest już uruchomiony.");
                    }
                }
                else 
                {
                    MessageBox.Show(this, "Proszę podać adres nasłuchu serwera.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                ShowConfiguration();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
    }
}