using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CSHARP_MAIN_CRAWLER
{
    public class Connector
    {

        // Will have to get the scan Id number to 
        // use in the url table and the link_rel table

        // Either object gets created with it or
        // I have to go find it at some point 
        // before the scan gets going to ensure 
        // this trash will work. 

        // last_insert_id() will only work
        // if each thread creates it's own connection
        // POTENTIAL THREAD UNSAFE IF NOT FOLLOWED

        // Necessary for connections
        public MySqlConnection connection;
        public string server;
        public string database;
        public string user;
        public string password;

        // This ID will provide the scan ID
        public int id;
        public string url_table;
        public string link_rel_table;

        //Constructor
        public Connector()
        {
            init();
            id = do_check_started();
            url_table = "url" + id;
            link_rel_table = "link_rel" + id;
        }

        //Initialize values
        private void init()
        {
            server = "localhost";
            database = "db_forager";
            user = "root";
            password = "";
            string str = "server=" + server + ";database=" + database + ";userid=" + user + ";password=" + password + ";";
            connection = new MySqlConnection(str);
        }

        //open connection to database
        public bool openConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Failed OPEN Connection to DB: " + e.StackTrace);
                return false;
            }

        }

        //Close connection
        public bool closeConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Failed CLOSE Connection to DB: " + e.StackTrace);
                return false;
            }
        }


        public int do_get_scan_id()
        {
            string query = "SELECT scan_id FROM `scan` WHERE is_running = 1";
            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                MySqlDataReader reader;
                reader = cmd_query.ExecuteReader();
                if (reader.Read())
                {
                    int url_id = reader.GetInt32("url_id");
                    reader.Close();
                    this.closeConnection();
                    return url_id;
                }
                else
                {
                    reader.Close();
                    this.closeConnection();
                    return -1;
                }

            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
                return -1;
            }
        }

        // Will refactor into one method with multiple SQL Statements
        // 1. Check Scan_id and set variable to id
        // 2. Create url Table
        // 3. Create link_rel Table
        public void do_create_scan_tables(int scan_id)
        {
            url_table = "url" + scan_id;
            link_rel_table = "link_rel" + scan_id;

            string query = "CREATE TABLE IF NOT EXISTS`" + url_table + "`(";
            query += "`url_id` int(11) NOT NULL AUTO_INCREMENT,";
            query += "`url` varchar(1000) NOT NULL,";
            query += "`domain` varchar(1000) NOT NULL,";
            query += "`link` varchar(1000) NOT NULL,";
            query += "`source` varchar(1000) NOT NULL,";
            query += "`url_type` int(11) NOT NULL,";
            query += "`status_code` int(11) NOT NULL,";
            query += "`status_code_type` varchar(1000) NOT NULL,";
            query += "`state` tinyint(1) NOT NULL,";
            query += " PRIMARY KEY (`url_id`)";
            query += ") ENGINE=InnoDB DEFAULT CHARSET=latin1 AUTO_INCREMENT=1;";

            query += "CREATE TABLE IF NOT EXISTS`" + link_rel_table + "`(";
            query += "`url_id` int(11) NOT NULL,";
            query += "`dest_id` int(11) NOT NULL";
            query += ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                cmd_query.ExecuteNonQuery();
                this.closeConnection();
            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
            }
        }

        public int do_check_url(string source, string link)
        {
            string query = "SELECT url_id FROM `" + url_table + "` WHERE source = '" + source + "' AND link = '" + link + "' ";
            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                MySqlDataReader reader;
                reader = cmd_query.ExecuteReader();
                // Check reader documentation April 17 JUSTIN
                if (reader.Read())
                {
                    int url_id = reader.GetInt32("url_id");
                    reader.Close();
                    this.closeConnection();
                    return url_id;
                }
                else
                {
                    reader.Close();
                    this.closeConnection();
                    return -1;
                }
            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
                return -1;
            }
        }


        public int do_insert_url(string source, string link)
        {

            string url = source + link;
            string domain = source.Substring(0, source.IndexOf(".edu") + 4);
            string query = "INSERT INTO `" + url_table + "`(`url`,`domain`,`link`,`source`) VALUES('" + url + "','" + domain + "','" + link + "','" + source + "'); ";
            query += "SELECT last_insert_id();";
            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                int inserted_id = Convert.ToInt32(cmd_query.ExecuteScalar());
                this.closeConnection();
                return inserted_id;
            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
                return -1;
            }

        }

        public void do_insert_link_rel(int url_id, int dest_id)
        {
            string query = "INSERT INTO `" + link_rel_table + "`(`url_id`,`dest_id`)VALUES(" + url_id + "," + dest_id + "); ";
            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                cmd_query.ExecuteNonQuery();
                this.closeConnection();
            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
            }

        }

        public void do_update_url_status(int url_id, int status_code, string status_code_type, int state, int url_type)
        {
            string query = "UPDATE `" + url_table + "` SET status_code = " + status_code + ", status_code_type = '" + status_code_type + "', state = " + state + ", url_type = " + url_type + " WHERE url_id = " + url_id + ";";
            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                cmd_query.ExecuteNonQuery();
                this.closeConnection();
            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
            }

        }

        public void do_scanner_stop_updates()
        {
            // run a query that sets is_running = 0
            // also calculates number of errors and 
            // number of pages scanned to update
            // the appropriate scan row in the scan table
            // ??? HOW DO WE KNOW WHAT WAS A PAGE AND WHAT WAS NOT ???

            string error_count_query = "SET @errorVar = (SELECT COUNT(*) FROM `" + url_table + "` WHERE state = 0); ";
            string pages_count_query = "SET @pagesVar = (SELECT COUNT(*) FROM `" + url_table + "` WHERE url_type = 0); ";

            string update_query = "UPDATE `scan` ";
            update_query += "SET pages_scanned = @pagesVar, number_errors = @errorVar, is_running = 0 ";
            update_query += "WHERE scan_id = " + id + ";";

            string compound_query = error_count_query + pages_count_query + update_query;

            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(compound_query, connection);
                cmd_query.ExecuteNonQuery();
                this.closeConnection();
            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
            }
        }

        public int do_check_started()
        {
            string query = "SELECT scan_id FROM `scan` WHERE is_started = 1 ;";
            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                MySqlDataReader reader;
                reader = cmd_query.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    this.closeConnection();
                    return 1;
                }
                else
                {
                    reader.Close();
                    this.closeConnection();
                    return 0;
                }
            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
                return -1;
            }
        }


        public int do_check_stop()
        {
            string query = "SELECT * FROM `scan` WHERE scan_id = " + id + " AND is_started = 0;";
            if (this.openConnection() == true)
            {
                MySqlCommand cmd_query = new MySqlCommand(query, connection);
                MySqlDataReader reader;
                reader = cmd_query.ExecuteReader();
                if (reader.Read())
                {
                    reader.Close();
                    this.closeConnection();
                    return 1;
                }
                else
                {
                    reader.Close();
                    this.closeConnection();
                    return 0;
                }

            }
            else
            {
                Console.WriteLine("Broke.....BUT YOU WERE THE CHOSEN ONE!");
                return -1;
            }
        }


    }//end Class
}//end NameSpace
