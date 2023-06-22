using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ConsoleApp1
{
    class SQLHandler
    {
        private MySqlConnection connection;
        public SQLHandler()
        {
            string connectionString = "server=localhost;user id=root;password=214616872Raz;database=dht";
            connection = new MySqlConnection(connectionString);
            connection.Open();
        }
        public void AddUser(string user_id, string ip, string file_id)
        {
            try
            {
                string query = "INSERT INTO user (user_id, ip) VALUES (@value1, @value2)";
                MySqlCommand command = new MySqlCommand(query, connection);

                // add parameters to your MySqlCommand object with the data you want to insert into your table
                command.Parameters.AddWithValue("@value1", user_id);
                command.Parameters.AddWithValue("@value2", ip);
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
               
            }
            try
            {
                string query = "INSERT INTO connector (user_id, file_id) VALUES (@value1, @value2)";
                MySqlCommand command = new MySqlCommand(query, connection);

                // add parameters to your MySqlCommand object with the data you want to insert into your table
                command.Parameters.AddWithValue("@value1", user_id);
                command.Parameters.AddWithValue("@value2", file_id);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                
            }
        }
        public void AddFile(string file_id, string file_name)
        {
            try
            {
                string query = "INSERT INTO files (file_id, file_name) VALUES (@value1, @value2)";
                MySqlCommand command = new MySqlCommand(query, connection);

                // add parameters to your MySqlCommand object with the data you want to insert into your table
                command.Parameters.AddWithValue("@value1", file_id);
                command.Parameters.AddWithValue("@value2", file_name);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
            }
        }
    }
}
