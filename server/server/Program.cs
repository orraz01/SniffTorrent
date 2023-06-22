using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Data;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace server
{
    class Program
    {

        static byte[] DataSetToByteArray(DataSet dataSet)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, dataSet);
                return memoryStream.ToArray();
            }
        }
        static void SendData(SslStream socket, byte[] data)
        {
            int bufferSize = 1024;
            int offset = 0;

            while (offset < data.Length)
            {
                int remaining = data.Length - offset;
                int bytesToSend = Math.Min(bufferSize, remaining);
                byte[] buffer = new byte[bytesToSend];

                Array.Copy(data, offset, buffer, 0, bytesToSend);

                socket.Write(buffer);
                socket.Flush();

                offset += bytesToSend;
            }
        }
        public static bool IsUsernameAvailable(string username)
        {
            using (MySqlConnection connection = new MySqlConnection("server=localhost;user id=root;password=214616872Raz;database=dht"))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM registereduser WHERE username = @username collate utf8mb4_bin";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username.Replace("'","''"));

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count == 0;
                }
            }
        }
        static void SendMessage(SslStream socket, string input)
        {
            byte[] byteArray = BitConverter.GetBytes(input.Length);
            socket.Write(byteArray);
            socket.Flush();
            Console.WriteLine(input.Length);
            byte[] msg = Encoding.UTF8.GetBytes(input);
            socket.Write(msg);
        }
        static byte[] ReceiveBytesFromStream(SslStream sslStream, int size)
        {
            byte[] buffer = new byte[size];
            int bytesRead = 0;

            while (bytesRead < size)
            {
                int bytesReceived = sslStream.Read(buffer, bytesRead, size - bytesRead);
                if (bytesReceived == 0)
                {
                    // Handle connection closed or error condition
                    break;
                }

                bytesRead += bytesReceived;
            }

            return buffer;
        }
        static void Main(string[] args)
        {


            while (true)
            {
                TcpListener server = new TcpListener(IPAddress.Any, 5938);
                Console.WriteLine("created server");
                try
                {
                    server.Start();

                    // Accept a client connection
                    TcpClient clientsocket = server.AcceptTcpClient();
                    Console.WriteLine("accepted connection");
                    // Create an SSL/TLS stream
                    SslStream client = new SslStream(clientsocket.GetStream(), false);

                    // Create an X509Certificate2 object from the server certificate file
                    X509Certificate2 serverCertificate = new X509Certificate2("C:\\Users\\Administrator\\source\\repos\\server\\certificate\\cert.pfx", "rOfn+q4zdm6OU5vM+WuzMhdy/JpVumtrMrSVmHs8tu8=");

                    // Authenticate the server
                    client.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, enabledSslProtocols: SslProtocols.Tls, checkCertificateRevocation: true);

                    while (true)
                    {
                        byte[] lengthByte = ReceiveBytesFromStream(client, 4);
                        int length = BitConverter.ToInt32(lengthByte, 0);
                        Console.WriteLine(length);
                        byte[] databytes = ReceiveBytesFromStream(client, length);
                        string msg = Encoding.UTF8.GetString(databytes);


                        string[] msgSplit = msg.Split(' ');
                        string connectionString = "server=localhost;user id=root;password=214616872Raz;database=dht";
                        var connection = new MySqlConnection(connectionString);
                        connection.Open();
                        Console.WriteLine("input: "+msg);
                        string action = msgSplit[0];
                        string username = msgSplit[1];
                        string password = msgSplit[2];
                        if (action == "Register:")
                        {
                            if (username == "")
                            {
                                SendMessage(client, "enter username");
                            }
                            else if (password == "")
                            {
                                SendMessage(client, "enter password");
                            }
                            else if (!IsUsernameAvailable(username))
                            {
                                SendMessage(client, "username already exists");
                            }
                            else
                            {
                                string hash = PasswordHashing.GeneratePasswordHash(password);
                                string query = "INSERT INTO registereduser (username, passwordhash) VALUES (@value1, @value2)";
                                MySqlCommand command = new MySqlCommand(query, connection);

                                // add parameters to your MySqlCommand object with the data you want to insert into your table
                                command.Parameters.AddWithValue("@value1", username.Replace("'", "''"));
                                command.Parameters.AddWithValue("@value2", hash.Replace("'", "''"));
                                command.ExecuteNonQuery();
                                SendMessage(client, "Registered into system");

                            }
                        }
                        else if (action == "Login:")
                        {
                            if (username == "")
                            {
                                SendMessage(client, "enter username");
                            }
                            else if (password == "")
                            {
                                SendMessage(client, "enter password");
                            }
                            else if (IsUsernameAvailable(username))
                            {
                                SendMessage(client, "username doesnt exist");
                            }
                            else
                            {
                                string query = $"SELECT passwordhash FROM registereduser WHERE username = @username collate utf8mb4_bin";
                                using (MySqlCommand command = new MySqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@username", username.Replace("'", "''"));
                                    string hashpassword = command.ExecuteScalar() as string;
                                    if (PasswordHashing.VerifyPassword(password, hashpassword))
                                    {
                                        SendMessage(client, "Success");
                                        break;
                                    }
                                    SendMessage(client, "password or username are incorrect");

                                }

                            }
                        }
                    }
                    while (true)
                    {
                        byte[] lengthByte = ReceiveBytesFromStream(client, 4);
                        int length = BitConverter.ToInt32(lengthByte, 0);
                        byte[] databytes = ReceiveBytesFromStream(client, length);
                        string msg = Encoding.UTF8.GetString(databytes);
                        Console.WriteLine(msg);
                        string connectionString = "server=localhost;user id=root;password=214616872Raz;database=dht";
                        var connection = new MySqlConnection(connectionString);
                        MySqlCommand command = new MySqlCommand($"SELECT user.user_id,user.ip, files.file_name, files.file_id FROM connector JOIN user ON user.user_id = connector.user_id JOIN files ON connector.file_id = files.file_id WHERE files.file_name LIKE '%{msg.Replace("'", "''")}%' limit 10000", connection);

                        try
                        {

                            connection.Open();
                            MySqlDataAdapter dataAdapter = new MySqlDataAdapter(command);
                            DataSet dataTable = new DataSet();
                            dataAdapter.Fill(dataTable);
                            byte[] input = DataSetToByteArray(dataTable);
                            byte[] byteArray;
                            Console.WriteLine(input.Length);
                            if (input.Length == 0)
                            {
                                byteArray = BitConverter.GetBytes(1);
                            }
                            else
                            {
                                byteArray = BitConverter.GetBytes(input.Length);
                            }
                            client.Write(byteArray);
                            client.Flush();
                            SendData(client, input);
                        }
                        catch (MySqlException ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                        finally
                        {
                            connection.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    server.Stop();
                }
            }

        }
    }
}
