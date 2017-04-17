using BookManager.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;

namespace BookManager.Utility
{
    public static class FileSearch
    {
        private static List<string> foldersToSkip = new List<string>() { "$RECYCLE.BIN" };

        public static async Task<List<BookFile>> GetPdfFiles(string path, string pattern, bool skipBin = false)
        {
            List<BookFile> pdfFiles = new List<BookFile>();
            await Task.Run(async () =>
            {
                try
                {
                    var dirs = Directory.GetDirectories(Path.Combine(@"\?\", path)).AsParallel();
                    foreach (var dir in dirs)
                    {
                        if (skipBin && foldersToSkip.Contains(Path.GetFileName(dir)))
                            continue;
                        pdfFiles.AddRange(await GetPdfFiles(dir, pattern));
                    }
                    var files = Directory.GetFiles(Path.Combine(@"\?\", path), pattern, SearchOption.TopDirectoryOnly).AsParallel();
                    foreach (var file in files)
                    {
                        pdfFiles.Add(
                            new BookFile
                            {
                                FileLocation = path,
                                FileName = file,
                                Hash = GetHash(file)
                            });
                    }

                }
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
            });
            return pdfFiles;
        }

        private static byte[] GetHash(string file)
        {
            byte[] hash = null;
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(file))
                    {
                        hash = md5.ComputeHash(stream);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            return hash;
        }
    }
}
