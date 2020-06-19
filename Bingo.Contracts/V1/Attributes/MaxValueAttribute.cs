using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Bingo.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxValueAttribute : ValidationAttribute
    {
        private readonly Int64 _maxValue;

        public MaxValueAttribute(Int64 maxValue)
        {
            _maxValue = maxValue;
        }

        public override bool IsValid(object value)
        {
            return (int)value <= _maxValue;
        }
    }
}
