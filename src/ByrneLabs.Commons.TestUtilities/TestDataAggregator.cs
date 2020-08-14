using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.TestUtilities
{
    [PublicAPI]
    public class TestDataAggregator : TestDataProvider
    {
        private readonly IList<ITestDataProvider> _dataProviders;

        public TestDataAggregator(params ITestDataProvider[] testDataProviders)
        {
            _dataProviders = new List<ITestDataProvider>(testDataProviders);
        }

        public override bool CanProvide(Type type)
        {
            return _dataProviders.Any(dataProvider => dataProvider.CanProvide(type));
        }

        public override IEnumerable Random(Type type, int minCount, int maxCount)
        {
            AssertCanProvide(type);

            return _dataProviders.Single(dataProvider => dataProvider.CanProvide(type)).Random(type, minCount, maxCount);
        }

        public override IEnumerable TestData(Type type)
        {
            AssertCanProvide(type);

            return _dataProviders.Single(dataProvider => dataProvider.CanProvide(type)).TestData(type);
        }

        protected void AddTestDataProvider(ITestDataProvider testDataProvider)
        {
            _dataProviders.Add(testDataProvider);
        }

        protected override object CreateTestObject(Type type) => throw new NotSupportedException();
    }
}
