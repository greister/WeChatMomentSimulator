// Models/DynamicParameter.cs
using System;

namespace WeChatMomentSimulator.Core.Models
{
    public class DynamicParameter<T>
    {
        public T MinValue { get; set; }
        public T MaxValue { get; set; }
        public T CurrentValue { get; set; }
        
        private static readonly Random _random = new Random();
        
        public T GenerateRandomValue()
        {
            if (typeof(T) == typeof(int))
            {
                var min = Convert.ToInt32(MinValue);
                var max = Convert.ToInt32(MaxValue);
                return (T)(object)_random.Next(min, max + 1);
            }
            else if (typeof(T) == typeof(double))
            {
                var min = Convert.ToDouble(MinValue);
                var max = Convert.ToDouble(MaxValue);
                return (T)(object)(_random.NextDouble() * (max - min) + min);
            }
            
            return CurrentValue;
        }
    }
}