using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Forager_Tester
{
    class Connector
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
        public Connector(int id)
        {
            init();
            this.id = id;
            url_table = "url"; //+id;
            link_rel_table = "link_rel";// +id;
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

        public int do_check_url(string source, string link)
        {
            string query = "SELECT url_id FROM `" + url_table + "` WHERE source = '" + source + "' AND link = '" + link + "' ";
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


        public int do_insert_url(string source, string link, string type, int state)
        {

            string url = source + link;
            string domain = source.Substring(0, source.IndexOf(".edu") + 4);
            string query = "INSERT INTO `" + url_table + "`(`url`,`domain`,`link`,`source`,`type`,`state`) VALUES('" + url + "','" + domain + "','" + link + "','" + source + "','" + type + "','" + state + "'); ";
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
            string query = "INSERT INTO `" + link_rel_table + "`(`url_id`,`dest_id`)VALUES('" + url_id + "','" + dest_id + "'); ";
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

        public int do_check_running()
        {
            string query = "SELECT scan_id FROM `scan` WHERE is_running = 1 ;";
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
        