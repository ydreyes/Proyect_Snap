using System;

namespace SnapClone.Core.Services
{
    public class RandomService
    {
        private Random _random;
        
        public RandomService(int seed = 0)
        {
            _random = seed == 0 ? new Random() : new Random(seed);
        }
        
        public int Next(int maxValue) => _random.Next(maxValue);
        public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
        public float NextFloat() => (float)_random.NextDouble();
        public bool NextBool() => _random.Next(2) == 0;
    }
}
