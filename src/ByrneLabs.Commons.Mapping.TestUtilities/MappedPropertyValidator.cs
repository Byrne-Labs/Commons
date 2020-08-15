using System;
using JetBrains.Annotations;
using Xunit;

namespace ByrneLabs.Commons.Mapping.TestUtilities
{
    [PublicAPI]
    public class MappedPropertyValidator<TFrom, TTo>
    {
        public Func<TFrom, object> From { get; set; }

        public string Name { get; set; }

        public Func<TTo, object> To { get; set; }

        public void Validate(TFrom from, TTo to)
        {
            var fromValue = From(from);
            var toValue = To(to);

            Assert.Equal(toValue, fromValue);
        }
    }
}
