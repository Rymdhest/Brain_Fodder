using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brain_Fodder.Recording
{
    public class VideoMetadata
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string[] Tags { get; set; }
        public string CategoryId { get; set; }
        public string PlaylistName { get; set; }
    }
}
