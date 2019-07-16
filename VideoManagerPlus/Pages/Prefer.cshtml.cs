using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace VideoManagerPlus.Pages
{
    public class PreferModel : IndexModel
    {
        public PreferModel(IOptions<AppSettings> settings) : base(settings)
        {
        }

        public List<Video> PreferVideos { get; set; }

        public override async Task OnGetAsync()
        {
            await base.OnGetAsync();
            if (!HttpContext.Request.Cookies.ContainsKey("preferVideo"))
            {
                PreferVideos = null;
                return;
            }

            var allPrefer = HttpContext.Request.Cookies["preferVideo"].Split(",");
            var allPreferId = new List<int>();
            foreach (var str in allPrefer)
                try
                {
                    allPreferId.Add(Convert.ToInt32(str));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            allPreferId.Sort();
            PreferVideos = AllVideos.FindAll(v => allPreferId.Contains(v.Id));
        }
    }
}