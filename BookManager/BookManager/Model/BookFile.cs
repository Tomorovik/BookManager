using System.IO;
using System.Text;

namespace BookManager.Model
{
    public class BookFile
    {
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        public byte[] Hash { get; set; }
        public string PublicHash => Encoding.Default.GetString(Hash);
        public string ShortFileName => Path.GetFileName(FileName);
        public string Extension => Path.GetExtension(FileName);
    }
}
