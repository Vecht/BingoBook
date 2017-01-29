using System.Collections.Generic;
using System.Linq;

namespace MFDSkillTracking.Extensions
{
    public static class StatsFunctions
    {
        public static double[] Normalize(this IEnumerable<double> distribution)
        {
            var dist = distribution.ToArray();
            var sum = dist.Sum();
            return dist.Select(x =>
            {
                var val = x/sum;
                return val <= 1.0e-100 ? 1.0e-100
                : val > 0.95 ? 0.95
                : val;
            }).ToArray();
        }
    }
}
