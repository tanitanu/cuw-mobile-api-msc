using System.Runtime.Serialization;

namespace United.Definition.FFC
{
    [DataContract]
    public class PinDetail
    {
        [DataMember(EmitDefaultValue = false)]
        public virtual string PromoID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual string CertPin { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual string PNR { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual int TrackNum { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual string FlightNum { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual string CertNum { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual string FreqFlyerNum { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual int CertIntNum { get; set; }

    }
}
