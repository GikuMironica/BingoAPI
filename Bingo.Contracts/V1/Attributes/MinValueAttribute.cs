using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Bingo.Contracts.V1.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MinValueAttribute : ValidationAttribute
    {
        private readonly Int64 _minValue;

        public MinValueAttribute(Int64 minValue)
        {
            _minValue = minValue;
        }

        public override bool IsValid(object value)
        {
            return (int)value >= _minValue;
        }
    }
}
