using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace OAuth
{
    class Program
    {
        const string ConsumerKey = "";
        const string ConsumerSecret = "";

        static void Main(string[] args)
        {
            string endpoint = $"https://query.yahooapis.com/v1/yql?format=json&q={"select * from geo.places where text=\"Vancouver, BC\" and country.code=\"CA\"".OAuthUrlEncode()}";
            string result = YQLRequest(endpoint);
        }

        static string YQLRequest(string endpoint)
        {
            // Use 2-legged OAuth because we're accessing a public api and not private user specific api
            // i.e. we don't need a user oauth_token or user oauth_token_secret.

            // https://developer.yahoo.com/oauth/guide/oauth-signing.html
            // http://stackoverflow.com/a/13205627/188740

            var request = (HttpWebRequest)HttpWebRequest.Create(endpoint);
            OAuthBase YQL = new OAuthBase();
            string nonce = YQL.GenerateNonce();
            string timestamp = YQL.GenerateTimeStamp();
            request.Headers.Add(
                "Authorization: OAuth " +
                "realm=\"yahooapis.com\"," +
                "oauth_consumer_key=\"" + ConsumerKey + "\"," +
                "oauth_nonce=\"" + nonce + "\"," +
                "oauth_signature_method=\"PLAINTEXT\"," +
                "oauth_timestamp=\"" + timestamp + "\"," +
                "oauth_version=\"1.0\"," +
                "oauth_signature=\"" + ConsumerSecret + "%26\"" // We add '&' https://developer.yahoo.com/oauth/guide/oauth-sign-plaintext.html
            );
            using (StreamReader read = new StreamReader(request.GetResponse().GetResponseStream(), true))
                return read.ReadToEnd();
        }
    }

    public static class StringExtensions
    {
        /// <summary>
        ///   Oauth-compliant Url Encoder.  The default .NET
        ///   encoder outputs the percent encoding in lower case.  While this
        ///   is not a problem with the percent encoding defined in RFC 3986,
        ///   OAuth (RFC 5849) requires that the characters be upper case
        ///   throughout. Also, Twitter doesn't like space encoded as +, so we're replacing it with %20.
        /// </summary>
        /// <param name="s">The value to encode</param>
        /// <returns>the Url-encoded version of that string</returns>
        public static string OAuthUrlEncode(this string s)
        {

            //http://stackoverflow.com/a/1916028/188740

            var value = HttpUtility.UrlEncode(s);
            return Regex.Replace(value, "(%[0-9a-f][0-9a-f])", c => c.Value.ToUpper()).Replace("+", "%20");
        }
    }
}
