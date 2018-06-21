using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ByrneLabs.Commons.Tests
{
    [Trait("Category", "Unit Test")]
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
    }
}
