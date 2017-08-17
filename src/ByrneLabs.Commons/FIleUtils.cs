using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using EnsureThat;

namespace ByrneLabs.Commons
{
    public static class FileUtils
    {
        public static DirectoryInfo Combine(this DirectoryInfo parentDirectory, params string[] childDirectories) => new DirectoryInfo(Path.Combine(parentDirectory.FullName, Path.Combine(childDirectories)));

        public static DirectoryInfo CombineDirectories(params string[] path) => new DirectoryInfo(Path.Combine(path));

        public static void Copy(string source, DirectoryInfo destination)
        {
            Copy(new DirectoryInfo(source), destination);
        }

        public static void Copy(this DirectoryInfo source, string destination)
        {
            source.Copy(new DirectoryInfo(destination));
        }

        public static void Copy(this DirectoryInfo source, DirectoryInfo destination)
        {
            if (!source.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {source.FullName}");
            }

            if (!destination.Exists)
            {
                destination.Create();
            }

            var files = source.GetFiles();
            foreach (var file in files)
            {
                file.CopyTo(Path.Combine(destination.FullName, file.Name), false);
            }

            foreach (var subDirectory in source.GetDirectories())
            {
                subDirectory.Copy(destination.Combine(subDirectory.Name));
            }
        }

        public static FileSystemInfo GetGreatestCommonPath(this FileSystemInfo file1, FileSystemInfo file2)
        {
            EnsureArg.IsNotNull(file1);
            EnsureArg.IsNotNull(file2);

            FileSystemInfo commonFileSystemInfo;

            if (file1 is FileInfo && file2 is FileInfo && file1.FullName.Equals(file2.FullName, StringComparison.Ordinal))
            {
                commonFileSystemInfo = new FileInfo(file1.FullName);
            }
            else
            {
                var filePath1 = file1.SplitPath();
                var filePath2 = file2.SplitPath();

                var commonPath = new StringBuilder();
                for (var index = 0; index < filePath1.Length && index < filePath2.Length; index++)
                {
                    if (filePath1[index].Equals(filePath2[index], StringComparison.Ordinal))
                    {
                        commonPath.Append(filePath1[index]).Append(Path.DirectorySeparatorChar);
                    }
                    else
                    {
                        break;
                    }
                }

                commonFileSystemInfo = new DirectoryInfo(commonPath.Remove(commonPath.Length - 1, 1).ToString());
            }

            return commonFileSystemInfo;
        }

        public static string GetRelativePath(this DirectoryInfo fromDirectory, FileSystemInfo toFile)
        {
            var commonPathLength = GetGreatestCommonPath(fromDirectory, toFile).SplitPath().Length;
            var fromPathLength = fromDirectory.SplitPath().Length;
            var relativePath = new StringBuilder();

            relativePath.Append($"..{Path.DirectorySeparatorChar}".Repeat(fromPathLength - commonPathLength));
            var toPath = toFile.SplitPath();
            relativePath.Append(string.Join(Path.DirectorySeparatorChar.ToString(CultureInfo.CurrentCulture), toPath.Skip(commonPathLength)));

            return relativePath.ToString();
        }

        public static string[] SplitPath(this FileSystemInfo fileSystemInfo)
        {
            EnsureArg.IsNotNull(fileSystemInfo);
            return SplitPath(fileSystemInfo.FullName);
        }

        public static string[] SplitPath(string path)
        {
            EnsureArg.IsNotNull(path);
            return path.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
