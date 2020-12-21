using System;
using System.Collections.Generic;
using System.Text;

namespace project_server.Models
{
    public class SquidLogDTS
    {
        public string Time { get; set; }
        public string Duration { get; set; }
        public string ClientAdress { get; set; }
        public string ResultCode { get; set; }
        public string Bytes { get; set; }
        public string RequestMethod { get; set; }
        public string URL { get; set; }
        public string User { get; set; }
        public string HierarchyCode { get; set; }
        public string Type { get; set; }
    }
}