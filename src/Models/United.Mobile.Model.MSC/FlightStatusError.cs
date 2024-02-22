﻿using System;
using United.Definition;

namespace United.Mobile.Model.MSC
{
    public class FlightStatusError
    {
        public Error[] Errors { get; set; }
        public DateTime GenerationTime { get; set; }
        public string InnerException { get; set; }
        public string Message { get; set; }
        public string ServerName { get; set; }
        public int Severity { get; set; }
        public string StackTrace { get; set; }
        public string Version { get; set; }

    }

}