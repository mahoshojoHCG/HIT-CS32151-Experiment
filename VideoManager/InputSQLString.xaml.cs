using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace VideoManager
{
    /// <summary>
    /// InputSQLString.xaml 的交互逻辑
    /// </summary>
    public partial class InputSQLString : Window
    {
        public InputSQLString()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //测试连接
                var connection = new SqlConnection(input.Text);
                await connection.OpenAsync();
                connection.Close();
                App.Config.SqlConnectionString = input.Text;
                Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"无法连接到数据库，似乎问题是{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
