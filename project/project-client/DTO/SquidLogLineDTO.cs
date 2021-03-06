﻿using System;

namespace project_client.DTO
{   
    [Serializable]
    public class SquidLogLineDTO 
    { 
        public string Time { get; set; }
        public string Duration { get; set; }
        public string ClientAdress { get; set; }
        public string ResultCode { get; set; }
        public string Bytes { get; set; }
        public string RequestMethod { get; set; }
        public string URL { get; set; }
        public string UserRequest { get; set; }
        public string HierarchyCode { get; set; }
        public string Type { get; set; }
        public TimeSpan ObjectGeneratingTime { get; set; }
    }
}
