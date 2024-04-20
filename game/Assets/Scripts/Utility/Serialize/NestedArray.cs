using System;

namespace Utility.Serialize
{
    [Serializable]
    public class NestedArray<T>
    {
        public T[] array;
    }
}