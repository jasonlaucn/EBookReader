using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBookReader.Model
{
    public class BookInfo
    {
        public string BookId { get; set; }
        public string BookName { get; set; }
        public string FilePath { get; set; }
        public string ChapterName { get; set; }
        public int ChapterIndex { get; set; }
        public double ChapterOffset { get; set; }
    }
}
