using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Dapper;
using project_client.DTO;
using project_server.Setting;
using Z.Dapper.Plus;

namespace project_server
{
    class Program
    {
        static void Main(string[] args)
        {

            StartListener(CreateServer(ProjectSetting.ServerPort, ProjectSetting.ServerIp));
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

                    SaveDataLog(stream);

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

        private static TcpListener CreateServer(Int32 port, IPAddress localAdress)
        {
            return new TcpListener(localAdress, port);
        }

        private static void SaveDataLog(NetworkStream stream) 
        {
            byte[] dataReceived = new byte[1024];
            TimeSpan generateObjectsTime = new TimeSpan();
            List<SquidLogLineDTO> listObj = new List<SquidLogLineDTO>();
            var stopwatch = new Stopwatch();
            int totalRegister = 0;

            using (var connection = new SqlConnection(ProjectSetting.DataBaseConnectionString))
            {
                while ((stream.Read(dataReceived, 0, dataReceived.Length)) != 0)
                {
                    stopwatch.Start();
                    var lineLog = Deserialize(dataReceived);
                    generateObjectsTime += lineLog.ObjectGeneratingTime;
                    //ExecutesSQL(lineLog, connection);
                    listObj.Add(lineLog);
                    byte[] response = PrepareMessaResponse("Deu Certo");
                    stream.Write(response, 0, response.Length);
                    totalRegister++;
                }
                ExecuteSQLMany(listObj,connection);
                stopwatch.Stop();

                SendEmail(stopwatch.Elapsed  + generateObjectsTime, stopwatch.Elapsed, generateObjectsTime, totalRegister);
            }
        }

        private static SquidLogLineDTO Deserialize(byte[] obj)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();

            stream.Write(obj, 0, obj.Length);
            stream.Position = 0;

            return formatter.Deserialize(stream) as SquidLogLineDTO;
        }

        private static void ExecutesSQL(SquidLogLineDTO obj, SqlConnection connection)
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
                UserRequest = obj.UserRequest,
                HierarchyCode = obj.HierarchyCode,
                Type = obj.Type
            });
            
        }

        private static void ExecuteSQLMany(List<SquidLogLineDTO> objs, SqlConnection connection)
        {
            DapperPlusManager.Entity<SquidLogLineDTO>().Table("DEFAULTLOG");
            connection.BulkInsert(objs);
        }


        private static void SendEmail(TimeSpan totalTime, TimeSpan transferTime, TimeSpan generateObjectsTime, int totalRegister)
        {
            MailMessage message = new MailMessage(ProjectSetting.EmailFrom, ProjectSetting.EmailTo);
            message.Subject = "Informações do processo de envio e recebimento dos dados.";
            message.Body = @$"Tempo leitura no client: {generateObjectsTime}. 
                            Tempo parse do arquivo para objetos: {generateObjectsTime}. 
                            Tempo de transferencia: {transferTime}. 
                            Total do processo: {totalTime}.
                            Total de registros: {totalRegister}";
            SmtpClient client = new SmtpClient()
            {
                Host = "smtp-mail.outlook.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(ProjectSetting.EmailFrom, ProjectSetting.EmailFromPassword)
            };

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                    ex.ToString());
            }
        }
    }
}

