using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
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
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace VideoManager
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private async Task InitDatabase()
        {
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            await new SqlCommand(SqlResources.CreateCatTable, connection).ExecuteNonQueryAsync();
            await new SqlCommand(SqlResources.CreateTagTable, connection).ExecuteNonQueryAsync();
            //await new SqlCommand(SqlResources.CreateUserTable, connection).ExecuteNonQueryAsync();
            await new SqlCommand(SqlResources.CreateVideosTale, connection).ExecuteNonQueryAsync();
            connection.Close();
        }

        private async void Login_OnLoaded(object sender, RoutedEventArgs e)
        {
            var appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Video Manager");
            var tempDir = Path.Combine(appDataDir, "temp");
            Directory.CreateDirectory(appDataDir);
            Directory.CreateDirectory(tempDir);
            File.WriteAllBytes(Path.Combine(appDataDir,"ffmpeg.exe"),Resource.ffmpeg);
            await Task.Delay(1);
            IsEnabled = false;
            while (!File.Exists("config.json"))
            {
                App.Config = new Config();
                new InputSQLString().ShowDialog();
                if (!string.IsNullOrWhiteSpace(App.Config.SqlConnectionString))
                {
                    File.WriteAllText("config.json",JsonConvert.SerializeObject(App.Config,Formatting.Indented));
                    await InitDatabase();
                }
            }

            App.Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            Hide();
            new MainWindow().Show();
            IsEnabled = true;
        }
    }
}
