using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Planning
{
    public static class Helpers
    {
        public static T GetMin<T>(this IEnumerable<T> iter, Func<T, float> value)
        {
            var min = default(T);
            var minv = float.MaxValue;

            foreach (var item in iter)
            {
                var val = value(item);
                if (val >= minv)
                    continue;

                min = item;
                minv = val;
            }

            return min;
        }

        public static T GetMax<T>(this IEnumerable<T> iter, Func<T, float> value)
        {
            var max = default(T);
            var maxv = -float.MaxValue;

            foreach (var item in iter)
            {
                var val = value(item);
                if (val <= maxv)
                    continue;

                max = item;
                maxv = val;
            }

            return max;
        }
    }

}
