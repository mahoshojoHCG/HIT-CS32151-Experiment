﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FFmpeg.NET;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Options;
using VideoManagerPlus.Controllers;

namespace VideoManagerPlus.Pages
{
    public class IndexModel : PageModel
    {
        protected VideoController Controller { get; }

        public List<Video> AllVideos { get; set; }

        public List<Tag> AllTags { get; set; }

        public List<Cat> AllCats { get; set; }


        public IndexModel(IOptions<AppSettings> settings)
        {
            Controller = new VideoController(settings);
        }
        public virtual async Task OnGetAsync()
        {
            var task1 = Controller.GetAllVideos();
            var task2 = Controller.GetAllCats();
            var task3 = Controller.GetAllTags();
            AllVideos = await task1;
            AllCats = await task2;
            AllTags = await task3;
        }
    }
}
