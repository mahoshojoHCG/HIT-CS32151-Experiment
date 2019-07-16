using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace VideoManagerPlus.Pages
{
    public class InputTokenModel : PageModel
    {
        public InputTokenModel(IOptions<AppSettings> settings)
        {
            AppSettings = settings.Value;
            Error = false;
        }

        private AppSettings AppSettings { get; }

        [BindProperty] [Required] public string Token { get; set; }

        public bool Error { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (AppSettings.AllowedTokens.ToList().FindAll(token => token == Token).Count != 0)
            {
                HttpContext.Session.SetString("allowed", "true");
                return RedirectToPage("/Index");
            }

            await Task.Delay(1);
            Error = true;
            return Page();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.TryGetValue("allowed", out var val))
                return Page();
            if (Encoding.Default.GetString(val) == "true")
                return RedirectToPage("/Index");
            await Task.Delay(1);
            return Page();
        }
    }
}