using System.Collections.Generic;

namespace AviassemblyMod
{
    public class Utils
    {
        public static void AppendArray<T>(ref T[] array, T item)
        {
            var ret = new List<T>(array.Length + 1);
            ret.AddRange(array);
            ret.Add(item);
            array = ret.ToArray();
        }
    }
}
