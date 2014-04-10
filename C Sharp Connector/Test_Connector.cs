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
            string type = "web page";
            int state = 1;
            int row_inserted = trial.do_insert_url(source, link, type, state);
            Console.WriteLine("Success Row = "+row_inserted);
            Console.ReadLine();


        }//end Main

    }//end class
}//end namespace
