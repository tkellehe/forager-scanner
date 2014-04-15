using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forager_Tester
{
    class Test_Connector
    {
        static void Main(string[] args)
        {
            Connector trial = new Connector(1);
            string source = "http://spsu.edu/schoolcse/";
            string link = "/index.htm";

            int row_inserted = trial.do_insert_url(source, link);
            Console.WriteLine("Success Row = "+row_inserted);
            Console.ReadLine();

            trial.do_update_url_status(2, 200, "Connection success", 1);
            Console.ReadLine();

            int newScan = trial.do_check_started();
            Console.WriteLine("Success Row = " + newScan);
            Console.ReadLine();
            trial.do_create_scan_tables(newScan);
            Console.ReadLine();





        }//end Main

    }//end class
}//end namespace
