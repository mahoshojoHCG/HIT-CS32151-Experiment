using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using FFmpeg.NET;

namespace VideoManager
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MenuItem catItem;

        private Dictionary<int, string> CatList;

        private Dictionary<int, string> Images;

        private ContextMenu imgContextMenu;


        private Dictionary<int, List<int>> SortByCat;
        private Dictionary<int, List<int>> SortByTag;
        private MenuItem tagItem;
        private Dictionary<int, string> TagList;

        private Dictionary<int, Video> Videos;

        public MainWindow()
        {
            InitializeComponent();
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
                video.Id = (int) await newId;
                video.VideoCat = 0;
                video.VideoTag = 0;

                Videos.Add(video.Id, video);

                //获取略缩图
                ImageSource source = new BitmapImage(new Uri(await thumbnail));

                var added = GetImageWithText(source, video.VideoName, video.Id);
                //添加控件
                AllWarpPanel.Children.Add(added);
                Images.Add(video.Id, await thumbnail);
                LoadCatUI();
                LoadTagUI();
            }
        }

        private async void CatItemClick(object sender, RoutedEventArgs e)
        {
            var img = ContextMenuService.GetPlacementTarget(
                ((sender as MenuItem).Parent as MenuItem).Parent) as VideoImage;
            var cat = (sender as MenuItem).Header as string;

            //分类相同无需切换
            if (CatList[Videos[img.VideoId].VideoCat] == cat)
                return;
            if (cat == "新分类")
            {
                var inputName = new InputName();
                inputName.Title = "输入分类名";
                inputName.ShowDialog();
                await CreateNewCat(inputName.input.Text);
                await LoadAll();
                return;
            }

            var catId = CatList.Where(p => p.Value == cat).Select(p => p.Key).FirstOrDefault();
            await SetVideoCat(img.VideoId, catId);
            await LoadAll();

            //(CatTab.Items[0] as TabItem).Focus();
        }

        private async void CatTabDrop(object sender, DragEventArgs e)
        {
            var text = ((sender as ScrollViewer).Parent as TabItem).Header as string;
            var catId = CatList.Where(p => p.Value == text).Select(p => p.Key).FirstOrDefault();

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
                insertVideo.Parameters.Add("@VideoCat", SqlDbType.Int).Value = catId;
                insertVideo.Parameters.Add("@VideoTag", SqlDbType.Int).Value = 0;
                await insertVideo.ExecuteNonQueryAsync();

                var video = new Video();

                //重新获取信息
                var newId = fetchExists.ExecuteScalarAsync();

                //设置属性
                video.FilePath = file;
                video.VideoName = Path.GetFileName(file);
                video.Id = (int) await newId;
                video.VideoCat = catId;
                video.VideoTag = 0;

                Videos.Add(video.Id, video);

                //获取略缩图
                ImageSource source = new BitmapImage(new Uri(await thumbnail));

                var added = GetImageWithText(source, video.VideoName, video.Id);
                //添加控件
                AllWarpPanel.Children.Add(added);
                Images.Add(video.Id, await thumbnail);
                LoadCatUI();
                LoadTagUI();
            }
        }

        private async Task<int> CreateNewCat(string catName)
        {
            if (string.IsNullOrEmpty(catName) || catName == "未分类" || catName == "新分类")
            {
                MessageBox.Show("分类名称中含有非法字符！", "分类创建失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }

            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();

            //检测是否存在同名分类
            var fetchInfo = new SqlCommand("SELECT * FROM [dbo].[Cat] WHERE CatName = @CatName", connection);
            fetchInfo.Parameters.Add("@CatName", SqlDbType.NVarChar).Value = catName;
            if (await fetchInfo.ExecuteScalarAsync() != null)
            {
                MessageBox.Show("已经创建了此分类！", "分类创建失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }

            var command = new SqlCommand("INSERT INTO [dbo].[Cat] ([CatName]) Values (@CatName)", connection);
            command.Parameters.Add("@CatName", SqlDbType.NVarChar).Value = catName;
            await command.ExecuteNonQueryAsync();

            return (int) await fetchInfo.ExecuteScalarAsync();
        }

        private async Task<int> CreateNewTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName) || tagName == "无标签" || tagName == "新标签")
            {
                MessageBox.Show("分类名称中含有非法字符！", "分类创建失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }

            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();

            //检测是否存在同名分类
            var fetchInfo = new SqlCommand("SELECT * FROM [dbo].[Tag] WHERE TagName = @TagName", connection);
            fetchInfo.Parameters.Add("@TagName", SqlDbType.NVarChar).Value = tagName;
            if (await fetchInfo.ExecuteScalarAsync() != null)
            {
                MessageBox.Show("已经创建了此标签！", "标签创建失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }

            var command = new SqlCommand("INSERT INTO [dbo].[Tag] ([TagName]) Values (@TagName)", connection);
            command.Parameters.Add("@TagName", SqlDbType.NVarChar).Value = tagName;
            await command.ExecuteNonQueryAsync();

            return (int) await fetchInfo.ExecuteScalarAsync();
        }


        private Canvas GetImageWithText(ImageSource source, string text, int videoId)
        {
            var can = new Canvas {Width = 240, Height = 180};
            var img = new VideoImage {Source = source, Width = 220, Height = 160, VideoId = videoId};
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
            img.MouseDown += (sender, e) =>
            {
                if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2) PlayVideo(videoId);
            };
            img.ContextMenu = imgContextMenu;


            Canvas.SetTop(can.Children[0], 0);
            Canvas.SetBottom(can.Children[1], 0);
            return can;
        }

        private async Task GetOneImage(Video video)
        {
            Images.Add(video.Id, await GetOverview(video.FilePath));
        }

        private async Task<string> GetOverview(string path)
        {
            if (!File.Exists(path)) return null;

            var thumbnail = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                $"Video Manager\\temp\\{Guid.NewGuid()}.jpg");
            var input = new MediaFile(path);
            var output = new MediaFile(thumbnail);
            var ffmpeg = new Engine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Video Manager\\ffmpeg.exe"));
            var options = new ConversionOptions {Seek = TimeSpan.FromSeconds(10)};
            await ffmpeg.GetThumbnailAsync(input, output, options);
            return thumbnail;
        }

        private void Img_MouseMove(object sender, MouseEventArgs e)
        {
            var img = sender as VideoImage;
            Debug.Assert(img != null, nameof(img) + " != null");
            img.Effect = new BlurEffect();
        }

        private void ImgOnMouseLeave(object sender, MouseEventArgs e)
        {
            var img = sender as VideoImage;
            Debug.Assert(img != null, nameof(img) + " != null");
            img.Effect = null;
        }

        private async Task LoadAll()
        {
            SortByCat = new Dictionary<int, List<int>>();
            SortByTag = new Dictionary<int, List<int>>();
            Images = new Dictionary<int, string>();
            catItem = new MenuItem {Header = "分类"};
            tagItem = new MenuItem {Header = "标签"};

            var task1 = LoadCats();
            var task2 = LoadTags();
            var task3 = LoadVideo();

            await task3;
            await LoadAllImages();
            await task1;
            await task2;

            LoadImgContextMenu();

            AllWarpPanel.Children.Clear();
            //在全部界面添加界面
            foreach (var video in Videos.Values)
            {
                var add = GetImageWithText(new BitmapImage(new Uri(Images[video.Id])), video.VideoName, video.Id);
                AllWarpPanel.Children.Add(add);
            }

            LoadCatUI();
            LoadTagUI();
        }

        private async Task LoadAllImages()
        {
            var tasks = new Task[Videos.Count];
            var i = 0;
            foreach (var video in Videos.Values)
            {
                tasks[i] = GetOneImage(video);
                ++i;
            }

            foreach (var task in tasks) await task;
        }


        private async Task LoadCats()
        {
            catItem = new MenuItem {Header = "分类"};
            CatList = new Dictionary<int, string>();
            CatList.Add(0, "未分类");
            SortByCat = new Dictionary<int, List<int>>();
            SortByCat.Add(0, new List<int>());
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM dbo.Cat", connection);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                SortByCat.Add((int) reader[0], new List<int>());
                CatList.Add((int) reader[0], reader[1] as string);
            }

            connection.Close();

            foreach (var value in CatList.Values)
            {
                var menuItem = new MenuItem {Header = value};
                menuItem.Click += CatItemClick;
                catItem.Items.Add(menuItem);
            }

            var addNewCat = new MenuItem {Header = "新分类"};
            addNewCat.Click += CatItemClick;
            catItem.Items.Add(addNewCat);
        }

        private void LoadCatUI()
        {
            //预载
            foreach (var video in Videos.Values) SortByCat[video.VideoCat].Add(video.Id);

            //对于每一个分类，添加对应的界面
            CatTab.Items.Clear();
            foreach (var pair in CatList)
            {
                var scrollViewer = new ScrollViewer();
                var panel = new WrapPanel();
                foreach (var vid in SortByCat[pair.Key])
                    panel.Children.Add(GetImageWithText(new BitmapImage(new Uri(Images[vid])), Videos[vid].VideoName,
                        vid));
                scrollViewer.Content = panel;
                scrollViewer.AllowDrop = true;
                scrollViewer.Drop += CatTabDrop;
                var item = new TabItem {Header = pair.Value, Content = scrollViewer};
                CatTab.Items.Add(item);
            }
        }

        private void LoadImgContextMenu()
        {
            imgContextMenu.StaysOpen = true;
            var menuItem = new MenuItem {Header = "播放"};
            menuItem.Click += PlayVideoClick;
            imgContextMenu.Items.Clear();
            imgContextMenu.Items.Add(menuItem);
            imgContextMenu.Items.Add(catItem);
            imgContextMenu.Items.Add(tagItem);
        }

        private async Task LoadTags()
        {
            tagItem = new MenuItem {Header = "标签"};
            TagList = new Dictionary<int, string>();
            TagList.Add(0, "无标签");
            SortByTag = new Dictionary<int, List<int>>();
            SortByTag.Add(0, new List<int>());
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT * FROM dbo.Tag", connection);
            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                SortByTag.Add((int) reader[0], new List<int>());
                TagList.Add((int) reader[0], reader[1] as string);
            }

            connection.Close();

            foreach (var value in TagList.Values)
            {
                var menuItem = new MenuItem {Header = value};
                menuItem.Click += TagItemClick;
                tagItem.Items.Add(menuItem);
            }

            var addNewTag = new MenuItem {Header = "新标签"};
            addNewTag.Click += TagItemClick;
            tagItem.Items.Add(addNewTag);
        }


        private void LoadTagUI()
        {
            //预载
            foreach (var video in Videos.Values) SortByTag[video.VideoTag].Add(video.Id);

            //对于每一个标签，添加对应的界面
            TagTab.Items.Clear();
            foreach (var pair in TagList)
            {
                var scrollViewer = new ScrollViewer();
                var panel = new WrapPanel();
                foreach (var vid in SortByTag[pair.Key])
                    panel.Children.Add(GetImageWithText(new BitmapImage(new Uri(Images[vid])), Videos[vid].VideoName,
                        vid));
                scrollViewer.Content = panel;
                scrollViewer.AllowDrop = true;
                scrollViewer.Drop += TagTabDrop;
                var item = new TabItem {Header = pair.Value, Content = scrollViewer};
                TagTab.Items.Add(item);
            }
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
                    Id = (int) reader["VideosId"],
                    VideoCat = (int) reader["VideoCat"],
                    VideoTag = (int) reader["VideoTag"],
                    VideoName = reader["VideoName"] as string
                };
                Videos.Add(vid.Id, vid);
            }

            connection.Close();
        }


        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            imgContextMenu = new ContextMenu();
            CopyrightBox.Text =
                string.Format(TextResources.Copyright, Assembly.GetExecutingAssembly().GetName().Version);
            await LoadAll();
        }

        private void OneKeyDelete(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("本操作将清空数据库以及本地配置文件，这一操作是不可逆的，是否继续", "警告", MessageBoxButton.YesNo,
                    MessageBoxImage.Asterisk) ==
                MessageBoxResult.Yes)
            {
                var connection = new SqlConnection(App.Config.SqlConnectionString);
                connection.Open();
                new SqlCommand(" DROP TABLE [dbo].[Cat]", connection).ExecuteNonQuery();
                new SqlCommand(" DROP TABLE [dbo].[Tag]", connection).ExecuteNonQuery();
                new SqlCommand(" DROP TABLE [dbo].[Videos]", connection).ExecuteNonQuery();
                File.Delete("config.json");
                Environment.Exit(0);
            }
        }

        private void PlayVideo(int vid)
        {
            Process.Start(Videos[vid].FilePath);
        }

        private void PlayVideoClick(object sender, RoutedEventArgs e)
        {
            var img = ContextMenuService.GetPlacementTarget(
                (sender as MenuItem).Parent) as VideoImage;
            PlayVideo(img.VideoId);
        }

        private async Task SetVideoCat(int vid, int newCatId)
        {
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("UPDATE [dbo].[Videos] SET [VideoCat] = @NewCat WHERE [VideosId] = @VideoId",
                connection);
            command.Parameters.Add("@NewCat", SqlDbType.Int).Value = newCatId;
            command.Parameters.Add("@VideoId", SqlDbType.Int).Value = vid;
            await command.ExecuteNonQueryAsync();
            connection.Close();
        }

        private async Task SetVideoTag(int vid, int newTagId)
        {
            var connection = new SqlConnection(App.Config.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("UPDATE [dbo].[Videos] SET [VideoTag] = @NewTag WHERE [VideosId] = @VideoId",
                connection);
            command.Parameters.Add("@NewTag", SqlDbType.Int).Value = newTagId;
            command.Parameters.Add("@VideoId", SqlDbType.Int).Value = vid;
            await command.ExecuteNonQueryAsync();
            connection.Close();
        }

        private async void TagItemClick(object sender, RoutedEventArgs e)
        {
            var img = ContextMenuService.GetPlacementTarget(
                ((sender as MenuItem).Parent as MenuItem).Parent) as VideoImage;
            var tag = (sender as MenuItem).Header as string;

            //标签相同无需切换
            if (TagList[Videos[img.VideoId].VideoTag] == tag)
                return;
            if (tag == "新标签")
            {
                var inputName = new InputName();
                inputName.Title = "输入标签名";
                inputName.ShowDialog();
                await CreateNewTag(inputName.input.Text);
                await LoadAll();
                return;
            }

            var tagId = TagList.Where(p => p.Value == tag).Select(p => p.Key).FirstOrDefault();
            await SetVideoTag(img.VideoId, tagId);

            await LoadAll();
            //(TagTab.Items[0] as TabItem).Focus();
        }

        private async void TagTabDrop(object sender, DragEventArgs e)
        {
            var text = ((sender as ScrollViewer).Parent as TabItem).Header as string;
            var tagId = TagList.Where(p => p.Value == text).Select(p => p.Key).FirstOrDefault();

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
                insertVideo.Parameters.Add("@VideoTag", SqlDbType.Int).Value = tagId;
                insertVideo.Parameters.Add("@VideoCat", SqlDbType.Int).Value = 0;
                await insertVideo.ExecuteNonQueryAsync();

                var video = new Video();

                //重新获取信息
                var newId = fetchExists.ExecuteScalarAsync();

                //设置属性
                video.FilePath = file;
                video.VideoName = Path.GetFileName(file);
                video.Id = (int) await newId;
                video.VideoCat = 0;
                video.VideoTag = tagId;

                Videos.Add(video.Id, video);

                //获取略缩图
                ImageSource source = new BitmapImage(new Uri(await thumbnail));

                var added = GetImageWithText(source, video.VideoName, video.Id);
                //添加控件
                AllWarpPanel.Children.Add(added);
                Images.Add(video.Id, await thumbnail);
                LoadCatUI();
                LoadTagUI();
            }
        }
    }
}