using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CSHARP_MAIN_CRAWLER
{
    /// <summary>
    /// (The actual crawler) Controls all of the workers.
    /// Keeps track of what the workers are going to do, are doing, and have done.
    /// </summary>
    public class CEO
    {
        #region Attributes
        /// <summary>
        /// The work needed to be assigned to a worker.
        /// </summary>
        public List<List<string>> SCAN_WORK, MOVE_WORK, TEST_WORK, UPDATE_WORK;

        /// <summary>
        /// The workers working on a particular type of work.
        /// </summary>
        public List<WORKER> SCAN_WORKERS, MOVE_WORKERS, TEST_WORKERS, UPDATE_WORKERS, IDLE_WORKERS, READY_WORKERS;

        /// <summary>
        /// The threads the ceo uses to get work done.
        /// </summary>
        private Thread grab_data, assign_work, find_idle;

        /// <summary>
        /// Tells whether or not the ceo and its workers have finished all of the work.
        /// </summary>
        private bool done = false;

        /// <summary>
        /// Tells whether or not the ceo was requested to stop work.
        /// </summary>
        private bool stopped = false;

        /// <summary>
        /// Tells whether or not the thread grab_data is working.
        /// </summary>
        public bool grab_working = false;

        /// <summary>
        /// Tells whether or not the thread find_idle is working.
        /// </summary>
        public bool find_working = false;
        #endregion

        #region Constructors
        /// <summary>
        /// (The actual crawler) Controls all of the workers.
        /// Keeps track of what the workers are going to do, are doing, and have done.
        /// </summary>
        /// <param name="thread_count">The amount of threads to create.</param>
        public CEO(int thread_count, List<List<string>> starting_domains)
        {
            SCAN_WORKERS = new List<WORKER>();
            MOVE_WORKERS = new List<WORKER>();
            TEST_WORKERS = new List<WORKER>();
            UPDATE_WORKERS = new List<WORKER>();
            IDLE_WORKERS = new List<WORKER>();
            READY_WORKERS = new List<WORKER>();
            SCAN_WORK = new List<List<string>>();
            MOVE_WORK = starting_domains;//Starts the cycle
            TEST_WORK = new List<List<string>>();
            UPDATE_WORK = new List<List<string>>();
            for (int i = 0; i < thread_count; ++i)
                READY_WORKERS.Add(new WORKER());
        }

        public void GO()
        {
            foreach (var i in READY_WORKERS)
                i.GO();
            grab_data = new Thread(GRAB_DATA);
            assign_work = new Thread(ASSIGN_WORK);
            find_idle = new Thread(FIND_IDLE);
            
            find_idle.Start();
            grab_data.Start();
            assign_work.Start();
        }
        #endregion

        #region grab data thread functions
        /// <summary>
        /// Moves the work stored in the workers' logs into the appropriate work list;
        /// </summary>
        public void GRAB_DATA()
        {
            while (true)
            {
                grab_working = true;
                //grab from db to see if asked to stop.(do not check if already know it was asked to stop)
                if (IDLE_COUNT > 0)
                {
                    //pop off an idle worker
                    WORKER w = IDLE_WORKERS[0];
                    IDLE_WORKERS.RemoveAt(0);

                    //figure out what the worker just worked on
                    var prev = w.prevtype;

                    //dump the data
                    var data = w.DUMP;

                    //check to see if stopped
                    //if stopped block flow to scan from update
                    //also block if outside of domain

                    //grab the correct work to dump data into
                    var l = prev == "scan" ? SCAN_WORK :
                            prev == "move" ? MOVE_WORK :
                            prev == "test" ? TEST_WORK :
                            UPDATE_WORK;

                    //move data to the correct work
                    foreach (var i in data)
                    {
                        Console.WriteLine(i[1] + i[2]);
                        l.Add(i);
                    }

                    //Add it to the ready queue
                    READY_WORKERS.Add(w);
                }
                grab_working = false;
            }
        }
        #endregion

        #region assign work thread functions
        /// <summary>
        /// Assigns work stored in the work lists and assign it to workers.
        /// </summary>
        public void ASSIGN_WORK()
        {
            while (!done)
            {
                if (READY_COUNT > 0)
                {
                    //pop off of ready queue
                    WORKER w = READY_WORKERS[0];

                    //calculate ratios
                    int a = SCAN_WORK.Count / (SCAN_COUNT + 1);
                    int b = MOVE_WORK.Count / (MOVE_COUNT + 1);
                    int c = TEST_WORK.Count / (TEST_COUNT + 1);
                    int d = UPDATE_WORK.Count / (UPDATE_COUNT + 1);

                    //The work to be taken away from
                    var work = SCAN_WORK;

                    //Where the worker will be placed
                    var work_area = SCAN_WORKERS;

                    //type of work
                    string type = "scan";

                    //find who is the largest
                    if (a < b)
                    {
                        a = b;
                        work = MOVE_WORK;
                        work_area = MOVE_WORKERS;
                        type = "move";
                    }
                    if (a < c)
                    {
                        a = c;
                        work = TEST_WORK;
                        work_area = TEST_WORKERS;
                        type = "test";
                    }
                    if (a < d)
                    {
                        a = d;
                        work = UPDATE_WORK;
                        work_area = UPDATE_WORKERS;
                        type = "update";
                    }

                    if(a != 0)
                    {
                        //give work to the worker
                        w.WAKE_UP(work[0], type);
                        work.RemoveAt(0);

                        //move the worker to the appropriate worker group
                        //if stopped and prev was update then block flow or if outside domain
                        work_area.Add(w);
                        READY_WORKERS.RemoveAt(0);
                    }
                }
                //calculate if done or not. 
                //(just need to check work needed to be done, idle workers, ready workers, and working workers)
                //done = READY_COUNT == TOTAL_WORKERS &&
                //       SCAN_WORK.Count == 0 &&
                //       MOVE_WORK.Count == 0 &&
                //       TEST_WORK.Count == 0 &&
                //       UPDATE_WORK.Count == 0 &&
                //       !grab_working &&
                //       !find_working;
            }
            //kill other threads
            find_idle.Abort();
            grab_data.Abort();
            //kill all workers
            foreach (var i in READY_WORKERS)
                i.KILL();
            //talk to db to say done
            Console.WriteLine("done!!!");
            //kill self
            assign_work.Abort();
        }
        #endregion

        #region find idle workers functions
        /// <summary>
        /// Locates idle workers and moves them to the idle worker list to be workered on.
        /// </summary>
        public void FIND_IDLE()
        {
            while(true)
            {
                find_working = true;
                if (SCAN_COUNT > 0 || MOVE_COUNT > 0 || TEST_COUNT > 0 || UPDATE_COUNT > 0)
                {
                    //locate idle workers and move to idle list
                    int c = SCAN_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (SCAN_WORKERS[i].sleeping)
                        {
                            IDLE_WORKERS.Add(SCAN_WORKERS[i]);
                            SCAN_WORKERS.RemoveAt(i);
                        }
                    c = MOVE_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (MOVE_WORKERS[i].sleeping)
                        {
                            IDLE_WORKERS.Add(MOVE_WORKERS[i]);
                            MOVE_WORKERS.RemoveAt(i);
                        }
                    c = TEST_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (TEST_WORKERS[i].sleeping)
                        {
                            IDLE_WORKERS.Add(TEST_WORKERS[i]);
                            TEST_WORKERS.RemoveAt(i);
                        }
                    c = UPDATE_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (UPDATE_WORKERS[i].sleeping)
                        {
                            IDLE_WORKERS.Add(UPDATE_WORKERS[i]);
                            UPDATE_WORKERS.RemoveAt(i);
                        }
                }
                find_working = false;
            }
        }
        #endregion

        #region COUNTS
        /// <summary>
        /// The amount of workers this CEO is in charge of.
        /// </summary>
        public int TOTAL_WORKERS
        {
            get { return SCAN_COUNT + MOVE_COUNT + TEST_COUNT + UPDATE_COUNT + IDLE_COUNT + READY_COUNT; }
        }

        /// <summary>
        /// The amount of workers who are idle.
        /// </summary>
        public int IDLE_COUNT
        {
            get { return IDLE_WORKERS.Count; }
        }

        /// <summary>
        /// The amount of workers who are scanning web pages.
        /// </summary>
        public int SCAN_COUNT
        {
            get { return SCAN_WORKERS.Count; }
        }

        /// <summary>
        /// The amount of workers who are moving data to the database.
        /// </summary>
        public int MOVE_COUNT
        {
            get { return MOVE_WORKERS.Count; }
        }

        /// <summary>
        /// The amount of workers who are testing links to see what they are.
        /// </summary>
        public int TEST_COUNT
        {
            get { return TEST_WORKERS.Count; }
        }

        /// <summary>
        /// The amount of workers who are updating the database to what type the urls are.
        /// </summary>
        public int UPDATE_COUNT
        {
            get { return UPDATE_WORKERS.Count; }
        }

        /// <summary>
        /// The amount of workers who are testing links to see what they are.
        /// </summary>
        public int READY_COUNT
        {
            get { return READY_WORKERS.Count; }
        }
        #endregion
    }
}
