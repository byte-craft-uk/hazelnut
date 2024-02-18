using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hazelnut.Mvc
{
    public static partial class HazelnutMvcExtensions
    {
        /// <summary>
        /// Gets a URL helper for the current http context.
        /// </summary>
        public static UrlHelper GetUrlHelper(this HttpContext context)
            => new UrlHelper(Context.Current.ActionContext());
    }
}
