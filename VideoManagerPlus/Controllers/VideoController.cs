using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.NET;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace VideoManagerPlus.Controllers
{
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        public VideoController(IOptions<AppSettings> settings)
        {
            AppSettings = settings.Value;
        }

        private AppSettings AppSettings { get; }

        public bool IsAllowedEdit()
        {
            if (!HttpContext.Session.TryGetValue("allowed", out var val))
                return false;
            return Encoding.Default.GetString(val) == "true";
        }

        [HttpGet("GetThumbnail")]
        public async Task<IActionResult> GetThumbnail([FromQuery] int? id)
        {
            if (id == null)
                return NotFound();
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var fetch = new SqlCommand("SELECT [FilePath] FROM [dbo].[Videos] WHERE [VideosId] = @vid", connection);
            fetch.Parameters.Add("@vid", SqlDbType.Int).Value = id;
            var path = await fetch.ExecuteScalarAsync() as string;

            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                connection.Close();
                return NotFound();
            }

            var thumbnail = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                $"Video Manager\\temp\\{Guid.NewGuid()}.jpg");
            var input = new MediaFile(path);
            var output = new MediaFile(thumbnail);
            var ffmpeg = new Engine(Path.Combine(Directory.GetCurrentDirectory(),
                "ffmpeg.exe"));
            var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(10) };
            await ffmpeg.GetThumbnailAsync(input, output, options);
            var buf = System.IO.File.ReadAllBytes(thumbnail);
            System.IO.File.Delete(thumbnail);

            connection.Close();
            return File(buf, "image/jpeg");
        }

        [HttpPost("AddTag")]
        public async Task<IActionResult> AddNewTag(string tagName)
        {
            if (!IsAllowedEdit())
                return Forbid();
            if (string.IsNullOrEmpty(tagName) || tagName == "无标签")
                return BadRequest();
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var fetch = new SqlCommand("SELECT * FROM [dbo].[Tag] WHERE [TagName] = @TagName", connection);
            fetch.Parameters.Add("@TagName", SqlDbType.NVarChar).Value = tagName;
            if (await fetch.ExecuteScalarAsync() != null)
            {
                connection.Close();
                return BadRequest();
            }

            var insert = new SqlCommand("INSERT INTO[dbo].[Tag]([TagName]) Values(@TagName)", connection);
            insert.Parameters.Add("@TagName", SqlDbType.NVarChar).Value = tagName;
            await insert.ExecuteNonQueryAsync();
            var response = JsonConvert.SerializeObject(new { tagId = await fetch.ExecuteScalarAsync() });
            connection.Close();
            return Ok(response);
        }


        [HttpPost("AddCat")]
        public async Task<IActionResult> AddNewCat(string catName)
        {
            if (!IsAllowedEdit())
                return Forbid();
            if (string.IsNullOrEmpty(catName) || catName == "未分类")
                return BadRequest();
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var fetch = new SqlCommand("SELECT * FROM [dbo].[Cat] WHERE [CatName] = @CatName", connection);
            fetch.Parameters.Add("@CatName", SqlDbType.NVarChar).Value = catName;
            if (await fetch.ExecuteScalarAsync() != null)
            {
                connection.Close();
                return BadRequest();
            }

            var insert = new SqlCommand("INSERT INTO[dbo].[Cat]([CatName]) Values(@CatName)", connection);
            insert.Parameters.Add("@CatName", SqlDbType.NVarChar).Value = catName;
            await insert.ExecuteNonQueryAsync();
            var response = JsonConvert.SerializeObject(new { catId = await fetch.ExecuteScalarAsync() });
            connection.Close();
            return Ok(response);
        }

        public async Task<List<Video>> GetAllVideos()
        {
            var returnValue = new List<Video>();
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var fetch = new SqlCommand("SELECT * FROM [dbo].[Videos]", connection);
            var reader = await fetch.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var vid = new Video
                {
                    Id = (int)reader["VideosId"],
                    VideoName = reader["VideoName"] as string,
                    FilePath = reader["FilePath"] as string,
                    VideoCat = (int)reader["VideoCat"],
                    VideoTag = (int)reader["VideoTag"]
                };
                returnValue.Add(vid);
            }

            connection.Close();
            return returnValue;
        }


        public async Task<Video> GetVideo(int? videoId)
        {
            if (videoId == null)
                return null;
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var fetch = new SqlCommand("SELECT * FROM [dbo].[Videos] WHERE [VideosId] = @vid", connection);
            fetch.Parameters.Add("@vid", SqlDbType.Int).Value = videoId;
            var reader = await fetch.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var video = new Video
                {
                    Id = (int)reader["VideosId"],
                    VideoName = reader["VideoName"] as string,
                    FilePath = reader["FilePath"] as string,
                    VideoCat = (int)reader["VideoCat"],
                    VideoTag = (int)reader["VideoTag"]
                };
                connection.Close();
                return video;
            }

            connection.Close();
            return null;
        }

        [HttpGet("GetTags")]
        public async Task<List<Tag>> GetAllTags()
        {
            var returnValue = new List<Tag>();
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var fetch = new SqlCommand("SELECT * FROM [dbo].[Tag]", connection);
            var reader = await fetch.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var tag = new Tag
                {
                    Id = (int)reader["TagId"],
                    TagName = reader["TagName"] as string
                };
                returnValue.Add(tag);
            }

            connection.Close();
            return returnValue;
        }

        [HttpGet("GetCats")]
        public async Task<List<Cat>> GetAllCats()
        {
            var returnValue = new List<Cat>();
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var fetch = new SqlCommand("SELECT * FROM [dbo].[Cat]", connection);
            var reader = await fetch.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var cat = new Cat
                {
                    Id = (int)reader["CatId"],
                    CatName = reader["CatName"] as string
                };
                returnValue.Add(cat);
            }

            connection.Close();
            return returnValue;
        }

        [HttpGet("GetVideoFile")]
        public async Task<IActionResult> GetVideoFile([FromQuery] int? id)
        {
            var video = await GetVideo(id);
            if (video == null || !System.IO.File.Exists(video.FilePath))
                return NotFound();
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(Path.GetExtension(video.FilePath), out var contentType))
                contentType = provider.Mappings[".mp4"];
            var result = File(System.IO.File.OpenRead(video.FilePath), contentType, Path.GetFileName(video.FilePath));
            result.EnableRangeProcessing = true;

            return result;
        }

        public async Task<string> GetTagName(int? id)
        {
            if (id == 0)
                return "无标签";
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT [TagName] FROM [dbo].[Tag] WHERE [TagId] = @id", connection);
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            var result = await command.ExecuteScalarAsync();
            connection.Close();
            return result as string;
        }

        public async Task<string> GetCatName(int? id)
        {
            if (id == 0)
                return "未分类";
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT [CatName] FROM [dbo].[Cat] WHERE [CatId] = @id", connection);
            command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            var result = await command.ExecuteScalarAsync();
            connection.Close();
            return result as string;
        }

        [HttpPost("Upload")]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> Upload(IFormFile fileInput, int? videoCat, int? videoTag)
        {
            if (!IsAllowedEdit())
                return Forbid();
            if (fileInput == null || fileInput.Length == 0)
                return BadRequest();

            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            var connectTask = connection.OpenAsync();

            //防止文件重名
            var guid = Guid.NewGuid().ToString();
            string ext;
            try
            {
                ext = fileInput.FileName.Split(".").Last();
            }
            catch
            {
                ext = "mp4";
            }

            var savePath = Path.Combine(
                Directory.GetCurrentDirectory(), "upload",
                $"{guid}.{ext}");
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await fileInput.CopyToAsync(stream);
            }

            await connectTask;
            var command = new SqlCommand(
                "INSERT INTO [dbo].[Videos] ([FilePath], [VideoName], [VideoTag], [VideoCat]) VALUES (@FilePath, @VideoName, @VideoTag, @VideoCat)",
                connection);
            command.Parameters.Add("@FilePath", SqlDbType.NVarChar).Value = savePath;
            command.Parameters.Add("@VideoName", SqlDbType.NVarChar).Value = fileInput.FileName;
            command.Parameters.Add("@VideoTag", SqlDbType.Int).Value = videoTag;
            command.Parameters.Add("@VideoCat", SqlDbType.Int).Value = videoCat;
            await command.ExecuteNonQueryAsync();
            connection.Close();
            return Ok("{}");
        }


        public async Task ModifyVideoProperty(int id, string newName, int videoCat, int videoTag)
        {
            var connection = new SqlConnection(AppSettings.SqlConnectionString);
            await connection.OpenAsync();
            var command =
                new SqlCommand(
                    "UPDATE [dbo].[Videos] SET [VideoName] = @newName, [VideoCat] = @newCat, [VideoTag] = @newTag WHERE [VideosId] = @vid",
                    connection);
            command.Parameters.Add("@vid", SqlDbType.Int).Value = id;
            command.Parameters.Add("@newName", SqlDbType.NVarChar).Value = newName;
            command.Parameters.Add("@newTag", SqlDbType.Int).Value = videoTag;
            command.Parameters.Add("@newCat", SqlDbType.Int).Value = videoCat;
            await command.ExecuteNonQueryAsync();
        }
    }
}