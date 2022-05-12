using System;
using System.Collections.Generic;
using System.Linq;

namespace MarvBotV3
{
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Shuffle<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            Random r = new Random();
            return source.OrderBy(x => r.Next()).ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
