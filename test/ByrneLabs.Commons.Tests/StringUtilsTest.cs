using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace ByrneLabs.Commons.Tests
{
    [Trait("Category", "Unit Test")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class StringUtilsTest
    {
        [Fact]
        public void TestContainsCount()
        {
            Assert.Equal(0, "asdf".ContainsCount("fdsa"));
            Assert.Equal(1, "asdf".ContainsCount("asdf"));
            Assert.Equal(1, "asdf".ContainsCount("as"));
            Assert.Equal(1, "asdf".ContainsCount("sd"));
            Assert.Equal(1, "asdf".ContainsCount("df"));
            Assert.Equal(2, "asdfasdf".ContainsCount("asdf"));
            Assert.Equal(2, "asdfasdf".ContainsCount("as"));
            Assert.Equal(2, "asdfasdf".ContainsCount("sd"));
            Assert.Equal(2, "asdfasdf".ContainsCount("df"));
        }

        [Fact]
        public void TestGetLineAndColumnNumber()
        {
            Assert.Equal((1, 1), "asdfasdfasdf".GetLineAndColumnNumber(0));
            Assert.Equal((1, 1), "asdf\nasdf\nasdf".GetLineAndColumnNumber(0));

            Assert.Equal((1, 5), "asdfasdfasdf".GetLineAndColumnNumber(4));
            Assert.Equal((1, 5), "asdf\nasdf\nasdf".GetLineAndColumnNumber(4));

            Assert.Equal((2, 1), "asdf\nasdf\nasdf".GetLineAndColumnNumber(5));

            Assert.Equal((2, 2), "asdf\nasdf\nasdf".GetLineAndColumnNumber(6));

            Assert.Equal((2, 3), "asdf\nasdf\nasdf".GetLineAndColumnNumber(7));

            Assert.Equal((3, 4), "asdf\nasdf\nasdf".GetLineAndColumnNumber(13));
        }

        [Fact]
        public void TestNthIndexOf()
        {
            Assert.Equal(-1, "asdfasdfasdf".NthIndexOf("fdsa", 1));
            Assert.Equal(0, "asdfasdfasdf".NthIndexOf("asdf", 1));
            Assert.Equal(4, "asdfasdfasdf".NthIndexOf("asdf", 2));
            Assert.Equal(8, "asdfasdfasdf".NthIndexOf("asdf", 3));
            Assert.Equal(-1, "asdfasdfasdf".NthIndexOf("asdf", 4));
        }

        [Fact]
        public void TestSubstringAfterLast()
        {
            Assert.Equal(string.Empty, "asdfasdf".SubstringAfterLast("fdsa"));
            Assert.Equal("df", "asdf".SubstringAfterLast("as"));
            Assert.Equal(string.Empty, "asdf".SubstringAfterLast("df"));
            Assert.Equal("df", "asdfasdf".SubstringAfterLast("as"));
            Assert.Equal(string.Empty, "asdfasdf".SubstringAfterLast("df"));
        }

        [Fact]
        public void TestSubstringBeforeLast()
        {
            Assert.Equal("asdfasdf", "asdfasdf".SubstringBeforeLast("fdsa"));
            Assert.Equal("asdf", "asdf".SubstringBeforeLast("as"));
            Assert.Equal("as", "asdf".SubstringBeforeLast("df"));
            Assert.Equal("asdf", "asdfasdf".SubstringBeforeLast("as"));
            Assert.Equal("asdfas", "asdfasdf".SubstringBeforeLast("df"));
        }

        [Fact]
        public void TestTrimStart()
        {
            Assert.Equal("asdf.www.", "www.asdf.www.".TrimStart("www."));
            Assert.Equal("asdf.www.", "asdf.www.".TrimStart("www."));
            Assert.Equal("asdf.www.", "asdf.www.".TrimStart("asdf.www.asdf"));
            Assert.Equal(string.Empty, "www.asdf.www.".TrimStart("www.asdf.www."));
            Assert.Equal(string.Empty, string.Empty.TrimStart("www."));
        }

        [Fact]
        public void TestTrimEnd()
        {
            Assert.Equal("www.asdf.", "www.asdf.www.".TrimEnd("www."));
            Assert.Equal("www.asdf", "www.asdf".TrimEnd("www."));
            Assert.Equal("asdf.www.", "asdf.www.".TrimEnd("asdf.www.asdf"));
            Assert.Equal(string.Empty, "www.asdf.www.".TrimEnd("www.asdf.www."));
            Assert.Equal(string.Empty, string.Empty.TrimEnd("www."));
        }
    }
}
