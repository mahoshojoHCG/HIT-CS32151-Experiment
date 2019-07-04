using System.Windows;

namespace VideoManager
{
    /// <summary>
    ///     InputSQLString.xaml 的交互逻辑
    /// </summary>
    public partial class InputName : Window
    {
        public InputName()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(input.Text)) Close();
        }
    }
}