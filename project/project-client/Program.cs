using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using project_client.DTO;

namespace project_client
{
    class Program
    {
        static void Main(string[] args)
        {
            string arquivo = @"../../../file/access.log";
            string server = "127.0.0.1";
            Int32 port = 13000;

            if (File.Exists(arquivo))
            {
                try
                {
                    SendDataLog(arquivo, server, port);
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


        private static void SendDataLog(string filePath, string serverAdress, Int32 port)
        {
            StreamReader sr = new StreamReader(filePath);
            NetworkStream stream;

            SquidLogLineDTO[] objectList = GenerateTransferDataList(sr);

            using (TcpClient client = new TcpClient(serverAdress, port))
            {
                try
                {
                    stream = client.GetStream();

                    foreach (var obj in objectList)
                    {
                        PostMessage(stream, obj);
                        GetResponse(stream);
                    }

                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
            }
        }

        private static SquidLogLineDTO StructuringData(string linha, TimeSpan time)
        {
            var arrayItem = linha.Split(" ");

            return new SquidLogLineDTO()
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
                Type = arrayItem[9],
                ObjectGeneratingTime = time
            };
        }

        private static void PostMessage(NetworkStream stream, SquidLogLineDTO newItem)
        {
            stream.Write(SerializeMessage<SquidLogLineDTO>(newItem), 0, SerializeMessage<SquidLogLineDTO>(newItem).Length);
            Console.WriteLine("Sent: {0}", newItem);
        }

        private static void GetResponse(NetworkStream stream)
        {
            byte[] data = new byte[1024];
            Int32 bytes = stream.Read(data, 0, data.Length);
            Console.WriteLine("Received: {0}", System.Text.Encoding.ASCII.GetString(data, 0, bytes));
        }
        private static byte[] SerializeMessage<T>(Object obj)
        {
            var formater = new BinaryFormatter();
            var stream = new MemoryStream();
            formater.Serialize(stream, obj);

            return stream.ToArray();
        }

        private static SquidLogLineDTO[] GenerateTransferDataList(StreamReader streamReader)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (streamReader == null) throw new ArgumentNullException(nameof(streamReader));

            String linha;
            List<SquidLogLineDTO> dataList = new List<SquidLogLineDTO>();
            while ((linha = streamReader.ReadLine()) != null)
            {
                stopwatch.Start();
                dataList.Add(StructuringData(linha, stopwatch.Elapsed));
                stopwatch.Stop();
            }

            

            streamReader.Close();

            
            return dataList.ToArray();
        }
    }
}
