using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using United.Service.Presentation.ReservationModel;
using United.Service.Presentation.SegmentModel;

namespace United.Common.Helper
{
    public static class CommonMethods
    {
        public static List<United.Service.Presentation.SegmentModel.ReservationFlightSegment> FlattenSegmentsForSeatDisplay(Reservation reservation)
        {
            List<United.Service.Presentation.SegmentModel.ReservationFlightSegment> flattenedSegments = new List<Service.Presentation.SegmentModel.ReservationFlightSegment>();

            try
            {
                InitializeSegmentIndices(reservation);

                foreach (United.Service.Presentation.SegmentModel.ReservationFlightSegment segment in reservation.FlightSegments)
                {

                    if (segment.Legs == null || segment.Legs.Count == 0)
                    {
                        //segment.SegmentNumber = flattenedSegments.Count;
                        flattenedSegments.Add(segment);
                    }
                    else
                    {
                        United.Service.Presentation.SegmentModel.ReservationFlightSegment parentSegment = CloneDeep(segment);
                        //parentSegment.SegmentNumber = flattenedSegments.Count;
                        flattenedSegments.Add(parentSegment);

                        bool appliedFirstSegment = false;
                        int stopIndex = 1;
                        United.Service.Presentation.SegmentModel.ReservationFlightSegment PrevSegment = parentSegment;

                        foreach (Service.Presentation.SegmentModel.PersonFlightSegment personSegment in segment.Legs)
                        {

                            if ((Convert.ToBoolean(personSegment.IsChangeOfGauge) || ((personSegment.Message != null && personSegment.Message.Any(m => m.Text == "CHANGEOFPLANE")))) && personSegment.ArrivalAirport != null && !string.IsNullOrEmpty(personSegment.ArrivalAirport.IATACityCode.CityCode))
                            {
                                if (!appliedFirstSegment)
                                {
                                    //parentSegment.FlightSegment.ArrivalAirport = personSegment.DepartureAirport;
                                    PrevSegment.FlightSegment.ArrivalAirport = CloneDeep<Service.Presentation.CommonModel.AirportModel.Airport>(personSegment.DepartureAirport);

                                    United.Service.Presentation.SegmentModel.ReservationFlightSegment stopWithCOG = CreateReservationSegmentFromPersonSegment(personSegment, parentSegment);

                                    if (stopWithCOG != null)
                                    {
                                        stopWithCOG.FlightSegment.SegmentNumber = parentSegment.FlightSegment.SegmentNumber;
                                        stopWithCOG.SegmentNumber = parentSegment.FlightSegment.SegmentNumber;
                                        if (stopWithCOG.Characteristic.IsNullOrEmpty()) stopWithCOG.Characteristic = new Collection<Service.Presentation.CommonModel.Characteristic>();
                                        stopWithCOG.Characteristic.Add(new Service.Presentation.CommonModel.Characteristic() { Code = "StopIndex", Value = stopIndex.ToString() });
                                        stopIndex++;
                                        flattenedSegments.Add(stopWithCOG);

                                        PrevSegment = stopWithCOG;
                                    }

                                    appliedFirstSegment = true;
                                }
                                else
                                {
                                    if (flattenedSegments.Count > 1)
                                    {
                                        United.Service.Presentation.SegmentModel.ReservationFlightSegment lastFlattenedSegment = flattenedSegments[flattenedSegments.Count - 1];

                                        PrevSegment.FlightSegment.ArrivalAirport = CloneDeep<Service.Presentation.CommonModel.AirportModel.Airport>(personSegment.DepartureAirport);

                                        United.Service.Presentation.SegmentModel.ReservationFlightSegment stopWithCOG = CreateReservationSegmentFromPersonSegment(personSegment, parentSegment);
                                        if (stopWithCOG != null)
                                        {
                                            stopWithCOG.SegmentNumber = parentSegment.SegmentNumber;
                                            stopWithCOG.FlightSegment.SegmentNumber = parentSegment.FlightSegment.SegmentNumber;
                                            if (stopWithCOG.Characteristic.IsNullOrEmpty()) stopWithCOG.Characteristic = new Collection<Service.Presentation.CommonModel.Characteristic>();
                                            stopWithCOG.Characteristic.Add(new Service.Presentation.CommonModel.Characteristic() { Code = "StopIndex", Value = stopIndex.ToString() });
                                            stopIndex++;
                                            flattenedSegments.Add(stopWithCOG);

                                            PrevSegment = stopWithCOG;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PrevSegment.FlightSegment.ArrivalAirport = CloneDeep<Service.Presentation.CommonModel.AirportModel.Airport>(personSegment.ArrivalAirport);
                            }
                        }
                    }
                }
                return flattenedSegments;
            }
            catch (Exception)
            {
                return flattenedSegments;
            }
        }


        private static void InitializeSegmentIndices(Reservation reservation)
        {
            try
            {
                if (reservation == null) return;
                if (reservation.FlightSegments == null || reservation.FlightSegments.Count == 0) return;
                int segmentIndex = 1;   //PLEASE DO NOT CHANGE THIS INDEX BASE TO OTHER VALUE NOT 1
                foreach (ReservationFlightSegment segment in reservation.FlightSegments)
                {
                    segment.SegmentNumber = segmentIndex;
                    segment.FlightSegment.SegmentNumber = segmentIndex;
                    segmentIndex++;
                }
            }
            catch (Exception)
            {

            }
        }
        public static T CloneDeep<T>(this T src)
        {
            try
            {
                if (src == null) return default(T);

                T dest = CreateDeepClone<T>(src);
                return dest;

            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public static T CreateDeepClone<T>(T source)
        {
            //if (!typeof(T).IsSerializable)
            //{
            //    throw new ArgumentException("The type must be serializable.", "source");
            //}

            // Don't serialize a null object, simply return the default for that object 
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            var obj = JsonConvert.SerializeObject(source);
            return (T)JsonConvert.DeserializeObject<T>(obj);

        }
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        //public static T CreateDeepClone<T>(T instanceToCopy)
        //{
        //    T clone;
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //        formatter.Context = new StreamingContext(StreamingContextStates.Clone);
        //        formatter.Serialize(stream, instanceToCopy);
        //        stream.Position = 0;        // Seek back to the start of the memory stream so we can deserialize
        //        clone = (T)formatter.Deserialize(stream);
        //    }
        //    return clone;
        //}
        private static Service.Presentation.SegmentModel.ReservationFlightSegment CreateReservationSegmentFromPersonSegment(Service.Presentation.SegmentModel.PersonFlightSegment personSegment, Service.Presentation.SegmentModel.ReservationFlightSegment parentSegment)
        {
            try
            {
                Service.Presentation.SegmentModel.ReservationFlightSegment resSegment = CloneDeep<United.Service.Presentation.SegmentModel.ReservationFlightSegment>(parentSegment);

                resSegment.FlightSegment = personSegment; //persSegment; // 
                resSegment.FlightSegment.ArrivalDateTime = parentSegment.FlightSegment.ArrivalDateTime;

                resSegment.FlightSegment.BookingClasses = parentSegment.FlightSegment.BookingClasses;

                resSegment.TripNumber = parentSegment.TripNumber;

                resSegment.FlightSegment.MarketedFlightSegment = parentSegment.FlightSegment.MarketedFlightSegment;

                resSegment.FlightSegment.OperatingAirline = parentSegment.FlightSegment.OperatingAirline;
                resSegment.FlightSegment.Message = parentSegment.FlightSegment.Message;

                return resSegment;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static bool IsNullOrEmpty<T>(this IEnumerable<T> items)
        {
            if (null == items || !items.Any())
            {
                return true;
            }
            return false;
        }

    }
}
