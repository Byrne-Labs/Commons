using System.IO;
using Xunit;

namespace ByrneLabs.Commons.Tests
{
    [Trait("Category", "Unit Test")]
    public class FileUtilsTest
    {
        [Fact]
        public void TestGetRelativePath1()
        {
            var directory = new DirectoryInfo("c:\\test1\\test2");
            var file = new FileInfo($"{directory.FullName}\\test.txt");
            var relativePath = directory.GetRelativePath(file);

            Assert.Equal("test.txt", relativePath);
        }

        [Fact]
        public void TestSplitPath1()
        {
            var directory = new DirectoryInfo("c:\\test1\\test2");
            var pathPieces = directory.SplitPath();
            Assert.Equal(pathPieces, new[] { "c:", "test1", "test2" });
        }

        [Fact]
        public void TestSplitPath2()
        {
            var pathPieces = FileUtils.SplitPath("\\");
            Assert.Equal(new[] { string.Empty }, pathPieces);
        }
    }
}
