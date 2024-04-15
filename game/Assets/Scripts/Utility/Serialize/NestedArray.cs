using System;
using System.Collections;

namespace Utility.Serialize
{
    [Serializable]
    public class NestedArray<T>
    {
        public T[] array;
    }
}