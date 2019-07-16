using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using VideoManagerPlus.Controllers;

namespace VideoManagerPlus.Pages
{
    public class CatModel : PageModel
    {
        public CatModel(IOptions<AppSettings> settings)
        {
            Controller = new VideoController(settings);
        }

        private VideoController Controller { get; }

        public List<Video> AllVideos { get; set; }

        public List<Cat> AllCats { get; set; }

        public Cat QueryCat { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] int? id)
        {
            AllVideos = await Controller.GetAllVideos();
            AllCats = await Controller.GetAllCats();
            QueryCat = AllCats.Find(t => t.Id == id);
            if (id == 0)
                QueryCat = new Cat {Id = 0, CatName = "未分类"};
            return Page();
        }
    }
}