using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ByrneLabs.Commons.TestUtilities
{
    public class TestDataAggregator : TestDataProvider
    {
        private readonly IList<ITestDataProvider> _dataProviders;

        public TestDataAggregator(params ITestDataProvider[] domainEntityTestDomainEntities)
        {
            _dataProviders = new List<ITestDataProvider>(domainEntityTestDomainEntities);
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

        protected override object Initialize(Type type) => throw new NotSupportedException();
    }
}
