﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Hazelnut
{
    public static partial class HazelnutWebExtensions
    {
        /// <summary>
        /// Gets the cookies sent by the client.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetCookies(this HttpRequest @this)
        {
            if (@this.Cookies == null) return Enumerable.Empty<KeyValuePair<string, string>>();
            return @this.Cookies.AsEnumerable();
        }

        /// <summary>
        /// Returns a string value specified in the request context (form, query string or route).
        /// </summary>
        public static string Param(this HttpRequest @this, string key)
        {
            if (@this.HasFormContentType && @this.Form.ContainsKey(key))
                return @this.Form[key].ToStringOrEmpty();

            if (@this.Query.ContainsKey(key))
                return @this.Query[key].ToStringOrEmpty();

            return (@this.GetRouteValues()?[key]).ToStringOrEmpty();
        }


        /// <summary>
        /// Finds the search keywords used by this user on Google that led to the current request.
        /// </summary>
        public static string FindGoogleSearchKeyword(this HttpRequest @this)
        {
            var urlReferrer = @this.Headers["Referer"].ToString();
            if (urlReferrer.IsEmpty()) return null;

            // Note: Only Google is supported for now:

            if (!urlReferrer.ToLower().Contains(".google.co"))
                return null;

            foreach (var possibleQuerystringKey in new[] { "q", "as_q" })
            {
                var queryString = urlReferrer.Split('?').Skip(1).FirstOrDefault();

                var query = queryString.TrimStart("?").Split('&').Trim().
                    FirstOrDefault(p => p.StartsWith(possibleQuerystringKey + "="));

                if (query.HasValue())
                    return query.Substring(1 + possibleQuerystringKey.Length).UrlDecode();
            }

            return null;
        }

        /// <summary>
        /// Gets the actual IP address of the user considering the Proxy and other HTTP elements.
        /// </summary>
        public static string GetIPAddress(this HttpRequest @this)
            => @this.HttpContext.Connection.RemoteIpAddress.ToString();

        /// <summary>
        /// Determines if the specified argument exists in the request (form, query string or route).
        /// </summary>
        public static bool Has(this HttpRequest @this, string argument)
        {
            if (@this.HasFormContentType && @this.Form.ContainsKey(argument)) return true;
            else if (@this.Query.ContainsKey(argument)) return true;
            else return (@this.GetRouteValues()?[argument]).ToStringOrEmpty().HasValue();
        }

        public static RouteValueDictionary GetRouteValues(this HttpRequest @this)
        {
            return Context.Current.ActionContext()?.RouteData?.Values;
        }

        /// <summary>
        /// Determines if the specified argument not exists in the request (query string or form).
        /// </summary>
        public static bool Lacks(this HttpRequest @this, string argument) => !@this.Has(argument);

        /// <summary>
        /// Gets the root of the requested website.
        /// </summary>
        public static string RootUrl(this HttpRequest @this)
        {
            var forwarded = @this.Headers["X-Forwarded-Proto"].FirstOrDefault();
            var scheme = forwarded.Or(@this.Scheme);

            return $"{scheme}://{@this.Host}/";
        }

        /// <summary>
        /// Gets the raw url of the request.
        /// </summary>
        public static string ToRawUrl(this HttpRequest @this) =>
            $"{@this.PathBase}{@this.ToPathAndQuery()}";

        /// <summary>
        /// Gets the relative request path plus the query string (encoded).
        /// </summary>
        public static string ToPathAndQuery(this HttpRequest @this)
         => $"{@this.Path}{@this.Query.ToEncodedString()}";

        /// <summary>
        /// Returns the query string as originally requested with a ? prefix.
        /// </summary>
        public static string ToEncodedString(this IQueryCollection @this)
        {
            return @this.OrEmpty()
                .Select(x => x.Key.UrlEncode() + "=" + x.Value.ToString(",").UrlEncode())
                .ToString("&")
                .WithPrefix("?");
        }

        /// <summary>
        /// Gets the absolute Uri of the request.
        /// </summary>
        public static string ToAbsoluteUri(this HttpRequest @this)
            => @this.RootUrl().TrimEnd('/') + @this.ToRawUrl();

        /// <summary>
        /// Gets the absolute URL for a specified relative url.
        /// </summary>
        public static string GetAbsoluteUrl(this HttpRequest @this, string relativeUrl) =>
            @this.RootUrl() + relativeUrl.TrimStart("/");

        
        /// <summary>
        /// Determines if this is a GET http request.
        /// </summary>
        public static bool IsGet(this HttpRequest @this) => @this.Method == System.Net.WebRequestMethods.Http.Get;

        /// <summary>
        /// Determines if this is a POST http request.
        /// </summary>
        public static bool IsPost(this HttpRequest @this) => @this.Method == System.Net.WebRequestMethods.Http.Post;

        /// <summary>
        /// Gets the currently specified return URL.
        /// </summary>
        public static string GetReturnUrl(this HttpRequest @this)
        {
            var result = @this.Param("ReturnUrl");

            if (result.IsEmpty()) return string.Empty;

            if (result.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                result.ToCharArray().ContainsAny('\'', '\"', '>', '<') ||
                result.ContainsAny(new[] { "//", ":" }, caseSensitive: false))
                return string.Empty;

            return result;
        }

        public static bool IsLocal(this HttpRequest @this)
        {
            var connection = @this.HttpContext.Connection;

            if (connection.RemoteIpAddress != null)
            {
                if (IPAddress.IsLoopback(connection.RemoteIpAddress)) return true;

                if (Dns.GetHostAddresses(Dns.GetHostName()).Contains(connection.RemoteIpAddress))
                    return true;
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
                return true;

            return false;
        }
    }
}