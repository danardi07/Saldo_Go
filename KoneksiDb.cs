using System;
using System.IO;
using System.Windows.Forms;
namespace SaldoGo
{
    internal static class KoneksiDb
    {
        public static string koneksi = @"Data Source=DESKTOP-7NAQSTK\SQLEXPRESS; Initial Catalog=Saldo_Go;Integrated Security=True";

        public static string GetLocalIPAddress()
        {
            string localIP = string.Empty;
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting local IP address: " + ex.Message);
            }
            return localIP;
        }

        public static string GetConnectionString()
        {
            string localIP = GetLocalIPAddress();
            if (string.IsNullOrEmpty(localIP))
            {
                localIP = "localhost";
            }
            string connectionString = $"Data Source={localIP}\\SQLEXPRESS;Initial Catalog=Saldo_Go;Integrated Security=True";
            return connectionString;
        }

        public static string koneksiDinamis = GetConnectionString();

        public static string GetConnectionStringFromConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, "database.config");
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    string serverIP = "localhost";
                    string databaseName = "Saldo_Go";

                    foreach (string line in lines)
                    {
                        if (line.StartsWith("ServerIP="))
                        {
                            serverIP = line.Substring("ServerIP=".Length);
                        }
                        else if (line.StartsWith("Database="))
                        {
                            databaseName = line.Substring("Database=".Length);
                        }
                    }

                    return $"Data Source={serverIP}\\SQLEXPRESS;Initial Catalog={databaseName};Integrated Security=True";
                }
                else
                {
                    return koneksi;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading database config: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return koneksi;
            }
        }

        public static string koneksiConfig = GetConnectionStringFromConfig();
    }
}
