using System;
using System.Collections.Generic;
using System.Linq;

namespace QOLFramework.Utilities
{
    /// <summary>
    /// Utilitários de aleatoriedade para módulos do QOL.
    /// </summary>
    public static class RandomHelper
    {
        private static readonly System.Random _rng = new System.Random();

        public static bool Chance(int percentChance)
        {
            return _rng.Next(100) < percentChance;
        }

        public static bool Chance(float percentChance)
        {
            return _rng.NextDouble() * 100.0 < percentChance;
        }

        public static T Pick<T>(IList<T> items)
        {
            if (items == null || items.Count == 0) return default;
            return items[_rng.Next(items.Count)];
        }

        public static T Pick<T>(params T[] items)
        {
            return Pick((IList<T>)items);
        }

        public static T PickWeighted<T>(IList<(T item, int weight)> weightedItems)
        {
            if (weightedItems == null || weightedItems.Count == 0) return default;

            int totalWeight = weightedItems.Sum(w => w.weight);
            int roll = _rng.Next(totalWeight);
            int cumulative = 0;

            foreach (var (item, weight) in weightedItems)
            {
                cumulative += weight;
                if (roll < cumulative) return item;
            }

            return weightedItems[weightedItems.Count - 1].item;
        }

        public static IList<T> Shuffle<T>(IList<T> list)
        {
            if (list == null) return new List<T>();
            var shuffled = new List<T>(list);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[j];
                shuffled[j] = temp;
            }
            return shuffled;
        }

        public static IEnumerable<T> PickMultiple<T>(IList<T> items, int count)
        {
            if (items == null || items.Count == 0 || count <= 0) return Enumerable.Empty<T>();
            var shuffled = Shuffle(items);
            return shuffled.Take(Math.Min(count, shuffled.Count));
        }

        public static int Range(int min, int max)
        {
            return _rng.Next(min, max);
        }

        public static float Range(float min, float max)
        {
            return (float)(_rng.NextDouble() * (max - min) + min);
        }

        public static UnityEngine.Vector3 RandomOffset(float maxXZ, float maxY = 0f)
        {
            float x = Range(-maxXZ, maxXZ);
            float z = Range(-maxXZ, maxXZ);
            float y = maxY > 0 ? Range(0f, maxY) : 0f;
            return new UnityEngine.Vector3(x, y, z);
        }
    }
}
