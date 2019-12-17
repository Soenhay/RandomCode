using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoenhaysLibrary
{
    public static class UrlHelper
    {
        /// <summary>
        /// Converts the provided app-relative path into an absolute Url containing the 
        /// full host name
        /// </summary>
        /// <param name="relativeUrl">App-Relative path</param>
        /// <returns>Provided relativeUrl parameter as fully qualified Url</returns>
        /// <example>~/path/to/foo to http://www.web.com/path/to/foo</example>
        /// <remarks>http://stackoverflow.com/questions/3681052/get-absolute-url-from-relative-path-refactored-method</remarks>
        public static string ToAbsoluteUrl(this string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;

            if (HttpContext.Current == null)
                return relativeUrl;

            if (relativeUrl.StartsWith("/"))
                relativeUrl = relativeUrl.Insert(0, "~");
            if (!relativeUrl.StartsWith("~/"))
                relativeUrl = relativeUrl.Insert(0, "~/");

            var url = HttpContext.Current.Request.Url;
            var port = url.Port != 80 ? (":" + url.Port) : String.Empty;

            string host;
            if (url.Host == "localhost")
            {
                host = "www.ogat.space";
                port = "";
            }
            else
            {
                host = url.Host;
            }

            return String.Format("{0}://{1}{2}{3}",
                url.Scheme, host, port, VirtualPathUtility.ToAbsolute(relativeUrl));
        }


        public static string getHostPrefix(string host)
        {
            string preHost = host.Split('.')[0].ToLower().Replace("http://", "").Replace("https://", "").Trim();
            return preHost;
        }

        #region Handling url parameters

        /// <summary>
        /// Given a string of url's parameters and a parameter's name. 
        /// This function returns value of the parameter
        /// </summary>
        /// <param name="urlParams"></param>
        /// <param name="paramToGet"></param>
        /// <returns></returns>
        public static string URL_Param_Get(string urlParams, string paramToGet)
        {
            string ret = "";

            if (urlParams.Contains("?"))
            {
                string[] urlSplit = urlParams.Split('?');
                urlParams = urlSplit[1];
            }

            string[] prams = urlParams.Split('&');

            foreach (string pram in prams)
            {
                string[] op = pram.Split('=');
                if (op[0] == paramToGet)
                    return op[1];
            }

            return ret;
        }

        /// <summary>
        /// Safely add a parameter and value to the url. Will not add it if it is already there.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public static string URL_Param_UpdateAdd(string url, string paramName, string paramValue)
        {
            string result = url;
            string[] urlSplit = result.Split('?');
            string currParams = (urlSplit.Length > 1) ? urlSplit[1] : "";
            result = urlSplit[0].Replace("?", "") + "?" + Params_UpdateAdd(currParams, paramName, paramValue).Replace("?", "");
            return result;
        }

        /// <summary>
        /// Change the value of a given url parameter in a given url's parameters. Add the param and value if it does not exist.
        /// </summary>
        /// <param name="urlParams"></param>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        public static string Params_UpdateAdd(string urlParams, string paramName, string paramValue)
        {
            //Test cases
            //Any of the these test cases with a ? in it will cause an infinite loop when testing unless you comment it out after setting it. It won't cause an infinite loop under normal circumstances.
            //urlParams = "http://www.somewebsite.com/afolder/apage.aspx";
            //urlParams = "http://www.somewebsite.com/afolder/apage.aspx?";
            //urlParams = "http://www.somewebsite.com/afolder/apage.aspx?SLR=1&un=1234&al=284&co=14";
            //urlParams = "SLR=1&un=1234&al=284";
            //urlParams = "?SLR=1&un=1234&al=284";
            //urlParams = "?";

            if (urlParams.Contains("?"))
            {
                return URL_Param_UpdateAdd(urlParams, paramName, paramValue);
            }

            Boolean found = false;
            string[] prams = urlParams.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            urlParams = "";//Must be after split.

            foreach (string pram in prams)
            {
                string[] op = pram.Split('=');

                if (op[0].ToLower().Trim().Equals(paramName.ToLower().Trim()))
                {
                    found = true;
                    op[0] = paramName;
                    op[1] = paramValue;
                }

                urlParams += op[0] + "=" + ((op.Length > 1) ? op[1] : "");
                if (Array.IndexOf(prams, pram) < prams.Length - 1)
                {
                    urlParams += "&";
                }
            }

            if (urlParams.EndsWith("="))
                urlParams = urlParams.Remove(urlParams.Length - 1);

            if (!found)
            {
                if (urlParams.Length > 0)
                {
                    if (urlParams.Contains("="))
                        urlParams += "&";
                    else
                        urlParams += "?";//A url with 0 params was passed in.

                }
                urlParams += paramName + "=" + paramValue;
            }

            return urlParams;
        }

        /// <summary>
        /// Remove a url parameter with a given name from the url's parameters
        /// </summary>
        /// <param name="urlParams"></param>
        /// <param name="paramToRemove"></param>
        public static string Params_Remove(string urlParams, string paramToRemove)
        {
            //Test cases
            //urlParams = "http://www.somewebsite.com/afolder/apage.aspx";
            //urlParams = "http://www.somewebsite.com/afolder/apage.aspx?";
            //urlParams = "http://www.somewebsite.com/afolder/apage.aspx?SLR=1&un=1234&al=284&co=14";
            //urlParams = "SLR=1&un=1234&al=284";
            //urlParams = "?SLR=1&un=1234&al=284";
            //urlParams = "?";

            if (urlParams == "?")
                return "?";

            string url = "";
            if (urlParams.Contains("?"))
            {
                string[] urlSplit = urlParams.Split('?');
                url = urlSplit[0];
                urlParams = urlSplit[1];
                url = (url.Length > 0 && urlParams.Length > 0) ? url + "?" : url;
                url = (url.Length == 0 && urlParams.Length > 0) ? url + "?" : url;
            }

            string[] prams = urlParams.Split('&');
            urlParams = "";//Must be after split.

            foreach (string pram in prams)
            {
                string[] op = pram.Split('=');

                if (op[0] != paramToRemove)
                {
                    urlParams += op[0] + "=" + ((op.Length > 1) ? op[1] : "");
                    if (Array.IndexOf(prams, pram) < prams.Length - 1)
                    {
                        urlParams += "&";
                    }
                }
            }

            if (urlParams.EndsWith("=") || urlParams.EndsWith("&"))
                urlParams = urlParams.Remove(urlParams.Length - 1);

            return url + urlParams;
        }

        #endregion Handling url parameters
    }

    public static class UrlHelperExtensions
    {
        //public static string GetSubdomain(this HttpRequestBase request)
        //{
        //    return request.Url.Host.ToLower().Replace(".ogat.space", "");
        //}

        ///Get the sub-domain from the provided URL
        /// <summary>
        /// <param name="url">URL to retrieve sub-domain from</param>
        /// <returns></returns>
        /// <remarks>http://developmentsolutionsjunction.blogspot.com/2011/05/get-sub-domain-from-provided-url.html</remarks>
        /// </summary>
        public static string RetrieveSubDomain(this Uri url)
        {
            string subDomain = "";
            if (url.HostNameType == UriHostNameType.Dns && (!(url.HostNameType == UriHostNameType.Unknown)))
            {
                string host = url.Host;
                if (host.StartsWith("www."))
                {
                    host = host.Remove(0, 4);
                }

                int length = host.Split('.').Length; if (length > 2)
                {
                    int last = host.LastIndexOf(".");
                    int idx = host.LastIndexOf(".", last - 1);
                    subDomain = host.Substring(0, idx);
                }
            }
            return MiscUtility.isDebug() ? "localhost" : subDomain;
        }

        public static bool isValidSubdomain(this string subDomain)
        {
            //return (subDomain != "localhost" && subDomain != "www" && subDomain != "");
            return MiscUtility.isDebug() ? true : (subDomain != "localhost" && subDomain != "www" && subDomain != "");
        }
    }
}
