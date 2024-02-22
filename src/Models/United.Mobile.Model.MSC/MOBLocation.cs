using System;

namespace United.Definition
{
    [Serializable()]
    public class MOBLocation
    {
        private Double longitude;
        private Double latitude;

        public MOBLocation()
        {
        }

        public MOBLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public Double Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }

        public Double Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }
    }
}
