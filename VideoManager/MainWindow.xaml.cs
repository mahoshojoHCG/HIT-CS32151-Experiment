using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using FFmpeg.NET;
using Newtonsoft.Json;
using Image = System.Windows.Controls.Image;

namespace VideoManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Dictionary<int, Video> Videos;

        private Dictionary<int, string> CatList;
        private Dictionary<int, string> TagList;


        private Dictionary<int, List<int>> SortByCat;
        private Dictionary<int, List<int>> SortByTag;

        private Dictionary<int, string> Images;


        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                ImageSource newSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(handle);
                return newSource;
            }
            catch
            {
                DeleteObject(handle);
                return null;
            }
        }

        private Tuple<Canvas, Image, TextBlock> GetImageWithText(ImageSource source, string text)
        {
            var can = new Canvas { Width = 240, Height = 180 };
            var img = new Image { Source = source, Width = 220, Height = 160 };
            var txt = new TextBlock
            {
                Text = text,
                Width = 220,
                TextWrapping = TextWrapping.NoWrap,
                TextAlignment = TextAlignment.Center
            };
            can.Children.Add(img);
            can.Children.Add(txt);
            img.MouseMove += Img_MouseMove;
            img.MouseLeave += ImgOnMouseLeave;
            Canvas.SetTop(can.Children[0], 0);
            Canvas.SetBottom(can.Children[1], 0);
            return new Tuple<Canvas, Image, TextBlock>(can, img, txt);
        }

        private void ImgOnMouseLeave(object sender, MouseEventArgs e)
        {
            var img = sender as Image;
            Debug.Assert(img != null, nameof(img) + " != null");
            img.Effect = null;
        }

        private void Img_MouseMove(object sender, MouseEventArgs e)
        {
            var img = sender as Image;
            Debug.Assert(img != null, nameof(img) + " != null");
            img.Effect = new BlurEffect();
        }

        private async Task<string> GetOverview(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var thumbnail = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                $"Video Manager\\temp\\{Guid.NewGuid()}.jpg");
            var input = new MediaFile(path);
            var output = new MediaFile(thumbnail);
            var ffmpeg = new Engine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Video Manager\\ffmpeg.exe"));
            var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(10) };
            await ffmpeg.GetThumbnailAsync(input, output, options);
            return thumbnail;
        }


        private async Task LoadCats()
        {
            CatList = new Dictionary<int, string>();
            CatList.Add(0, "未分类");
            SortByCat.Add(0, new List<int>());
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM dbo.Cat", connection);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                SortByCat.Add((int)reader[0], new List<int>());
                CatList.Add((int)reader[0], reader[1] as string);
            }

            connection.Close();
        }

        private async Task LoadTags()
        {
            TagList = new Dictionary<int, string>();
            TagList.Add(0, "未分类");
            SortByTag.Add(0, new List<int>());
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM dbo.Tag", connection);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                SortByTag.Add((int)reader[0], new List<int>());
                TagList.Add((int)reader[0], reader[1] as string);
            }
            connection.Close();
        }

        private async Task LoadVideo()
        {
            Videos = new Dictionary<int, Video>();
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM dbo.Videos", connection);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var vid = new Video
                {
                    FilePath = reader["FilePath"] as string,
                    Id = (int)reader["VideosId"],
                    VideoCat = (int)reader["VideoCat"],
                    VideoTag = (int)reader["VideoTag"],
                    VideoName = reader["VideoName"] as string
                };
                Videos.Add(vid.Id, vid);
            }
            connection.Close();
        }

        private void LoadCatUI()
        {
            //对于每一个分类，添加对应的界面
            CatTab.Items.Clear();
            foreach (var pair in CatList)
            {
                var scrollViewer = new ScrollViewer();
                var panel = new WrapPanel();
                foreach (var vid in SortByCat[pair.Key])
                {
                    panel.Children.Add(GetImageWithText(new BitmapImage(new Uri(Images[vid])), Videos[vid].VideoName).Item1);
                }
                scrollViewer.Content = panel;
                var item = new TabItem { Header = pair.Value, Content = scrollViewer };
                CatTab.Items.Add(item);
            }
        }

        private void LoadTagUI()
        {

        }

        private async Task LoadAllImages()
        {
            var tasks = new Task[Videos.Count];
            int i = 0;
            foreach (var video in Videos.Values)
            {
                tasks[i] = GetOneImage(video);
                ++i;
            }

            foreach (var task in tasks)
            {
                await task;
            }
            
        }

        private async Task GetOneImage(Video video)
        {
            Images.Add(video.Id, await GetOverview(video.FilePath));
        }

        private async Task LoadAll()
        {
            SortByCat = new Dictionary<int, List<int>>();
            SortByTag = new Dictionary<int, List<int>>();
            Images = new Dictionary<int, string>();

            var task1 = LoadCats();
            var task2 = LoadTags();
            var task3 = LoadVideo();

            await task3;
            await LoadAllImages();
            await task1;
            await task2;



            //在全部界面添加界面
            foreach (var video in Videos.Values)
            {
                var add = GetImageWithText(new BitmapImage(new Uri(Images[video.Id])), video.VideoName);
                SortByCat[video.VideoCat].Add(video.Id);
                SortByTag[video.VideoTag].Add(video.Id);
                AllWarpPanel.Children.Add(add.Item1);
            }

            LoadCatUI();


        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            CopyrightBox.Text = string.Format(TextResources.Copyright, Assembly.GetExecutingAssembly().GetName().Version);

            await LoadAll();
        }

        private void ExternPlayBox_OnClick(object sender, RoutedEventArgs e)
        {
            if (ExternPlayBox.IsChecked != null)
                App.Config.ExternalPlay = (bool)ExternPlayBox.IsChecked;
            File.WriteAllText("config.json", JsonConvert.SerializeObject(App.Config, Formatting.Indented));
        }

        private async void AllWarpPanel_OnDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] files)) return;
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            foreach (var file in files)
            {
                //检查是否支持文件类型
                var exts = Resource.SupportedFile.Split('|');
                if (!exts.Contains(Path.GetExtension(file)))
                {
                    MessageBox.Show($"导入视频{file}失败，不支持的拓展名{Path.GetExtension(file)}。", "导入失败", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    continue;
                }

                //检测是否已将视频导入
                var fetchExists =
                    new SqlCommand("SELECT * FROM [dbo].[Videos] WHERE [FilePath] = @FilePath", connection);
                fetchExists.Parameters.Add("@FilePath", SqlDbType.NVarChar).Value = file;
                if (await fetchExists.ExecuteScalarAsync() != null)
                {
                    MessageBox.Show($"导入视频{file}失败，已经导入这个视频了。", "导入失败", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    continue;
                }

                //获取略缩图
                var thumbnail = GetOverview(file);

                //视频信息插入数据库
                var insertVideo =
                    new SqlCommand(
                        "INSERT INTO [dbo].[Videos] ([FilePath], [VideoName], [VideoTag], [VideoCat]) VALUES (@FilePath, @VideoName, @VideoTag, @VideoCat)",
                        connection);
                insertVideo.Parameters.Add("@FilePath", SqlDbType.NVarChar).Value = file;
                insertVideo.Parameters.Add("@VideoName", SqlDbType.NVarChar).Value = file.Split('\\').Last();
                insertVideo.Parameters.Add("@VideoCat", SqlDbType.Int).Value = 0;
                insertVideo.Parameters.Add("@VideoTag", SqlDbType.Int).Value = 0;
                await insertVideo.ExecuteNonQueryAsync();

                var video = new Video();

                //重新获取信息
                var newId = fetchExists.ExecuteScalarAsync();

                //设置属性
                video.FilePath = file;
                video.VideoName = Path.GetFileName(file);
                video.Id = (int)await newId;
                video.VideoCat = 0;
                video.VideoTag = 0;

                //获取略缩图
                ImageSource source = new BitmapImage(new Uri(await thumbnail));

                var added = GetImageWithText(source, video.VideoName);
                //添加控件
                AllWarpPanel.Children.Add(added.Item1);
                Images.Add(video.Id, await thumbnail);
                LoadCatUI();
                LoadTagUI();
            }
        }
    }
}
