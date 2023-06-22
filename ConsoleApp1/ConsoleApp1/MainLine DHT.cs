using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using BencodeNET.Parsing;
using BencodeNET.Objects;
using System.Security.Cryptography;

namespace ConsoleApp1
{
    class MainLine_DHT
    {
        private string id;
        private byte[] IdBytes;
        private string stringBytes;
        private BTree<Bucket> Routing_Table;
        private const char left = '1';
        private const char right = '0';
        private int port;
        private static RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();
        private Dictionary<IPEndPoint, Token> tokenHandler;
        private Dictionary<byte[], PeerList> peers;
        private UdpClient connection;
        private SQLHandler sqlHandle;
        public MainLine_DHT(int port)
        {
            this.peers = new Dictionary<byte[], PeerList>(new ByteArrayEqualityComparer());
            this.tokenHandler = new Dictionary<IPEndPoint, Token>();
            this.IdBytes = new byte[20];
            rnd.GetBytes(IdBytes);//makes random byte array composed of 20 bytes
            this.id = BitConverter.ToString(IdBytes).Replace("-", string.Empty); //changes bytes to hex string simailair to sha 1
            this.id = this.id.ToLower();
            //changes from upper to lowercase
            this.stringBytes = BMethods.BytesToString(IdBytes);
            //bytes array repreasented as string
            this.Routing_Table = new BTree<Bucket>(new Bucket());
            Console.WriteLine("myid:" + stringBytes);
            this.port = port;
            this.connection = new UdpClient(this.port);
            this.connection.Client.SendTimeout = 5000;
            this.connection.Client.ReceiveTimeout = 5000;
            this.sqlHandle = new SQLHandler();
        }
        public void start()
        {
            //starts the dht process
            bootstrap();
            //Start bootstrapping process
            MainLoop();
        }
        public void HandleRequests(BDictionary request,IPEndPoint sender)
        {
            if (request[new BString("y")].ToString()=="q") // if packet is a request
            {
                BString tkey = request[new BString("t")] as BString;
                if (request[new BString("q")].ToString() == "find_node")// if request is find_node
                {
                    BString bTarget = ((BDictionary)request["a"])["target"] as BString;
                    string target = BMethods.BytesToId(bTarget.Value.ToArray());
                    Bucket returnbucket = this.FindClosestBucket(target);
                    byte[] response = KRPC.FindNodeResponse(this.IdBytes, returnbucket.NodesList(), tkey.Value.ToArray());
                    connection.SendAsync(response, response.Length, sender);
                }
                else if(request[new BString("q")].ToString() == "ping")
                {
                    byte[] response = KRPC.PingResponse(this.IdBytes, tkey.Value.ToArray());
                    connection.SendAsync(response, response.Length, sender);
                }
                else if(request[new BString("q")].ToString() == "get_peers")
                {
                    byte[] tokenValue = new byte[8];
                    rnd.GetBytes(tokenValue);
                    Token token = new Token(tokenValue);
                    if(!tokenHandler.ContainsKey(sender))
                        tokenHandler.Add(sender, token);
                    BString bId = ((BDictionary)request["a"])["id"] as BString;
                    string nodeId = BMethods.BytesToId(bId.Value.ToArray());
                    BString bInfoHash = ((BDictionary)request["a"])["info_hash"] as BString;
                    byte[] infohash = bInfoHash.Value.ToArray();
                    Bucket returnbucket = this.FindClosestBucket(BMethods.BytesToId(infohash));
                    byte[] response;
                    if (peers.ContainsKey(infohash))
                    {
                        response = KRPC.GetPeersResponse(this.IdBytes, returnbucket.NodesList(), tkey.Value.ToArray(), token.GetValue(), peers[infohash].GetList());
                    }
                    else
                        response = KRPC.GetPeersResponse(this.IdBytes, returnbucket.NodesList(), tkey.Value.ToArray(), token.GetValue(), null);
                    connection.SendAsync(response, response.Length, sender);
                }
                else if(request[new BString("q")].ToString() == "announce_peer")
                {
                    if(tokenHandler.ContainsKey(sender))
                    {
                        BString bToken = ((BDictionary)request["a"])["token"] as BString;
                        byte[] tokenVal = bToken.Value.ToArray();
                        if(tokenHandler.ContainsKey(sender))
                        {
                            if (tokenHandler[sender].GetValue().SequenceEqual(tokenVal)) //issue to fix
                            {
                                byte[] response = KRPC.PingResponse(this.IdBytes, tkey.Value.ToArray());
                                connection.SendAsync(response, response.Length, sender);
                                BString bInfoHash = ((BDictionary)request["a"])["info_hash"] as BString;
                                byte[] infohash = bInfoHash.Value.ToArray();
                                // Convert portBytes to network byte order
                                BNumber bPortBytes = ((BDictionary)request["a"])["port"] as BNumber;
                                int portNetworkOrder = (int)bPortBytes;
                                BDictionary aDict = ((BDictionary)request["a"]);
                                if (aDict.ContainsKey(new BString("implied_port")) && aDict[new BString("implied_port")] is BNumber && ((BNumber)aDict[new BString("implied_port")]).Value == 0)
                                {
                                    sender = new IPEndPoint(sender.Address, portNetworkOrder);
                                }
                                if (!this.peers.ContainsKey(infohash))
                                {
                                    this.peers[infohash] = new PeerList();
                                }
                                this.peers[infohash].AddPeer(sender);

                                BString bId = ((BDictionary)request["a"])["id"] as BString;
                                byte[] id = bInfoHash.Value.ToArray();
                                if (aDict.ContainsKey(new BString("name")))
                                {
                                    BString bName = ((BDictionary)request["a"])["name"] as BString;
                                    this.sqlHandle.AddFile(BMethods.BytesToId(infohash), bName.ToString());
                                }
                                this.sqlHandle.AddUser(BMethods.BytesToId(id), sender.Address.ToString(), BMethods.BytesToId(infohash));
                            }
                        }
                        else
                        {
                            byte[] response = KRPC.BadTokenError(tkey.Value.ToArray());
                            connection.SendAsync(response, response.Length, sender);
                        }
                    }
                    else
                    {
                        byte[] response = KRPC.BadTokenError(tkey.Value.ToArray());
                        connection.SendAsync(response, response.Length, sender);
                    }
                }
            }
        }
        private void MainLoop()
        {
            var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
            Dictionary<byte[], PingHandler> pingHandler = new Dictionary<byte[], PingHandler>(new ByteArrayEqualityComparer());
            this.connection.Close();
            this.connection = new UdpClient(this.port);
            this.connection.Client.ReceiveTimeout = -1;
            while (true)
            {
                try
                {
                    if (connection.Available > 0)//checks if theres data to receive
                    {
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, this.port);
                        byte[] data = connection.Receive(ref endPoint); //sniffs
                        BDictionary request = parser.Parse<BDictionary>(data);
                        //handle request and send the sender
                        HandleRequests(request, endPoint);
                        if (request.ContainsKey(new BString("r")))
                        {
                            BString bTarget = ((BDictionary)request["r"])["id"] as BString;
                            byte[] target = bTarget.Value.ToArray();
                            if (pingHandler.ContainsKey(target))
                            {
                                pingHandler[target].Data = data;
                            }
                        }
                        else if (request.ContainsKey(new BString("y")) && request["y"].ToString() == "q")
                        {
                            BString bTarget = ((BDictionary)request["a"])["id"] as BString;
                            byte[] target = bTarget.Value.ToArray();
                            Bucket place = this.AddNode(BMethods.BytesToId(target), endPoint.Address, endPoint.Port);
                            if (place != null)
                            {
                                DHTNode leastSeen = place.LeastSeenNode();
                                DHTNode nodeToAdd = new DHTNode(BMethods.BytesToId(target), this.IdBytes, endPoint.Address, endPoint.Port);
                                if (!leastSeen.IsUpToDate())
                                    if (!pingHandler.ContainsKey(leastSeen.Id))
                                    {
                                        pingHandler.Add(leastSeen.Id, new PingHandler(connection, leastSeen, nodeToAdd, place, this.IdBytes));
                                    }
                            }
                        }
                    }
                    for (int i = 0; i < pingHandler.Keys.Count; i++)
                    {
                        byte[] key = pingHandler.Keys.ElementAt(i);
                        if (pingHandler[key].Ping())
                        {
                            pingHandler.Remove(key);
                            i--;
                        }
                    }
                    for (int i = 0; i < this.tokenHandler.Keys.Count; i++)
                    {
                        IPEndPoint key = this.tokenHandler.Keys.ElementAt(i);
                        if (!this.tokenHandler[key].UpToDate())
                        {
                            this.tokenHandler.Remove(key);
                            i--;
                        }
                    }
                    foreach(PeerList peerList in peers.Values)
                    {
                        peerList.Update();
                    }
                }
                catch
                {
                    continue;
                }
            }


        }
        public void bootstrap()
        {
            /* this method contacts known bootstrapping nodes with a find_node query with my id as the target.
             * this method will continueslly send continuously send find_node query to closer and closer node with my own id as the target until no closer nodes can be found*/
            IPAddress[] ipAddresses = Dns.GetHostAddresses("router.bittorrent.com"); //gets ip of known bootstrapping node
            var endpoint = new IPEndPoint(ipAddresses[0], 6881); //make end point object with ip from earlier and known port

            var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
            // Encode the query as a byte array
            byte[] tkey = new byte[2];
            rnd.GetBytes(tkey);
            //generates t key
            var queryBytes = KRPC.find_node_query(this.IdBytes, this.IdBytes, tkey);
            // Creates bencoded request dictionary as byte array

            this.FindNodeQuery(new IPEndPoint[] { endpoint }, this.IdBytes, tkey);

            while(true)
            {
                DHTNode[] ThreeClosestNodes = this.FindClosestBucket(this.id).Return_a_Closest(3);
                //Gets 3 closest nodes in routing table to my id
                IPEndPoint[] arr = new IPEndPoint[ThreeClosestNodes.Length];
                for (int i = 0; i < ThreeClosestNodes.Length; i++)
                {
                    arr[i] = new IPEndPoint(ThreeClosestNodes[i].Address, ThreeClosestNodes[i].Port);
                }
                this.FindNodeQuery(arr, this.IdBytes, tkey);
                DHTNode[] test = this.FindClosestBucket(this.id).Return_a_Closest(3);
                int count = 0;
                for(int l=0;l<test.Length;l++)
                {
                    if (DHTNode.Equals(test[l], ThreeClosestNodes[l]))
                    {
                        count++;
                    }
                }
                if (count == 3)
                {
                    ThreeClosestNodes = this.FindClosestBucket(this.id).Return_a_Closest(3);
                    DHTNode[] EightClosestNodes = this.FindClosestBucket(this.id).Return_a_Closest(8);
                    //Gets 3 closest nodes in routing table to my id
                    arr = new IPEndPoint[EightClosestNodes.Length];
                    for (int i = 0; i < EightClosestNodes.Length; i++)
                    {
                        arr[i] = new IPEndPoint(EightClosestNodes[i].Address, EightClosestNodes[i].Port);
                    }
                    this.FindNodeQuery(arr, this.IdBytes, tkey);
                    test = this.FindClosestBucket(this.id).Return_a_Closest(3);
                    count = 0;
                    for (int l = 0; l < test.Length; l++)
                    {
                        if (DHTNode.Equals(test[l], ThreeClosestNodes[l]))
                        {
                            count++;
                        }
                    }
                    if (count == 3)
                        break;
                }
            }
        }

        private void FindNodeQuery(IPEndPoint[] arr, byte[] target, byte[] tkey)
        {
            var parser = new BencodeParser(Encoding.GetEncoding("ISO-8859-1"));
            // Encode the query as a byte array
            var queryBytes = KRPC.find_node_query(target, target, tkey);
            // Creates bencoded request dictionary as byte array

            for (int i = 0; i < arr.Length; i++)
            {
                connection.SendAsync(queryBytes, queryBytes.Length, arr[i]);
            }
            IPEndPoint point = new IPEndPoint(IPAddress.Any, this.port);
            for (int i = 0; i < arr.Length; i++)
            {
                try
                {
                    byte[] data = connection.Receive(ref point);
                    //recieves data
                    BDictionary response = parser.Parse<BDictionary>(data);
                    BString nodes = ((BDictionary)response["r"])["nodes"] as BString;
                    byte[] nodesBytes = nodes.Value.ToArray();
                    for (int j = 0; j < nodesBytes.Length; j += 26)
                    {
                        byte[] nodeId = new byte[20];
                        Array.Copy(nodesBytes, j, nodeId, 0, 20);
                        //first 20 bytes is id
                        byte[] ipBytes = new byte[4];
                        Array.Copy(nodesBytes, j + 20, ipBytes, 0, 4);
                        IPAddress ip = new IPAddress(ipBytes);
                        //21-24 byte is ip
                        int port = (nodesBytes[j + 24] << 8) | nodesBytes[j + 25];
                        //23-26 byte is port
                        this.AddNode(BMethods.BytesToId(nodeId), ip, port);
                        // add node
                    }
                }
                catch
                {
                    continue;
                    //if timeout conditions are met continue
                }
            }

        }
        public Bucket FindClosestBucket(string hex, byte[] NodeId, byte[] Distance, string DistanceStr, int index, BTree<Bucket> pos)
        {
            // recrusively searched the routing table for the closest bucket and sends the 8 closest nodes by value
            if (DistanceStr[index] == '1')
            {
                DHTNode[] arr = pos.GetValue().Return_a_Closest(pos.GetValue().Index);
                Bucket Returnbucket = new Bucket();
                for (int i = 0; i < arr.Length; i++)
                {
                    Returnbucket.AddNode(arr[i].NodeId, this.IdBytes, arr[i].Address, arr[i].Port);
                }
                return Returnbucket;
            }
            if (this.stringBytes[index] == left)
            {
                if (!pos.HasLeft())
                {
                    DHTNode[] arr = pos.GetValue().Return_a_Closest(pos.GetValue().Index);
                    Bucket Returnbucket = new Bucket();
                    for (int i = 0; i < arr.Length; i++)
                    {
                        Returnbucket.AddNode(arr[i].NodeId, this.IdBytes, arr[i].Address, arr[i].Port);
                    }
                    return Returnbucket;
                    //returns pos if thats the last bucket
                }
                Bucket bucket = this.FindClosestBucket(hex, NodeId, Distance, DistanceStr, index + 1, pos.GetLeft());
                if (bucket.Index == 8)
                    return bucket;
                DHTNode[] arr1 = pos.GetValue().Return_a_Closest(pos.GetValue().Index);
                for (int i = 0; i < arr1.Length; i++)
                {
                    bucket.AddNode(arr1[i].NodeId, this.IdBytes, arr1[i].Address, arr1[i].Port);
                }
                return bucket;
            }
            else if (this.stringBytes[index] == right)
            {
                if (!pos.HasRight())
                {
                    DHTNode[] arr = pos.GetValue().Return_a_Closest(pos.GetValue().Index);
                    Bucket Returnbucket = new Bucket();
                    for (int i = 0; i < arr.Length; i++)
                    {
                        Returnbucket.AddNode(arr[i].NodeId, this.IdBytes, arr[i].Address, arr[i].Port);
                    }
                    return Returnbucket;
                    //returns pos if thats the last bucket
                }
                Bucket bucket = this.FindClosestBucket(hex, NodeId, Distance, DistanceStr, index + 1, pos.GetRight());
                if (bucket.Index == 8)
                    return bucket;
                DHTNode[] arr1 = pos.GetValue().Return_a_Closest(pos.GetValue().Index);
                for (int i = 0; i < arr1.Length; i++)
                {
                    bucket.AddNode(arr1[i].NodeId, this.IdBytes, arr1[i].Address, arr1[i].Port);
                }
                return bucket;
            }
            DHTNode[] arr2 = pos.GetValue().Return_a_Closest(pos.GetValue().Index);
            Bucket Returnbucket2 = new Bucket();
            for (int i = 0; i < arr2.Length; i++)
            {
                Returnbucket2.AddNode(arr2[i].NodeId, this.IdBytes, arr2[i].Address, arr2[i].Port);
            }
            return Returnbucket2;
        }
        public Bucket FindClosestBucket(string hex)
        {
            byte[] NodeId = BMethods.ConvertHexToByte(hex);
            //changes hex id to byte array
            byte[] Distance = BMethods.Xor(NodeId, this.IdBytes);
            //xor of my id and the new node's id for distance checking purposes
            string DistanceStr = BMethods.BytesToString(Distance);
            //takes the distance in byte array and changes to string
            if (DistanceStr[0] == '1')
                return this.Routing_Table.GetValue();
            return FindClosestBucket(hex, NodeId, Distance, DistanceStr, 0, this.Routing_Table);

        }
        public Bucket AddNode(string hex, IPAddress address, int port) // adds a node to the appropiate bucket in the routing table
        {
            //works the same as the FindClosestBucket method but instead adds a node in the correct place
            BTree<Bucket> pos = this.Routing_Table;
            byte[] NodeId = BMethods.ConvertHexToByte(hex);
            byte[] Distance = BMethods.Xor(NodeId, this.IdBytes);
            string DistanceStr = BMethods.BytesToString(Distance);
            int index = 0;
            while (DistanceStr[index] != '1')
            {
                if (this.stringBytes[index] == left)
                {
                    if (!pos.HasLeft())
                    {
                        if (!pos.GetValue().AddNode(hex, IdBytes, address, port))//if its full and cant add
                        {
                            pos.SetLeft(new BTree<Bucket>(pos.GetValue().split(index)));
                            //splits the bucket according to id with the next bucket being 1 byte closer to my own
                            pos.GetLeft().GetValue().AddNode(hex, this.IdBytes, address, port);
                            //adds node to the next bucket which is in the left leaf ("1")
                        }
                        return null;
                    }
                    pos = pos.GetLeft();
                }
                else if (stringBytes[index] == right)
                {
                    if (!pos.HasRight())
                    {
                        if (!pos.GetValue().AddNode(hex, IdBytes, address, port))//if its full and cant add
                        {
                            pos.SetRight(new BTree<Bucket>(pos.GetValue().split(index)));
                            //splits the bucket according to id with the next bucket being 1 byte closer to my own
                            pos.GetRight().GetValue().AddNode(hex, this.IdBytes, address, port);
                            //adds node to the next bucket which is in the right leaf ("0")
                        }
                        return null;
                    }
                    pos = pos.GetRight();
                }
                index++;
            }
            if (!pos.GetValue().AddNode(hex, this.IdBytes, address, port))
            {
                return pos.GetValue();
            }
            return null;
            //adds node to current bucket. no need for splitting because theres already a new bucket in the next leaf

        }
        public void PrintTree(BTree<Bucket> tree, int i, Node<Queue<Bucket>> layers) //simple printing method for binary tree
        {
            if (tree != null)
            {
                int j = i;
                Node<Queue<Bucket>> layers_start = layers;
                for (; j > 0 && layers.GetNext() != null; j--)
                {
                    layers = layers.GetNext();
                }
                if (layers.GetNext() == null && j != 0)
                {
                    layers.SetNext(new Node<Queue<Bucket>>(new Queue<Bucket>()));
                    layers = layers.GetNext();
                }
                layers.GetValue().Insert(tree.GetValue());
                PrintTree(tree.GetRight(), i + 1, layers_start);
                PrintTree(tree.GetLeft(), i + 1, layers_start);
            }
            else
                return;

        }
        public void PrintTree(BTree<Bucket> tree)
        {
            if (tree == null)
                return;
            PrintTree(tree.GetLeft());
            Console.WriteLine(tree.GetValue());
            PrintTree(tree.GetRight());
        }
        public void PrintTree()
        {
            BTree<Bucket> tree = this.Routing_Table;
            Node<Queue<Bucket>> layers = new Node<Queue<Bucket>>(new Queue<Bucket>());
            PrintTree(tree.GetLeft(), 0, layers);
            PrintTree(tree.GetRight(), 0, layers);
            int i = 1;
            Console.WriteLine(tree.GetValue());
            while (layers != null)
            {
                Console.WriteLine();
                while (!layers.GetValue().IsEmpty())
                {
                    Console.Write(layers.GetValue().Remove() + " ");
                }
                layers = layers.GetNext();
                Console.WriteLine("------------");
            }


        }
    }
}
