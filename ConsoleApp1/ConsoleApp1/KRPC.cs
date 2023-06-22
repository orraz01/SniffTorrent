using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Parsing;
using BencodeNET.Objects;


namespace ConsoleApp1
{
    class KRPC
    {
        public static byte[] find_node_query(byte[] id, byte[] target, byte[] t)
        {
            // Create a new bencoded dictionary for the query
            var query = new BDictionary();
            query["t"] = new BString(t); // transaction ID
            query["y"] = new BString("q"); // query type
            query["q"] = new BString("find_node"); // query name

            // Add additional parameters to the query
            var argsDict = new BDictionary();
            argsDict["id"] = new BString(id); // target node ID
            argsDict["target"] = new BString(target); // target ID
            query["a"] = argsDict;

            // Encode the query as a byte array
            return query.EncodeAsBytes();
        }
        public static byte[] get_peers_query(byte[] id, byte[] infohash, byte[] t)
        {
            // Create a new bencoded dictionary for the query
            var query = new BDictionary();
            query["t"] = new BString(t); // transaction ID
            query["y"] = new BString("q"); // query type
            query["q"] = new BString("get_peers"); // query name

            // Add additional parameters to the query
            var argsDict = new BDictionary();
            argsDict["id"] = new BString(id); // target node ID
            argsDict["target"] = new BString(infohash); // target ID
            query["a"] = argsDict;

            // Encode the query as a byte array
            return query.EncodeAsBytes();
        }
        public static byte[] PingReqest(byte[] id,byte[] tkey)
        {
            // Create a new bencoded dictionary for the query
            var query = new BDictionary();
            query["t"] = new BString(tkey); // transaction ID
            query["y"] = new BString("q"); // query type
            query["q"] = new BString("ping"); // query name

            // Add additional parameters to the query
            var argsDict = new BDictionary();
            argsDict["id"] = new BString(id); // target node ID
            query["a"] = argsDict;

            // Encode the query as a byte array
            return query.EncodeAsBytes();
        }
        public static byte[] PingResponse(byte[] id, byte[] tkey)
        {
            // Create a new bencoded dictionary for the query
            var query = new BDictionary();
            query["t"] = new BString(tkey); // transaction ID
            query["y"] = new BString("r"); // query type
            // Add additional parameters to the query
            var argsDict = new BDictionary();
            argsDict["id"] = new BString(id); // target node ID
            query["r"] = argsDict;

            // Encode the query as a byte array
            return query.EncodeAsBytes();
        }
        public static byte[] GetPeersResponse(byte[] id, byte[] nodes, byte[] t,byte[] token,BList peers)
        {
            // Create a new bencoded dictionary for the query
            var query = new BDictionary();
            query["t"] = new BString(t); // transaction ID
            query["y"] = new BString("r"); // query type

            // Add additional parameters to the query
            var argsDict = new BDictionary();
            argsDict["id"] = new BString(id); // target node ID
            argsDict["nodes"] = new BString(nodes); // target ID
            argsDict["token"] = new BString(token); 
            if(peers!=null)
            {
                argsDict["values"] = peers;
            }
            query["r"] = argsDict;

            // Encode the query as a byte array
            return query.EncodeAsBytes();
        }
        public static byte[] FindNodeResponse(byte[] id, byte[] nodes, byte[] t)
        {
            // Create a new bencoded dictionary for the query
            var query = new BDictionary();
            query["t"] = new BString(t); // transaction ID
            query["y"] = new BString("r"); // query type

            // Add additional parameters to the query
            var argsDict = new BDictionary();
            argsDict["id"] = new BString(id); // target node ID
            argsDict["nodes"] = new BString(nodes); // target ID
            query["r"] = argsDict;

            // Encode the query as a byte array
            return query.EncodeAsBytes();
        }
        public static byte[] BadTokenError(byte[] t)
        {
            // Create a new bencoded dictionary for the query
            var query = new BDictionary();
            query["t"] = new BString(t); // transaction ID
            query["y"] = new BString("e"); // query type

            // Add additional parameters to the query
            BList argsList = new BList();
            
            argsList.Add((IBObject)new BNumber(203));
            argsList.Add(new BString("Bad Token"));
            query["e"] = argsList;

            // Encode the query as a byte array
            return query.EncodeAsBytes();
        }
    }
}
