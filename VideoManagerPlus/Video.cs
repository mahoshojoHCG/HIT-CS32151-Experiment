﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VideoManagerPlus
{
    public class Video
    {
        public int Id { get; set; }
        public string FilePath { get; set; }
        public string VideoName { get; set; }
        public int VideoCat { get; set; }
        public int VideoTag { get; set; }
    }
}
