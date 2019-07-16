using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using VideoManagerPlus.Controllers;

namespace VideoManagerPlus.Pages
{
    public class EditModel : PageModel
    {
        public EditModel(IOptions<AppSettings> settings)
        {
            Controller = new VideoController(settings);
        }

        protected VideoController Controller { get; }

        public Video CurrentVideo { get; set; }

        public List<Tag> AllTags { get; set; }

        public List<Cat> AllCats { get; set; }

        [BindProperty] [Required] public string NewName { get; set; }

        [BindProperty] [Required] public int NewVideoCat { get; set; }

        [BindProperty] [Required] public int NewVideoTag { get; set; }

        private bool IsAllowedEdit()
        {
            if (!HttpContext.Session.TryGetValue("allowed", out var val))
                return false;

            return Encoding.Default.GetString(val) == "true";
        }

        public async Task<IActionResult> OnPostAsync([FromQuery] int? id)
        {
            if (id == null)
                return NotFound();
            CurrentVideo = await Controller.GetVideo(id);
            if (CurrentVideo == null)
                return NotFound();
            await Controller.ModifyVideoProperty((int) id, NewName, NewVideoCat, NewVideoTag);
            return RedirectToPage("/Video", new {id});
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] int? id)
        {
            if (!IsAllowedEdit()) return RedirectToPage("/InputToken");
            if (id == null)
                return NotFound();
            var task1 = Controller.GetAllCats();
            var task2 = Controller.GetAllTags();
            var task3 = Controller.GetVideo(id);

            CurrentVideo = await task3;
            if (CurrentVideo == null)
                return NotFound();
            AllTags = await task2;
            AllCats = await task1;
            return Page();
        }
    }
}