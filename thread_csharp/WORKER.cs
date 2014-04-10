﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Net;
using System.Web.RegularExpressions;

namespace CSHARP_MAIN_CRAWLER
{
    /// <summary>
    /// Has a logger to keep track of its work.
    /// Also, has a thread that does its work.
    /// </summary>
    public class WORKER
    {
        #region Attributes
        /// <summary>
        /// Where the work is placed.
        /// </summary>
        public List<List<string>> work_log;
        /// <summary>
        /// The thread in charge of doing the work
        /// </summary>
        private Thread thread;
        /// <summary>
        /// The work for the thread to work on.
        /// scan = (id, source, link) 
        /// move = (fid, source, link) 
        /// test = (id, source, link) 
        /// update = (id, source, link, type, status code, link or file, good/bad)
        /// </summary>
        public List<string> work;
        /// <summary>
        /// Whether or not the thread is waiting to be given work.
        /// </summary>
        public bool sleeping = true;
        /// <summary>
        /// The type the work is.
        /// </summary>
        public string type = "idle";
        /// <summary>
        /// The type the work was.
        /// </summary>
        public string prevtype = "idle";
        /// <summary>
        /// The attributes the scanner workers are interested in
        /// </summary>
        private List<string[]> searches_in_webpages;
        #endregion

        #region Constructors
        /// <summary>
        /// Worker used to do work on specific types of work.
        /// </summary>
        public WORKER()
        {
            work_log = new List<List<string>>();
            work = new List<string>();
            searches_in_webpages = new List<string[]>{new string[] { "href", "=" },
                                                      new string[] { "src", "=" },
                                                      new string[] { "include", "(" }};
        }
        #endregion

        #region Methods
        /// <summary>
        /// Dump all of the work that has been logged by this worker.
        /// </summary>
        public List<List<string>> DUMP
        {
            get
            {
                List<List<string>> r = work_log;
                work_log.Clear();
                return r;
            }
        }

        /// <summary>
        /// Wakes up the worker and starts him working on the work given in the particular type given.
        /// </summary>
        /// <param name="new_work">The work to be worked on by the worker.</param>
        /// <param name="type_of_work">How the worker should operate on the work.</param>
        public void WAKE_UP(List<string> new_work, string type_of_work)
        {
            //get new work
            work = new_work;

            //get what type of work to do
            type = type_of_work;

            //set sleeping to false
            sleeping = false;

            //wake up thread
            thread.Interrupt();
        }

        /// <summary>
        /// Tells the worker to sleep until new work comes in.
        /// </summary>
        public void SLEEP()
        {
            //clean work
            work.Clear();

            //assign prevtype to what the current type is
            prevtype = type;

            //set the current type to idle
            type = "idle";

            //set sleeping to true
            sleeping = true;
            try
            {
                //make fall asleep
                Thread.Sleep(Timeout.Infinite);
            }
            catch (ThreadInterruptedException e) { } 
        }
        #endregion

        #region Thread functions
        /// <summary>
        /// Get a worker's thread running (immediately goes to sleep though)
        /// </summary>
        public void GO()
        {
            WORKER w = this;
            thread = new Thread(DO_WORK);
            thread.Start();
        }
        /// <summary>
        /// Aborts the worker's thread.
        /// </summary>
        public void KILL()
        {
            thread.Abort();
        }
        /// <summary>
        /// The method that the thread calls to do the work given.
        /// </summary>
        public void DO_WORK()
        {
            while(true)
            {
                //fall asleep
                SLEEP();

                //see what type I am
                if (type == "scan")
                {
                    // run scan on work
                    var data_collected = DO_COMBINATION_OF_GETS(work[1] + work[2]);
                    //process what was found then log it
                    foreach(var i in data_collected)
                    {
                        string source = work[1];
                        string link = i;

                        //if http:// is in i, then we must extract the begining from i...
                        if(i.Contains("://"))
                        {
                            string[] s = EXTRACT_SOURCE_AND_LINK(i);
                            source = s[0];
                            link = s[1];
                        }

                        work_log.Add(new List<string> { work[0], source, link });
                    }
                }
                else if (type == "move")
                {
                    //take work and push it towards the database
                    //if already exists:
                    //push towards url_link table
                    //if not:
                    //collect new ID and push it towads test
                    int ID = 0;
                    if (ID == -1)
                    { /*update link_url table*/}
                    else
                        work_log.Add(new List<string> { ID + "", work[1], work[2] });
                }
                else if (type == "test")
                {
                    //take work and run test
                    string[] s = TEST_URL(work[1] + work[2]);
                    //take and log for update
                    work_log.Add(new List<string> { work[0], work[1], work[2], s[0], s[1], s[2], s[3] });
                }
                else
                {
                    //use the ID to move the new data found from test
                    //to update the url table
                    //if a link and a good file log for scanning
                    if (work[5] == "1" && work[6] == "1")
                        work_log.Add(new List<string> { work[0], work[1], work[2] });
                }
            }
        }
        #endregion

        #region Outside interaction methods (scan and test)
        /// <summary>
        /// Used to seperate the source and link of a particular url.
        /// </summary>
        /// <param name="url">The url to split up.</param>
        /// <returns>Return the different parts. [0]=>source [1]=>link</returns>
        public static string[] EXTRACT_SOURCE_AND_LINK(string url)
        {
            int last_slash = url.LastIndexOf('/');
            int source_length = last_slash + 1;

            return new string[] { url.Substring(0, source_length), url.Substring(last_slash + 1) };
        }
        /// <summary>
        /// Combines GET_CONTENTS and GET_FROM_CONTENTS in order to only have to worry about one function.
        /// </summary>
        /// <param name="url">The url to apply everything to.</param>
        /// <returns></returns>
        public List<string> DO_COMBINATION_OF_GETS(string url)
        {
            var l = GET_FROM_CONTENTS(GET_CONTENTS(url), searches_in_webpages);
            List<string> r = new List<string>();
            foreach (var i in l)
                r.Add(CUT_OUT(i));
            return r;
        }

        /// <summary>
        /// Grabs the contents from a particular web page.
        /// </summary>
        /// <param name="url">The web page.</param>
        /// <returns>The code for the web page.</returns>
        public static string GET_CONTENTS(string url)
        {
            return new System.Net.WebClient().DownloadString(url);
        }

        /// <summary>
        /// Grabs stuff out of the given tokens
        /// </summary>
        /// <param name="s">The string you are wanting to cut.</param>
        /// <param name="c">Token to start the cut.</param>
        /// <returns>The cut out.</returns>
        public static string CUT_OUT(string s, List<string> c)
        {
            int start = s.IndexOf(c[0]);
            foreach (var token in c)
                if (start == -1)
                    start = s.IndexOf(token);
            return s.Substring(start + 1, s.Length - 1 - (start + 1));
        }

        public static string CUT_OUT(string s)
        {
            return CUT_OUT(s, new List<string> { '"' + "", "'" });
        }

        /// <summary>
        /// Uses a regex to pull everything from a particular input.
        /// </summary>
        /// <param name="input">The input (what you want to search through)</param>
        /// <param name="searches">Holds all of the attributes and the token associated with that.</param>
        /// <returns></returns>
        public static List<string> GET_FROM_CONTENTS(string input, List<string[]> searches)
        {
            //string s = @"(?:href\s*[=]|src\s*[=]|img\s*[=]|include\s*[(])\s*[\" + '"' + "\'](?<Link>.*?)[\"\']";
            string s = @"(?:" + searches[0][0] + @"\s*[" + searches[0][1] + @"]\s*";
            for (int i = 1; i < searches.Count; ++i)
                s += @"|" + searches[i][0] + @"\s*[" + searches[i][1] + @"]\s*";
            s += @")[\" + '"' + "\'](?<link>.*?)[\"\']";

            MatchCollection matches = Regex.Matches(input, s,
            RegexOptions.IgnoreCase);

            List<string> r = new List<string>();

            foreach (Match match in matches)
                foreach (Capture capture in match.Captures)
                    r.Add(CUT_OUT(capture.Value, new List<string> { '"' + "", "'" }));

            return r;
        }

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
        #endregion
    }
}
