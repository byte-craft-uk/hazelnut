using System;
using System.Collections.Generic;
using System.Linq;

namespace Hazelnut
{
    partial class HazelnutExtensions
    {
        /// <summary>
        /// Shortens this GUID.
        /// </summary>
        public static ShortGuid Shorten(this Guid @this) => new(@this);

        public static bool IsAnyOf(this Guid? @this, params Guid?[] items)
            => items?.Contains(@this) ?? false;

        public static bool IsAnyOf(this Guid? @this, IEnumerable<Guid?> items)
            => items?.Contains(@this) ?? false;

        public static bool IsAnyOf(this Guid? @this, IEnumerable<Guid> items)
            => @this.HasValue && (items?.Contains(@this) == true);

        public static bool IsAnyOf(this Guid @this, params Guid?[] items)
            => items?.Contains(@this) ?? false;

        public static bool IsAnyOf(this Guid @this, IEnumerable<Guid?> items)
            => items?.Contains(@this) ?? false;

        public static bool IsAnyOf(this Guid @this, IEnumerable<Guid> items)
            => items?.Contains(@this) ?? false;
    }
}