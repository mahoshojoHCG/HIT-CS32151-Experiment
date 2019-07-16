using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using VideoManagerPlus.Controllers;

namespace VideoManagerPlus.Pages
{
    public class TagModel : PageModel
    {
        public TagModel(IOptions<AppSettings> settings)
        {
            Controller = new VideoController(settings);
        }

        private VideoController Controller { get; }

        public List<Video> AllVideos { get; set; }

        public List<Tag> AllTags { get; set; }

        public Tag QueryTag { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] int? id)
        {
            AllVideos = await Controller.GetAllVideos();
            AllTags = await Controller.GetAllTags();
            QueryTag = AllTags.Find(t => t.Id == id);
            if (id == 0)
                QueryTag = new Tag {Id = 0, TagName = "无标签"};
            return Page();
        }
    }
}