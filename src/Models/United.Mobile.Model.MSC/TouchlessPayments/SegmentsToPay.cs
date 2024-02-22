﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
    public class SegmentsToPay
    {
        public string FlightNumber { get; set; }
        public string DepartureDate { get; set; }
        public string Origin { get; set; }
        public List<PaxToPay> PaxToPay { get; set; }
    }
}
