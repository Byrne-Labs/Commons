﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace ByrneLabs.Commons.TestUtilities
{
    [PublicAPI]
    public abstract class TestDataProvider : ITestDataProvider
    {
        private readonly IDictionary<Type, IList> _testData = new Dictionary<Type, IList>();

        protected static IEnumerable<T> RepeatCreate<T>(Func<T> create, int count)
        {
            var items = new List<T>();
            while (items.Count < count)
            {
                items.Add(create());
            }

            return items;
        }

        public bool CanProvide<T>() => CanProvide(typeof(T));

        public virtual bool CanProvide(Type type)
        {
            return _testData.Keys.Any(storedType => storedType.CanBeCastAs(type));
        }

        public T Random<T>() => (T) Random(typeof(T), 1, 1).Cast<object>().First();

        public object Random(Type type) => Random(type, 1, 1).Cast<object>().First();

        public IEnumerable<T> Random<T>(int maxCount) => Random(typeof(T), 1, maxCount).Cast<T>();

        public IEnumerable<T> Random<T>(int minCount, int maxCount) => Random(typeof(T), minCount, maxCount).Cast<T>();

        public IEnumerable Random(Type type, int maxCount) => Random(type, 1, maxCount);

        public virtual IEnumerable Random(Type type, int minCount, int maxCount)
        {
            AssertCanProvide(type);
            return TestData(type).RandomItems(minCount, maxCount);
        }

        public IEnumerable<T> TestData<T>() => TestData(typeof(T)).Cast<T>().ToArray();

        public virtual IEnumerable TestData(Type type)
        {
            AssertCanProvide(type);
            return _testData.Where(data => data.Key.CanBeCastAs(type)).SelectMany(data => data.Value.Cast<object>()).ToArray();
        }

        protected abstract object CreateTestObject(Type type);

        protected void AssertCanProvide(Type type)
        {
            if (!CanProvide(type))
            {
                throw new ArgumentException($"{type.FullName} is not supported");
            }
        }

        protected void Initialize(params Type[] types)
        {
            Initialize(types.Select(type => new Tuple<Type, int>(type, 20)).ToArray());
        }

        protected void Initialize(params Tuple<Type, int>[] types)
        {
            foreach (var type in types)
            {
                /*
                 * We add it first so the create method will not throw a not supported exception
                 */
                _testData.Add(type.Item1, new ArrayList());
                while (_testData[type.Item1].Count < type.Item2)
                {
                    _testData[type.Item1].Add(CreateTestObject(type.Item1));
                }
            }
        }

        protected void SetTestData<T>(IEnumerable<T> testData)
        {
            _testData[typeof(T)] = testData.ToArray();
        }

        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global", Justification = "False positive -- this should not be possible on a public API")]
        protected virtual IList StoredTestData(Type type)
        {
            AssertCanProvide(type);
            return _testData[type];
        }
    }
}
