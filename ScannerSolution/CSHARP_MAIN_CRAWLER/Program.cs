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

        static void Main(string[] args)
        {
            CEO ceo = new CEO(0, 100, new List<List<string>> { 
                                                         new List<string> { "-1",
                                                                            "http://www.spsu.edu/",
                                                                            "" }
                                                        });
            ceo.GO();
            //Console.ReadLine();
        }
    }
}
