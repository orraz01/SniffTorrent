using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Parsing;
using BencodeNET.Objects;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

namespace ConsoleApp1
{
    class Program
    {
        static object lockObject = new object();
        static void RunClient(int port)
        {
            MainLine_DHT client;
            lock(lockObject)
            {
                client = new MainLine_DHT(port);
                Console.WriteLine(port);
            }    
            client.start();
        }
        static void Main(string[] args)
        {
            Console.WriteLine("pick port range to run dht on (first input=first port, second input=last port)");
            int start = int.Parse(Console.ReadLine());
            int end = int.Parse(Console.ReadLine());
            for (int i =start;i<=end;)
            {

                Thread thread = new Thread(() => RunClient(i));
                thread.Start();
                Thread.Sleep(1000);
                lock(lockObject)
                {
                    i++;
                }
                
            }
            /*byte[] byteArray = new byte[] { 0x00, 0x00, 0x00, 0x6e }; // Example byte array in big-endian byte order
            int result = (byteArray[0] << 24) | (byteArray[1] << 16) | (byteArray[2] << 8) | byteArray[3]; // Convert bytes to int in big-endian byte order

            Console.WriteLine(result);
            string test = "64383a6d73675f74797065693165353a706965636569306531303a746f74616c5f73697a656931353839656564363a6c656e6774686935393838333937393065343a6e616d6536393a5468652e4c6173742e4f662e55732e5330314530372e5745425269702e343030702e7275732e43656e736f7265642e322e302e484452455a4b412e53545544494f2e61766931323a7069656365206c656e677468693833383836303865363a706965636573313434303a32e12b8c6fa611ee5f6c02427e84b0892a2d1c7f53b8dbf9a90c0b62f95902c97fe6e466ffc71c874a68ffbac85be0ef7f27fa55b4f74430a930ea1f2642dd9c8c43a6842fdc92ca9634e247b1e67c861e97d886230d9f93c4bd685749eee0d02c9cc3404794c4d4d6c582345227555d91f0423ec2789e1d50c32d70b49224d0be1660a6ba7bf59879b85d493674adf27033eb1caa32bffaefc6a822670f0f3e19b3b46fc2372dd62792158fe8e8d75a250bac5eaf663779592870f78ea33d84201f2dc3b5ddfb05e5d1910bf682a2e177010285370785f668f3ee1187db12035aa26138702209e8eedeb83d3724c521d5d640388a6ac3b243be8017b3e2dc7765cf8c73ee997820d32a2169095740e55a7cafead85a5258cefc8be5e1a7a58321a40363174042a845186d4fd9d0763cfdd64aee0340f704ce610b3104e71c4f408cb6a34a2d5a4cabd796d1ee25186524844e59505223a15fc14e961f593894c006852eab3856a8cfc8c3b34b892ffa89c08a631d27c0ac61faf3e03dc9e0a1cd318d4d92ccbfb92f1a868d116259d15dad8e54eb6c72e11753cc148b959352320611b4830ec3624bbf3ac8fa769011bc78aedbba2de57eead2a8367528eb4427b1296c05e069758bf4e3e58bc0be8e95f71a76bdf36593278738ce56209acc7302090061d831ab88fc1b12b43f6999e19f7eaf7ce40c67231ae26e2be0994ff2f9ec493b036d8a9d97bf4cb560772d0f2562e77db03881b0b1ea7e6cadec109a0fbc6031d848f5a9dfbc501eb1e66870102ec9f6ac2fa9f3be54d553f691d44d1e40cdbc8846720172b829fd7a0f814e4ea0d99a8f13fbb10f37ab79ef0fac1576021eb615173ec33bf73a365036f0a30fc397be855fa8d4f26d4db795ae34f71248516d2ff4f54f2568befe434a9284d1295af5c20adb022d90a9c67c65b7d498dc5d632f42ed63b78dd51e43131d528efe6414ef3a75a28f4fb47489100fe286bbf37488313989319c0c889de4c1a2d113afefa45057391c2384af4ce65755c402e3e41e22725111d808f2d848f4b3a80a8fe89646a8417a87c9b3d28befab14fbc536addca01c8c4776bab5df82c13d907efd7db098f6fb85dd5dc69be97679564fbfc91e272f9e0d20ec730dc62cea1ce96b7ad54eddfd9e61f67c4e4778501e6d180da18d2d7b79dc3ebe92daa4ce815037068f691f68057f1b40b71681d471d92811199a0b3ee97336fe1bd2580e741b070b2b9968c548f6e31eca2157551d80a48de1cb5ea8376b8eb40b6983fe99a5e87803ed72093682e85e7694d28a397f4d7b30c0d523ff3cd16298bd6d86c700be8a58778b483fa2cbd5fd9ba0a5a642fa05509ec5a45c2af111722924a76f6ade9247dd5ef66dc58d885e597e54bf958a6fb60d27ab00d7de7077c6c1418b41fcfffe0fe610e5cab8eb62940b9bbed5756ff404cf21248efb4a0ed153389162a88c436e3df28dea28a71b849f34113125322f5164ad50e228f7bca7f77a8ba0d97d9b3c10f32bdafc87bc629e20e8eb6acb4f308f3a42aa11b60cacc08e292921542071f0f919984c3c1210ef5c4bdf480f7049f4869f6f308586af6ea6a60af77a78d1c4bbabc654e479a339e7ffc100755026ecb96a8834abb106cf6d2cec720b99684e350780f3a5c922ff71a9c31794991d8464d7e77df37930dc5e61df95b0c21cd2b4657ce2ab9f4b89c92e55966dde4dc4191bdd4e16fa15a0187e327cf8cd231091054d153837d0a47ffb31568019b3525c953c3606b152bfd5ec8aed7c9e728ebfb166c7b07cb14beb51e05786b15daba57d3825238439e445fd8ee724d13a0679a30176327af6db17cdd8f970b0eac8b8de6f643614fdecfe069f6bb76227d5b5eb7398001455739f3fd12ca032ebb21128ee03c78e6ab5b8e8a881bc38920a908099b368acc0f7ee46fc6988d3a3cc8c65cac1823edef943f3a54ed3f06d73c3f8f381bbbc31aca65c0823d7dbe2cca57e1bb7296233e85996cc7a0f64232b1e3a8cd48213f4363a736f75726365333a525a4b65";
            var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
            BDictionary testDict = parser.Parse<BDictionary>(BMethods.ConvertHexToByte(test));
            foreach (KeyValuePair<BString, IBObject> kvp in testDict)
            {
                // Get the key and value for the current key-value pair
                BString key = kvp.Key;
                IBObject value = kvp.Value;

                // Print the key and value to the console
                Console.WriteLine("{0}: {1}", key.ToString(), value.ToString());
            }
            int length = testDict.EncodeAsBytes().Length;
            byte[] arr = BMethods.ConvertHexToByte(test);
            byte[] extractedData = new byte[arr.Length - length];

            // Copy the portion of the original array that we want to extract into the new array
            Array.Copy(arr, length, extractedData, 0, extractedData.Length);
            testDict = parser.Parse<BDictionary>(extractedData);
            foreach (KeyValuePair<BString, IBObject> kvp in testDict)
            {
                // Get the key and value for the current key-value pair
                BString key = kvp.Key;
                IBObject value = kvp.Value;

                // Print the key and value to the console
                Console.WriteLine("{0}: {1}", key.ToString(), value.ToString());
            }
            /*Random rnd = new Random();
            Console.WriteLine();
            string count = "";
            byte[] rv1 = new byte[20];
            byte[] rv2 = new byte[20];
            rnd.NextBytes(rv1);
            rnd.NextBytes(rv2);
            string query = "INSERT INTO files (file_id, file_name) VALUES (@value1, @value2)";
            connection.Open();
            MySqlCommand command = new MySqlCommand(query, connection);

            // add parameters to your MySqlCommand object with the data you want to insert into your table
            command.Parameters.AddWithValue("@value1", BMethods.BytesToId(rv1));
            command.Parameters.AddWithValue("@value2", "the joker movie 2019");

            command.ExecuteNonQuery();
            query = "INSERT INTO user (user_id, ip) VALUES (@value1, @value2)";
            command = new MySqlCommand(query, connection);

            // add parameters to your MySqlCommand object with the data you want to insert into your table
            command.Parameters.AddWithValue("@value1", BMethods.BytesToId(rv2));
            command.Parameters.AddWithValue("@value2", "122.132.232.1");

            command.ExecuteNonQuery();
            query = "INSERT INTO connector (user_id, file_id) VALUES (@value1, @value2)";
            command = new MySqlCommand(query, connection);

            // add parameters to your MySqlCommand object with the data you want to insert into your table
            command.Parameters.AddWithValue("@value1", BMethods.BytesToId(rv2));
            command.Parameters.AddWithValue("@value2", BMethods.BytesToId(rv1));

            command.ExecuteNonQuery();

            /*MainLine_DHT test = new MainLine_DHT(10022);
            test.start();
            /*Random rnd = new Random();
            Console.WriteLine();
            string count = "";
            byte[] rv1 = new byte[2];
            byte[] rv2 = new byte[20];
            rnd.NextBytes(rv1);
            rnd.NextBytes(rv2);
            string myId = BitConverter.ToString(rv1).Replace("-", string.Empty);
            Console.WriteLine(myId);
            string str;
            var parser = new BencodeParser();
            DateTime a = new DateTime(2010, 05, 12, 13, 15, 00);
            DateTime b = new DateTime(2010, 05, 12, 13, 45, 00);
            Console.WriteLine(b.Subtract(a).TotalMinutes);
            Console.WriteLine(b);
            Dictionary<IPEndPoint, Token> tokenHandler= new Dictionary<IPEndPoint, Token>();
            IPEndPoint var = new IPEndPoint(IPAddress.Parse("67.215.246.10"), 101);
            IPEndPoint var2 = new IPEndPoint(IPAddress.Parse("67.215.246.10"), 101);
            var.Equals(var2);
            byte[] place = { 12};
            tokenHandler.Add(var, new Token(place));


            string id = BMethods.BytesToString(rv2);
            string raz = BMethods.BytesToString(rv2);
            DHTNode x = new DHTNode(id, rv2, IPAddress.Parse("111.111.12.13"), 101); ;
            DHTNode y = new DHTNode(id, rv2, IPAddress.Parse("111.111.12.13"), 101); ;
            Console.WriteLine(x!=y);
            Node<int> r = new Node<int>(5, new Node<int>(3, new Node<int>(1, new Node<int>(7, new Node<int>(2, new Node<int>(11))))));
            Console.WriteLine("---"+r.Remove(2)); 
            Console.WriteLine(r);
            IPAddress[] ipAddresses = Dns.GetHostAddresses("router.bittorrent.com"); //gets ip of known bootstrapping node
            var endpoint = new IPEndPoint(ipAddresses[0], 6881); //make end point object with ip from earlier and known port
            parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
            // Encode the query as a byte array
            byte[] tkey = new byte[2];
            rnd.NextBytes(tkey);
            byte[] query = KRPC.FindNodeResponse(rv2, rv2, tkey);
            BDictionary dict = parser.Parse<BDictionary>(query);
            Console.WriteLine(dict.ContainsKey("t"));
            Console.WriteLine("here");
            Console.WriteLine(tkey.SequenceEqual((((BString)dict["t"]).Value.ToArray())));
            MainLine_DHT test = new MainLine_DHT(12000);
            test.start();
            //generates t key
            //check if new bucket works well and != operator
            /*BitArray myBA = new BitArray(raz);
            
            // Create a new bencoded dictionary for the query


            /*var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
            // Encode the query as a byte array
            byte[] tkey = new byte[2];
            rnd.NextBytes(tkey);
            Console.WriteLine(tkey.Length);
            Console.WriteLine(BMethods.BytesToId(tkey));
            var queryBytes = KRPC.find_node_query(id, id,tkey);
            // Create a new socket
            Console.WriteLine(hex);

            UdpClient udpServer = new UdpClient(11000);
            udpServer.Send(queryBytes, queryBytes.Length, endpoint); // reply back
            var data = udpServer.Receive(ref endpoint); // listen on port 11000
            BDictionary response = parser.Parse<BDictionary>(data);
            BString nodes = ((BDictionary)response["r"])["nodes"] as BString;
            byte[] nodesBytes = nodes.Value.ToArray();
            for (int i = 0; i < nodesBytes.Length; i += 26)
            {
                byte[] nodeId = new byte[20];
                Array.Copy(nodesBytes, i, nodeId, 0, 20);
                Console.WriteLine(BMethods.BytesToId(nodeId));
                byte[] ipBytes = new byte[4];
                Array.Copy(nodesBytes, i + 20, ipBytes, 0, 4);
                IPAddress ip = new IPAddress(ipBytes);
                int port = (nodesBytes[i + 24] << 8) | nodesBytes[i + 25];
                // do something with the nodeId, ip and port
                Console.WriteLine(ip);
            }
            Console.WriteLine(BMethods.BytesToId(((BString)response["t"]).Value.ToArray()));*/
        }
    }
}
