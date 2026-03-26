using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenantManagement.Core.Extensions
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, bool ignoreCase = true)
            where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be null or empty.");

            if (!Enum.TryParse<T>(value, ignoreCase, out var result) ||
                !Enum.IsDefined(typeof(T), result))
            {
                throw new ArgumentException($"Invalid value '{value}' for enum {typeof(T).Name}");
            }

            return result;
        }
    }
}
