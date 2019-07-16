using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using VideoManagerPlus.Controllers;

namespace VideoManagerPlus.Pages
{
    public class UploadModel : PageModel
    {
        protected VideoController Controller { get; }

        public List<Tag> AllTags { get; set; }

        public List<Cat> AllCats { get; set; }

        public UploadModel(IOptions<AppSettings> settings)
        {
            Controller = new VideoController(settings);
        }

        public async Task OnGetAsync()
        {
            var task2 = Controller.GetAllCats();
            var task3 = Controller.GetAllTags();
            AllCats = await task2;
            AllTags = await task3;
        }
    }
}