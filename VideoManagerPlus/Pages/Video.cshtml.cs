using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using VideoManagerPlus.Controllers;

namespace VideoManagerPlus.Pages
{
    public class VideoModel : PageModel
    {
        public VideoModel(IOptions<AppSettings> settings)
        {
            Controller = new VideoController(settings);
        }

        public Video CurrentVideo { get; set; }

        private VideoController Controller { get; }

        public string TagName { get; set; }
        public string CatName { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] int? id)
        {
            var video = await Controller.GetVideo(id);
            if (video == null)
                return NotFound();
            CurrentVideo = video;
            TagName = await Controller.GetTagName(video.VideoTag);
            CatName = await Controller.GetCatName(video.VideoCat);
            return Page();
        }
    }
}