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
            //string s = GET_CONTENTS("http://localhost:8080/Forager/page_test.html");
            //Console.WriteLine(s);
            //List<string> l = GET_FROM_CONTENTS(s, new List<string[]> {
            //                                                        new string[] { "href", "=" },
            //                                                        new string[] { "src", "=" },
            //                                                        new string[] { "include", "(" }});
            //foreach (var i in l)
            //    Console.WriteLine(i);
            //Console.WriteLine();
            //string[] a = TEST_URL("http://localhost:8080/Forager/broken.png");
            //foreach (var i in a)
            //    Console.WriteLine(i);
            //a = TEST_URL("http://localhost:8080/Forager/page_test.html");
            //foreach (var i in a)
            //    Console.WriteLine(i);
            //a = TEST_URL("http://localhost:8080/Forager/Combinatorics and graph theory - Harris.pdf");
            //foreach (var i in a)
            //    Console.WriteLine(i);
            //a = TEST_URL("http://localhost:8080/Forager/Test.php");
            //foreach (var i in a)
            //    Console.WriteLine(i);
            //Console.ReadLine();
            //string[] s = EXTRACT_SOURCE_AND_LINK("http://localhost:8080/Forager/page_test.html");
            //foreach (var i in s)
            //    Console.WriteLine(i);
            //Console.ReadLine();
            //CEO ceo = new CEO(0, 4, 4, new List<List<string>> { 
            //                                             new List<string> { "-1",
            //                                                                "http://localhost:8080/Forager/",
            //                                                                "page_test.html" } 
            //                                            });
            CEO ceo = new CEO(0, 2, 20, new List<List<string>> { 
                                                         new List<string> { "-1",
                                                                            "http://Americanyeti.com/",
                                                                            "" } 
                                                        });
            ceo.GO();
            Console.ReadLine();
        }
    }
}
