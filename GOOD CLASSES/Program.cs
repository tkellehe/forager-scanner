using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Web.RegularExpressions;
using System.IO;
using MySql.Data.MySqlClient;

namespace CSHARP_MAIN_CRAWLER
{
    class Program
    {
        /// <summary>
        /// Grab the type of url.
        /// </summary>
        /// <param name="url">The url you want to test.</param>
        /// <returns>
        /// [0]=>word for error [1]=>error number
        /// [2]=>(1/0)
        /// is or is not a web page
        /// [3]=>(1/0)can or cannot connect/exists
        /// </returns>
        public static string[] TEST_URL(string url)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
            httpReq.AllowAutoRedirect = false;

            int i = 400;
            string s = "BadRequest";
            string r = "0";
            try
            {
                HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse();

                //Close connections
                httpRes.Close();

                //get the actual number
                i = (int)httpRes.StatusCode;

                s = httpRes.StatusCode.ToString();

                r = httpRes.ContentType.Contains("html") ? "1" : "0";
            }
            catch (Exception e) { }

            return new string[] { s, i + "", r, i < 400 ? "1" : "0" };
        }
        static void Main(string[] args)
        {
  

            CEO ceo = new CEO(0, 10, new List<List<string>> {  new List<string> { "-1","http://www.spsu.edu/","" }  });
            ceo.GO();
            while (!ceo.stopped) ;
            ceo.done = true;
            Environment.Exit(0);
        }
    }
}
