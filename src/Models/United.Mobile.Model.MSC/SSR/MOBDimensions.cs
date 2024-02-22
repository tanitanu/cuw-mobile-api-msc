using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace United.Definition.SSR
{
    [Serializable()]
    public class MOBDimensions
    {
        private string dimensions;
        private double width;
        private double height;
        private string units;
        private string flightEquipmentDescription;
        private string wcFitConfirmationMsg;
        public string Dimensions
        {
            get { return dimensions; }
            set { dimensions = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }
        public double Width
        {
            get { return width; }
            set { width = value; }
        }
        public double Height
        {
            get { return height; }
            set { height = value; }
        }
        public string Units
        {
            get { return units; }
            set { units = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }
        public string FlightEquipmentDescription
        {
            get { return flightEquipmentDescription; }
            set { flightEquipmentDescription = value; }
        }
        public string WcFitConfirmationMsg
        {
            get { return wcFitConfirmationMsg; }
            set { wcFitConfirmationMsg = value; }
        }

    }
}
