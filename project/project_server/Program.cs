using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using AnySerializer.Extensions;
using Dapper;
using project_server.Models;

namespace project_server
{
    class Program
    {
        static void Main(string[] args)
        {
            Int32 port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            StartListener(CreateServer(port, "127.0.0.1"));
        }

        private static void StartListener(TcpListener server)
        {
            server.Start();

            try
            {
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    TcpClient client = server.AcceptTcpClient();

                    Console.WriteLine("Connected!");

                    NetworkStream stream = client.GetStream();

                    ReceberDadosLog(stream);

                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private static byte[] PrepareMessaResponse(string message) 
        {
            return System.Text.Encoding.ASCII.GetBytes(message);
        }

        private static TcpListener CreateServer(Int32 port, string localAdress)
        {
            return new TcpListener(IPAddress.Parse(localAdress), port);
        }

        private static void ReceberDadosLog(NetworkStream stream) 
        {
            var connection = AbrirConexao();

            byte[] dataReceived = new byte[1024];
            List<SquidLogDTS> data = new List<SquidLogDTS>();

            while ((stream.Read(dataReceived, 0, dataReceived.Length)) != 0)
            {
                SaveLog(dataReceived.Deserialize<SquidLogDTS>(), connection);
                byte[] response = PrepareMessaResponse("Deu Certo");
                stream.Write(response, 0, response.Length);
            }

            FecharConexao(connection);
        }


        private static void SaveLog(SquidLogDTS obj, SqlConnection connection)
        {
            
            string sql = @"INSERT INTO DEFAULTLOG(TIME, DURATION, CLIENT_ADRESS, RESULT_CODE, BYTES, REQUEST_METHOD, URL, USER_REQUEST,HIERARCHY_CODE,TYPE) 
                            VALUES(@Time, @Duration, @ClientAdress, @ResultCode, @Bytes, @RequestMethod, @Url, @UserRequest, @HierarchyCode, @Type);";
            
            connection.Execute(sql, new
            {
                Time = obj.Time,
                Duration = obj.Duration,
                ClientAdress = obj.ClientAdress,
                ResultCode = obj.ResultCode,
                Bytes = obj.Bytes,
                RequestMethod = obj.RequestMethod,
                Url = obj.URL,
                UserRequest = obj.User,
                HierarchyCode = obj.HierarchyCode,
                Type = obj.Type
            });
            
        }

        private static SqlConnection AbrirConexao()
        {
            string stringConnection = "User ID=sa;password=Senha_150189;Initial Catalog=SQUID;Data Source=tcp:.,1433";
            return new SqlConnection(stringConnection);
        }

        private static void FecharConexao(SqlConnection connection)
        {
            connection.Close();
        }
    }
}

