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
        /// The amount of workers/threads that this ceo is in charge of.
        /// </summary>
        public int total_threads;

        /// <summary>
        /// The id of the scan which used for connections to the database.
        /// </summary>
        public int scan_id;

        /// <summary>
        /// The max number of items to be scanned before the scan shuts itself down.
        /// (-1 for continue until all found)
        /// </summary>
        public int max;

        /// <summary>
        /// Counter to keep track of the amount of items scanned.
        /// </summary>
        public int count = 0;

        /// <summary>
        /// The object used to connect to the database.
        /// </summary>
        public Connector connect;

        /// <summary>
        /// A enumerated type to indicate the type of work a worker is doing.
        /// </summary>
        public enum WORK_TYPE {scan, move, test, update, idle }

        /// <summary>
        /// Tells when the ceo has officially killed itself.
        /// </summary>
        public bool dead = false;
        #endregion

        #region Constructors
        /// <summary>
        /// (The actual crawler) Controls all of the workers.
        /// Keeps track of what the workers are going to do, are doing, and have done.
        /// </summary>
        /// <param name="thread_count">The amount of threads to create.</param>
        /// <param name="starting_domains">The first links to start the scan on.</param>
        /// <param name="id">The scan id for this scan.</param>
        /// <param name="max">The max number of items to be scanned before the scan shuts itself down. 
        /// (-1 for continue until all found)</param>
        public CEO(int max, int thread_count, List<List<string>> starting_domains)
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
            connect = new Connector();
            connect.do_create_scan_tables(connect.do_check_started());
            for (int i = 0; i < thread_count; ++i)
                READY_WORKERS.Add(new WORKER());
            total_threads = thread_count;
            this.max = max;
            
        }
        /// <summary>
        /// The method that initializes the threads
        /// </summary>
        public void GO()
        {
            foreach (var i in READY_WORKERS)
                i.GO();
            CEO ceo = this;
            grab_data = new Thread(()=>GRAB_DATA(ref ceo));
            assign_work = new Thread(()=>ASSIGN_WORK(ref ceo));
            find_idle = new Thread(()=>FIND_IDLE(ref ceo));
            
            find_idle.Start();
            grab_data.Start();
            assign_work.Start();
        }
        #endregion

        #region grab data thread functions
        /// <summary>
        /// Moves the work stored in the workers' logs into the appropriate work list;
        /// </summary>
        public void GRAB_DATA(ref CEO ceo)
        {
            while (true)
            {
                //Make where all db interaction happens here (might be easy...)
                if(!stopped)
                {
                    //get from db
                    if (ceo.connect.do_check_stop() == 1)
                        stopped = true;
                    //Check count verses max
                    if (max != 0 && count >= max)
                        stopped = (ceo.connect.do_check_stop() == 1) || (max != 0 && count >= max);
                }
                if (ceo.IDLE_COUNT > 0)
                {
                    //pop off an idle worker
                    WORKER w = ceo.IDLE_WORKERS[0];
                    if (w != null)//losing worker somehow?
                    {
                        //figure out what the worker just worked on
                        var prev = w.prevtype;

                        //dump the data
                        var data = w.DUMP();

                        //grab the correct work to dump data into
                        var l = prev == WORK_TYPE.scan ? ceo.MOVE_WORK :
                                prev == WORK_TYPE.move ? ceo.TEST_WORK :
                                prev == WORK_TYPE.test ? ceo.UPDATE_WORK :
                                ceo.SCAN_WORK;
                        
                        //===========================================================================================
                        //This is used to only have one connector... dirt butt slow most likely because
                        //a worker goes and does nothing.
                        //Also, every move or update must go through here first........ lol
                        if(prev == WORK_TYPE.scan)
                        {
                            //Check to see if already in the database
                            int ID = ceo.connect.do_check_url(data[0][1], data[0][2]);

                            //If not in the database do an insert and send it to be tested
                            if (ID == -1)
                            {
                                ID = ceo.connect.do_insert_url(data[0][1], data[0][2]);
                                ceo.TEST_WORK.Add(new List<string> { ID + "", data[0][1], data[0][2] });
                            }

                            //Always do an insert into link_rel
                            //Basically if the work was the first thing to be scanned
                            if (data[0][0] != "-1")
                                ceo.connect.do_insert_link_rel(ID, Int32.Parse(data[0][0]));
                        }
                        else if(prev == WORK_TYPE.test)
                        {
                            //use the ID to move the new data found from test
                            //to update the url table
                            //if a link and a good file log for scanning
                            connect.do_update_url_status(Int32.Parse(data[0][0]), Int32.Parse(data[0][4]), data[0][3], Int32.Parse(data[0][6]), Int32.Parse(data[0][5]));

                            //If it is a good url and it is a web page create more work
                            if (data[0][5] == "1" && data[0][6] == "1")
                                ceo.SCAN_WORK.Add(new List<string> { data[0][0], data[0][1], data[0][2] });
                        }
                        //===========================================================================================

                        //if not stopped and work is not to be scanned--
                        if (!(stopped && (prev == WORK_TYPE.update || prev == WORK_TYPE.move)))
                            //move data to the correct work
                            foreach (var i in data)
                            {
                                //Make actual output of what work was just done
                                Console.WriteLine(i[1] + i[2] + " prev type :" + prev);
                                if (prev == WORK_TYPE.update)
                                {
                                    //Domain restriction
                                    if (i[1].Contains("spsu.edu"))
                                        l.Add(i);
                                }
                                else
                                    l.Add(i);

                                //Increment count of how many workers worked on scanning
                                if (prev == WORK_TYPE.scan)
                                    ++count;
                            }

                        //Add it to the ready queue
                        ceo.READY_WORKERS.Add(w);
                        ceo.IDLE_WORKERS.RemoveAt(0);

                    }// CLOSES if (w != null)
                    else
                        ceo.IDLE_WORKERS.RemoveAt(0);//Pop off if null
                }
                else
                    //If there are no idle threads then fall asleep for .1 seconds
                    Thread.Sleep(10);
            }
        }
        #endregion

        #region assign work thread functions
        /// <summary>
        /// Assigns work stored in the work lists and assign it to workers.
        /// </summary>
        public void ASSIGN_WORK(ref CEO ceo)
        {
            while (!ceo.done)
            {
                if (ceo.READY_COUNT > 0)
                {
                    //pop off of ready queue
                    WORKER w = READY_WORKERS[0];
                    if (w != null)
                    {
                        //calculate ratios
                            //Multipliers added to increase that particular
                            //type of work to be done
                            //All already 1/4 of the whole so needed to multiply the multiplier
                            //by four
                        double a = .4 * ceo.SCAN_WORK.Count / (ceo.SCAN_COUNT + 1);//10%
                        double b = .8 * ceo.MOVE_WORK.Count / (ceo.MOVE_COUNT + 1);//20%
                        double c = 1.6 * ceo.TEST_WORK.Count / (ceo.TEST_COUNT + 1);//40%
                        double d = 1.2 * ceo.UPDATE_WORK.Count / (ceo.UPDATE_COUNT + 1);//30%

                        //The work to be taken away from
                        var work = ceo.SCAN_WORK;

                        //Where the worker will be placed
                        var work_area = ceo.SCAN_WORKERS;

                        //type of work
                        WORK_TYPE type = WORK_TYPE.scan;

                        //find who is the largest
                        if (a < b)
                        {
                            a = b;
                            work = ceo.MOVE_WORK;
                            work_area = ceo.MOVE_WORKERS;
                            type = WORK_TYPE.move;
                        }
                        if (a < c)
                        {
                            a = c;
                            work = ceo.TEST_WORK;
                            work_area = ceo.TEST_WORKERS;
                            type = WORK_TYPE.test;
                        }
                        if (a < d)
                        {
                            a = d;
                            work = ceo.UPDATE_WORK;
                            work_area = ceo.UPDATE_WORKERS;
                            type = WORK_TYPE.update;
                        }

                        if (a != 0)
                        {
                            //give work to the worker
                            w.WAKE_UP(work[0], type);
                            work.RemoveAt(0);

                            //move the worker to the appropriate worker group
                            //if stopped and prev was update then block flow or if outside domain
                            work_area.Add(w);
                            ceo.READY_WORKERS.RemoveAt(0);
                        }
                    }
                    else
                        ceo.READY_WORKERS.RemoveAt(0);//Remove Null
                }
                else
                    //if no one is ready then fall asleep for .1 seconds
                    Thread.Sleep(10);
                //calculate if done or not. 
                //(just need to check work needed to be done, idle workers, ready workers, and working workers)
                ceo.done = ceo.READY_COUNT == ceo.TOTAL_WORKERS &&
                       ceo.SCAN_WORK.Count == 0 &&
                       ceo.MOVE_WORK.Count == 0 &&
                       ceo.TEST_WORK.Count == 0 &&
                       ceo.UPDATE_WORK.Count == 0;
            }

            //kill other threads
            ceo.find_idle.Abort();
            ceo.grab_data.Abort();

            //kill all workers
            foreach (var i in ceo.READY_WORKERS)
                i.KILL();

            //talk to db to say done
            Console.WriteLine("\nNUMBER: " + count + "\ndone!!!");

            //Tell database we have finished some work here
            ceo.connect.do_scanner_stop_updates();

            //Set dead to true
            ceo.dead = true;

            //kill self
            ceo.assign_work.Abort();
        }
        #endregion

        #region find idle workers functions
        /// <summary>
        /// Locates idle workers and moves them to the idle worker list to be workered on.
        /// </summary>
        public void FIND_IDLE(ref CEO ceo)
        {
            while(true)
            {
                //Make sure someone is working first
                if (ceo.SCAN_COUNT > 0 || ceo.MOVE_COUNT > 0 || ceo.TEST_COUNT > 0 || ceo.UPDATE_COUNT > 0)
                {
                    //locate idle workers and move to idle list
                    int c = ceo.SCAN_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (ceo.SCAN_WORKERS[i] != null && ceo.SCAN_WORKERS[i].sleeping)
                        {
                            ceo.IDLE_WORKERS.Add(ceo.SCAN_WORKERS[i]);
                            ceo.SCAN_WORKERS.RemoveAt(i);
                        }
                    c = ceo.MOVE_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (ceo.MOVE_WORKERS[i] != null && ceo.MOVE_WORKERS[i].sleeping)
                        {
                            ceo.IDLE_WORKERS.Add(ceo.MOVE_WORKERS[i]);
                            ceo.MOVE_WORKERS.RemoveAt(i);
                        }
                    c = TEST_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (ceo.TEST_WORKERS[i] != null && ceo.TEST_WORKERS[i].sleeping)
                        {
                            ceo.IDLE_WORKERS.Add(ceo.TEST_WORKERS[i]);
                            ceo.TEST_WORKERS.RemoveAt(i);
                        }
                    c = UPDATE_COUNT;
                    for (int i = c - 1; i >= 0; --i)
                        if (ceo.UPDATE_WORKERS[i] != null && ceo.UPDATE_WORKERS[i].sleeping)
                        {
                            ceo.IDLE_WORKERS.Add(ceo.UPDATE_WORKERS[i]);
                            ceo.UPDATE_WORKERS.RemoveAt(i);
                        }
                }
                else
                    //if no one is working then fall asleep for .1 seconds
                    Thread.Sleep(10);
            }
        }
        #endregion

        #region COUNTS
        /// <summary>
        /// The amount of workers this CEO is in charge of.
        /// </summary>
        public int TOTAL_WORKERS
        {
            get { return total_threads; }
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