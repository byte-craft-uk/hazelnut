﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hazelnut
{
    partial class HazelnutExtensions
    {
        const int HTTP_PORT_NUMBER = 80, HTTPS_PORT_NUMBER = 443, MINUTE = 60;

        /// <summary>
        /// Downloads the text in this URL.
        /// </summary>
        public static async Task<string> Download(this Uri @this, string cookieValue = null, double timeOutSeconds = MINUTE)
        {
            var request = (HttpWebRequest)WebRequest.Create(@this);

            request.Timeout = (int)(timeOutSeconds * 1000);

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(@this, cookieValue.OrEmpty());
            }

            using var response = await request.GetResponseAsync().ConfigureAwait(false);
            using var stream = response.GetResponseStream();
            return await stream.ReadAllText().ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads the data in this URL.
        /// </summary>
        public static async Task<byte[]> DownloadData(this Uri @this, string cookieValue = null, double timeOutSeconds = MINUTE)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(@this);

            request.Timeout = (int)(timeOutSeconds * 1000);

            if (cookieValue.HasValue())
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.SetCookies(@this, cookieValue.OrEmpty());
            }

            using var response = await request.GetResponseAsync().ConfigureAwait(false);
            using var stream = response.GetResponseStream();
            return await stream.ReadAllBytesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Posts the specified object as JSON data to this URL.
        /// </summary>
        public static Task<string> PostJson(this Uri @this, object data)
            => @this.PostJson(data, customiseClient: null);

        /// <summary>
        /// Posts the specified object as JSON data to this URL. Your server-side web api should take one parameter, with [FromBody] attribute.
        /// </summary>
        public static Task<string> PostJson(this Uri @this, object data, Action<HttpClient> customiseClient = null)
        {
            return PostJson(@this, JsonSerializer.Serialize(data), customiseClient);
        }

        /// <summary>
        /// Posts the specified JSON text to this URL. Your server-side web api should take one parameter, with [FromBody] attribute.
        /// </summary>
        public static async Task<string> PostJson(this Uri @this, string json, Action<HttpClient> customiseClient = null)
        {
            using var client = new HttpClient();
            customiseClient?.Invoke(client);

            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(@this, requestContent);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Posts the specified data to this url and returns the response as string.
        /// All items in the postData object will be sent as individual FORM parameters to the destination.
        /// </summary>
        public static Task<HttpResponseMessage> Post(this Uri url, object data, Action<HttpClient> customiseClient = null)
        {
            using var client = new HttpClient();
            customiseClient?.Invoke(client);

            return client.PostAsync(url.ToString(), new FormUrlEncodedContent(new Dictionary<string, string>().AddFromProperties(data)));
        }

        /// <summary>
        /// Posts the specified data to this url and returns the response as string.
        /// All items in the postData object will be sent as individual FORM parameters to the destination.
        /// </summary>
        public static Task<HttpResponseMessage> Post(this Uri url, Dictionary<string, string> postData, Action<HttpClient> customiseClient = null)
        {
            using var client = new HttpClient();
            customiseClient?.Invoke(client);
            return client.PostAsync(url.ToString(), new FormUrlEncodedContent(postData));
        }

        /// <summary>
        /// Removes all query string parameters of this Url and instead adds the specified ones.
        /// </summary>
        public static Uri ReplaceQueryString(this Uri baseUrl, Dictionary<string, string> queryStringDictionary)
        {
            var r = new StringBuilder();

            r.Append(baseUrl.Scheme);
            r.Append("://");
            r.Append(baseUrl.Host);

            if (baseUrl.Port != HTTP_PORT_NUMBER && baseUrl.Port != HTTPS_PORT_NUMBER) r.Append(":" + baseUrl.Port);
            r.Append(baseUrl.AbsolutePath);

            var query = queryStringDictionary
                .Select(a => "{0}={1}".FormatWith(a.Key, a.Value.UrlEncode())).ToString("&");

            if (query.HasValue())
            {
                r.Append("?");
                r.Append(query);
            }

            return new Uri(r.ToString());
        }

        /// <summary>
        /// Gets the query string parameters of this Url.
        /// </summary>
        public static Dictionary<string, string> GetQueryString(this Uri url)
        {
            var result = new Dictionary<string, string>();

            var query = url.Query.OrEmpty().TrimStart("?");
            if (query.IsEmpty()) return result;

            var namePos = 0;

            while (namePos <= query.Length)
            {
                int valuePos = -1, valueEnd = -1;

                for (var q = namePos; q < query.Length; q++)
                {
                    if (valuePos == -1 && query[q] == '=') valuePos = q + 1;
                    else if (query[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                string name;

                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else name = UrlDecode(query.Substring(namePos, valuePos - namePos - 1));

                if (valueEnd < 0) valueEnd = query.Length;

                namePos = valueEnd + 1;
                var value = UrlDecode(query.Substring(valuePos, valueEnd - valuePos));
                if (!(name is null)) result.Add(name, value);
            }

            return result;
        }

        /// <summary>
        /// Removes the specified query string parameter.
        /// </summary>
        public static Uri RemoveEmptyQueryParameters(this Uri @this)
        {
            var toRemove = @this.GetQueryString().Where(x => x.Value.IsEmpty()).ToList();

            foreach (var item in toRemove) @this = @this.RemoveQueryString(item.Key);

            return @this;
        }

        /// <summary>
        /// Removes the specified query string parameter.
        /// </summary>
        public static Uri RemoveQueryString(this Uri url, string key)
        {
            var qs = url.GetQueryString();
            // key = key.ToLower();
            if (qs.ContainsKey(key)) qs.Remove(key);

            return url.ReplaceQueryString(qs);
        }

        /// <summary>
        /// Adds the specified query string setting to this Url.
        /// </summary>
        public static Uri AddQueryString(this Uri url, string key, string value)
        {
            var qs = url.GetQueryString();

            qs.RemoveWhere(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

            qs.Add(key, value);

            return url.ReplaceQueryString(qs);
        }

        /// <summary>
        /// Properly sets a query string key value in this Uri, returning a new Uri object.
        /// </summary>
        public static Uri SetQueryString(this Uri uri, string key, object value)
        {
            var pairs = uri.GetQueryString();

            pairs[key] = value.ToStringOrEmpty();

            var builder = new UriBuilder(uri)
            {
                Query = pairs.Select(x => x.Key + "=" + x.Value.UrlEncode()).ToString("&")
            };

            return builder.Uri;
        }
    }
}