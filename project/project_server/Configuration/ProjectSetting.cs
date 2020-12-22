using System;
using System.Net;

namespace project_server.Setting
{
    public static class ProjectSetting
    {
        public static IPAddress ServerIp => IPAddress.Parse("127.0.0.1");
        public static Int32 ServerPort => 13000;
        public static string DataBaseConnectionString => "User ID=sa;password=Senha_150189;Initial Catalog=SQUID;Data Source=tcp:.,1433";
        public static string EmailFrom => "";
        public static string EmailFromPassword  => "";
        public static string EmailTo => "";

    }
}
