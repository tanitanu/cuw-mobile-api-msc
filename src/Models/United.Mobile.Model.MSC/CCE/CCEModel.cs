using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using United.Persist.Definition.CCE;

namespace United.Mobile.Model.MSC.CCE
{
    [Serializable()]
    [XmlRoot("ArrayOfCCEFlightReservationResponseByCartId")]
    public class ArrayOfCCEFlightReservationResponseByCartId
    {
        public List<CCEFlightReservationResponseByCartId> CCEFlightReservationResponseByCartId { get; set; }

    }

}

