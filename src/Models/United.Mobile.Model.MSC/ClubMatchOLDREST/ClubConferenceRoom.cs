﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition.ClubMatchOLDREST
{
    [Serializable()]
    public class ClubConferenceRoom
    {
        private string description = string.Empty;
        private string reservationPhoneNumber = string.Empty;
        private List<string> businessAmenities = new List<string>();

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ReservationPhoneNumber
        {
            get
            {
                return this.reservationPhoneNumber;
            }
            set
            {
                this.reservationPhoneNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<string> BusinessAmenities
        {
            get
            {
                return this.businessAmenities;
            }
            set
            {
                this.businessAmenities = value;
            }
        }
    }
}
