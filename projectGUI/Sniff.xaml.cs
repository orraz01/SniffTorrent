using System;
using System.Data;
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
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net.Sockets;
using System.Net;
using Microsoft.Win32;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;




namespace projectGUI
{
    /// <summary>
    /// Interaction logic for Sniff.xaml
    /// </summary>
    public partial class Sniff : Window
    {
        private SslStream socket;

        private string link="";
        public Sniff(SslStream socket)
        {
            InitializeComponent();
            this.socket = socket;
        }
        public byte[] ReceiveData(int size)
        {
            int bufferSize = 1024;
            byte[] data = new byte[size];
            int offset = 0;

            while (offset < size)
            {
                int remaining = size - offset;
                int bytesToReceive = Math.Min(bufferSize, remaining);
                byte[] buffer = new byte[bytesToReceive];

                int bytesRead = socket.Read(buffer,0,buffer.Length);
                if (bytesRead <= 0)
                {
                    throw new Exception("Socket connection closed while receiving data.");
                }

                Array.Copy(buffer, 0, data, offset, bytesRead);

                offset += bytesRead;
            }

            return data;
        }
        public void Back_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            socket.Close();
            this.Close();
        }
        public static DataSet ByteArrayToDataSet(byte[] byteArray)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(byteArray))
            {
                object obj = binaryFormatter.Deserialize(memoryStream);
                return obj as DataSet;
            }
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
        public void Check_Input(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Grid grid = (Grid)FindName("Sniff_Grid");

                    for (int i = grid.Children.Count - 1; i >= 0; i--)
                    {
                        UIElement child = grid.Children[i];
                        int childRowIndex = Grid.GetRow(child);

                        if (childRowIndex == 2)
                        {
                            grid.Children.Remove(child);
                        }
                    }

                    string input = this.input.Text;
                    this.input.Clear();
                    Label FileName = new Label();
                    FileName.Content = "File Name: " + input;
                    Grid.SetRow(FileName, 2);
                    FileName.FontFamily = new FontFamily("Myriad SemiExtended Bold");
                    FileName.FontSize = 20;
                    FileName.VerticalContentAlignment = VerticalAlignment.Top;
                    FileName.Margin = new Thickness(300, 0, 0, 0);
                    FileName.FontWeight = FontWeights.Bold;
                    FileName.Foreground = new SolidColorBrush(Colors.White);
                    grid.Children.Add(FileName);
                    byte[] byteArray;
                    if(input.Length == 0)
                    {
                        input = "%";
                    }
                    byteArray = BitConverter.GetBytes(input.Length);
                    socket.Write(byteArray);
                    socket.Flush();
                    Console.WriteLine(input.Length);
                    socket.Write(Encoding.UTF8.GetBytes(input));
                    socket.Flush();
                    try
                    {
                        byte[] lengthByte = ReceiveBytesFromStream(socket, 4);
                        int length = BitConverter.ToInt32(lengthByte, 0);
                        
                        byte[] databytes = new byte[length];
                        databytes=ReceiveData(length);
                        if(length>1)
                        {
                            DataSet dataTable = ByteArrayToDataSet(databytes);

                            myDataGrid.ItemsSource = dataTable.Tables[0].DefaultView;

                            myDataGrid.Visibility = Visibility.Visible;
                        }
                       
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Line: "+ex.StackTrace + "Error: " + ex.Message);

            }
        }
        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Get the selected row
                var row = (DataGridRow)sender;
                var data = row.Item as DataRowView;

                // Get the infohash from the FileId column
                var infohash = data["file_id"].ToString();

                // Construct the search URL for qBittorrent

                // Open qBittorrent with the search URL
                if(link=="")
                {
                    MessageBox.Show("Link Client");
                }
                else
                {
                    Process.Start(link, $"\"magnet:?xt=urn:btih:{infohash}\"");
                }
            }
            /*if (placeRow != null)
            {
                DataRowView row = placeRow.Item as DataRowView;
                string infohash = row.ToString().Substring(row.ToString().Length-21,20);
                MessageBox.Show(infohash);
                if (!string.IsNullOrEmpty(infohash))
                {
                    string qbittorentPath = "C:\\Program Files\\qBittorrent\\qbittorrent.exe";
                    Process.Start(qbittorentPath, $"/s {infohash}");

                }
            }
        }*/
            catch (Exception ex)
            {
                // Display an error message in a popup window
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Link_Client(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Torrent Client Executable|*.exe";
            openFileDialog.Title = "Select Torrent Client";

            if (openFileDialog.ShowDialog() == true)
            {
                this.link = openFileDialog.FileName;
            }
        }
    }
}
