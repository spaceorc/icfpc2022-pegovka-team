using System;
using System.IO;

namespace lib
{
    public class FileHelper
    {
        public static string FindFilenameUpwards(string filename, string? baseDirectoryPath = null)
        {
            return Path.GetFullPath(Path.IsPathRooted(filename) ? filename : WalkDirectoryTree(filename, File.Exists, baseDirectoryPath));
        }

        public static string FindDirectoryUpwards(string dirName, string? baseDirectoryPath = null)
        {
            return Path.GetFullPath(Path.IsPathRooted(dirName) ? dirName : WalkDirectoryTree(dirName, Directory.Exists, baseDirectoryPath));
        }

        private static string WalkDirectoryTree(string filename, Func<string, bool> fileSystemObjectExists, string? baseDirectoryPath = null)
        {
            if (baseDirectoryPath == null)
                baseDirectoryPath = Directory.GetCurrentDirectory();
            var baseDirectory = new DirectoryInfo(baseDirectoryPath);
            while (baseDirectory != null)
            {
                var candidateFilename = Path.Combine(baseDirectory.FullName, filename);
                if (fileSystemObjectExists(candidateFilename))
                    return candidateFilename;
                baseDirectory = baseDirectory.Parent;
            }

            return filename;
        }
    }
}
