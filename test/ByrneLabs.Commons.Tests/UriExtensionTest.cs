using System;
using Xunit;

namespace ByrneLabs.Commons.Tests
{
    public class UriExtensionTest
    {
        [Fact]
        public void TestRemoveQueryParameter1()
        {
            var uri = new Uri("http://some.domain/path1/path2/resource?key1=value1&key2=value2");
            var newUri = uri.RemoveQueryParameter("key1");
            Assert.Equal("http://some.domain/path1/path2/resource?key2=value2", newUri.ToString());
        }

        [Fact]
        public void TestRemoveQueryParameter2()
        {
            var uri = new Uri("http://some.domain/path1/path2/resource?key1=value1&key2=value2&key1=value3");
            var newUri = uri.RemoveQueryParameter("key1");
            Assert.Equal("http://some.domain/path1/path2/resource?key2=value2", newUri.ToString());
        }

        [Fact]
        public void TestRemoveQueryParameter3()
        {
            var uri = new Uri("http://some.domain/path1/path2/resource?key1=value");
            var newUri = uri.RemoveQueryParameter("key1");
            Assert.Equal("http://some.domain/path1/path2/resource", newUri.ToString());
        }
    }
}
