using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace United.Definition
{
    public class CouchInBoundFlightSegment
    {
        public string ArrivalAirport { get; set; }
        public string ArrivalAirportName { get; set; }
        public string CarrierCode { get; set; }
        public string DepartureAirport { get; set; }
        public string DepartureAirportName { get; set; }
        public string DepartureDate { get; set; }
        public string FlightNumber { get; set; }
    }
}