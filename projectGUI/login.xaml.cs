using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using Microsoft.Win32;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace projectGUI
{
    /// <summary>
    /// Interaction logic for login.xaml
    /// </summary>
    public partial class login : Window
    {
        private SslStream socket;
        public login()
        {
            InitializeComponent();
            // Create a TCP socket


            TcpClient client = new TcpClient("157.90.143.119", 5938);

            // Create an SSL/TLS stream
            socket = new SslStream(client.GetStream(), false,new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            // Authenticate the client
            socket.AuthenticateAsClient("157.90.143.119");


        }
        private byte[] ReceiveBytesFromStream(SslStream sslStream, int size)
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
        private void RegisterUser(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = $"Register: {username.Text.Replace(" ", "")} {password.Password.Replace(" ", "")}";
                username.Text = "";
                password.Password = "";
                byte[] byteArray = BitConverter.GetBytes(input.Length);
                socket.Write(byteArray);
                socket.Flush();
                Console.WriteLine(input.Length);
                socket.Write(Encoding.UTF8.GetBytes(input));
                socket.Flush();
                byteArray = ReceiveBytesFromStream(socket, 4);
                int length = BitConverter.ToInt32(byteArray, 0);
                byte[] data = ReceiveBytesFromStream(socket, length);
                string response = Encoding.UTF8.GetString(data);
                this.ErrorBox.Content = response;
            }
            catch(Exception exe)
            {
                MessageBox.Show(exe.ToString());
            }
        }
        private void LoginUser(object sender, RoutedEventArgs e)
        {
            try
            {
                string input = $"Login: {username.Text.Replace(" ", "")} {password.Password.Replace(" ", "")}";
                username.Text = "";
                password.Password = "";
                byte[] byteArray = BitConverter.GetBytes(input.Length);
                socket.Write(byteArray);
                socket.Flush();
                Console.WriteLine(input.Length);
                socket.Write(Encoding.UTF8.GetBytes(input));
                socket.Flush();
                byteArray = ReceiveBytesFromStream(socket, 4);
                int length = BitConverter.ToInt32(byteArray, 0);
                byte[] data = ReceiveBytesFromStream(socket, length);
                string response = Encoding.UTF8.GetString(data);
                this.ErrorBox.Content = response;
                if (response == "Success")
                {
                    Sniff i = new Sniff(socket);
                    i.Show();
                    this.Close();
                }

            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.ToString());
            }
        }
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // You can implement custom validation logic here
            // For simplicity, we'll just return true to accept any certificate
            return true;
        }
    }
}
