using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ByrneLabs.Commons.TestUtilities
{
    public interface ITestDataProvider
    {
        bool CanProvide(Type type);

        [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global", Justification = "This is something likely to be used in the future")]
        bool CanProvide<T>();

        object Random(Type type);

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Currently unused but it is a logical addition")]
        IEnumerable Random(Type type, int maxCount);

        IEnumerable Random(Type type, int minCount, int maxCount);

        T Random<T>();

        IEnumerable<T> Random<T>(int maxCount);

        [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Currently unused but it is a logical addition")]
        IEnumerable<T> Random<T>(int minCount, int maxCount);

        IEnumerable TestData(Type type);

        IEnumerable<T> TestData<T>();
    }
}
