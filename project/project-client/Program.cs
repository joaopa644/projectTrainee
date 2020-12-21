using project_client.DTS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using AnySerializer.Extensions;
using System.Threading.Tasks;

namespace project_client
{
    class Program
    {
        static void Main(string[] args)
        {
            string arquivo = @"../../../file/access.log";
            string server = "127.0.0.1";

            if (File.Exists(arquivo))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(arquivo))
                    {
                        var result = GenerateTransferDataList(sr);

                        Connect(server, result);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("O arquivo " + arquivo + " não foi localizado");
            }
            Console.ReadKey();
        }


        private static SquidLogDTS[] GenerateTransferDataList(StreamReader streamReader)
        {

            if (streamReader == null) throw new ArgumentNullException(nameof(streamReader));

            String linha;
            List<SquidLogDTS> dataList = new List<SquidLogDTS>();
            SquidLogDTS newItem;
            string[] arrayItem;
            while ((linha = streamReader.ReadLine()) != null)
            {
                arrayItem = linha.Split(" ");

                newItem = new SquidLogDTS()
                {
                    Time = arrayItem[0],
                    Duration = arrayItem[1],
                    ClientAdress = arrayItem[2],
                    ResultCode = arrayItem[3],
                    Bytes = arrayItem[4],
                    RequestMethod = arrayItem[5],
                    URL = arrayItem[6],
                    User = arrayItem[7],
                    HierarchyCode = arrayItem[8],
                    Type = arrayItem[9]
                };

                dataList.Add(newItem);
            }

            return dataList.ToArray();
        }

        private static void Connect(String server, SquidLogDTS[] messages)
        {

            Int32 port = 13000;
            TcpClient client = new TcpClient(server, port);
            byte[] data = new byte[1024];
            
            try
            {
                NetworkStream stream = client.GetStream();

                foreach (var message in messages)
                {
                    Array.Clear(data, 0, data.Length);
                    data = SerializeMessage<SquidLogDTS>(message);
                    stream.Write(data, 0, data.Length);

                    Console.WriteLine("Sent: {0}", message);

                    String responseData = String.Empty;

                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    Console.WriteLine("Received: {0}", responseData);
                }

                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }  

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        private static byte[] SerializeMessage<T>(Object obj)
        {
           return obj.Serialize();
        }
    }
}
