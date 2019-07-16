using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using VideoManagerPlus.Controllers;

namespace VideoManagerPlus.Pages
{
    public class UploadModel : PageModel
    {
        public UploadModel(IOptions<AppSettings> settings)
        {
            Controller = new VideoController(settings);
        }

        protected VideoController Controller { get; }

        public List<Tag> AllTags { get; set; }

        public List<Cat> AllCats { get; set; }

        private bool IsAllowedEdit()
        {
            if (!HttpContext.Session.TryGetValue("allowed", out var val))
                return false;
            return Encoding.Default.GetString(val) == "true";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!IsAllowedEdit())
                return RedirectToPage("/InputToken");
            var task2 = Controller.GetAllCats();
            var task3 = Controller.GetAllTags();
            AllCats = await task2;
            AllTags = await task3;
            return Page();
        }
    }
}