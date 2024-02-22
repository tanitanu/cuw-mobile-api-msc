using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace United.Definition.GooglePay
{
    [Serializable]
    public class MOBGooglePayFlightClassDef
    {
        private string kind;
        private string id;
        //public long version;
        private string issuerName;
        private string reviewStatus;
        private string localScheduledDepartureDateTime;
        private Flightheader flightHeader;
        private Origin origin;
        private Destination destination;
        private string localBoardingDateTime;
        private BoardingAndSeatingPolicy boardingAndSeatingPolicy;
        private string hexBackgroundColor;
        //private string localEstimatedOrActualDepartureDateTime;
        private string localScheduledArrivalDateTime;
        //private string localEstimatedOrActualArrivalDateTime;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string IssuerName
        {
            get { return issuerName; }
            set { issuerName = value; }
        }
        public string ReviewStatus
        {
            get { return reviewStatus; }
            set { reviewStatus = value; }
        }
        public string LocalScheduledDepartureDateTime
        {
            get { return localScheduledDepartureDateTime; }
            set { localScheduledDepartureDateTime = value; }
        }
        public Flightheader FlightHeader
        {
            get { return flightHeader; }
            set { flightHeader = value; }
        }
        public Origin Origin
        {
            get { return origin; }
            set { origin = value; }
        }
        public Destination Destination
        {
            get { return destination; }
            set { destination = value; }
        }
        public string LocalBoardingDateTime
        {
            get { return localBoardingDateTime; }
            set { localBoardingDateTime = value; }
        }
        public BoardingAndSeatingPolicy BoardingAndSeatingPolicy
        {
            get { return boardingAndSeatingPolicy; }
            set { boardingAndSeatingPolicy = value; }
        }
        public string HexBackgroundColor
        {
            get { return hexBackgroundColor; }
            set { hexBackgroundColor = value; }
        }
        //public string LocalEstimatedOrActualDepartureDateTime
        //{
        //    get { return localEstimatedOrActualDepartureDateTime; }
        //    set { localEstimatedOrActualDepartureDateTime = value; }
        //}
        public string LocalScheduledArrivalDateTime
        {
            get { return localScheduledArrivalDateTime; }
            set { localScheduledArrivalDateTime = value; }
        }
        //public string LocalEstimatedOrActualArrivalDateTime
        //{
        //    get { return localEstimatedOrActualArrivalDateTime; }
        //    set { localEstimatedOrActualArrivalDateTime = value; }
        //}
        public MOBGooglePayFlightClassDef()
        {
            this.Kind = "walletobjects#flightClass";
            this.IssuerName = "united-207920";
            ////this.version = "v1";
            this.ReviewStatus = "underReview";

            this.FlightHeader = new Flightheader();
            this.FlightHeader.Kind = "walletobjects#flightHeader";
            this.FlightHeader.Carrier = new Carrier();
            this.FlightHeader.Carrier.Kind = "walletobjects#flightCarrier";
            this.FlightHeader.Carrier.AirlineName = new Airlinename();
            this.FlightHeader.Carrier.AirlineName.Kind = "walletobjects#localizedString";
            //Translatedvalue translatedvalue2 = new Translatedvalue();
            //translatedvalue2.Kind = "walletobjects#translatedString";
            //translatedvalue2.Language = "en-US";
            //translatedvalue2.Value = "United Airlines";
            //this.flightHeader.Carrier.AirlineName.TranslatedValues = new List<Translatedvalue>();
            //this.flightHeader.Carrier.AirlineName.TranslatedValues.Add(translatedvalue2);
            this.FlightHeader.Carrier.AirlineName.DefaultValue = new Defaultvalue();
            this.FlightHeader.Carrier.AirlineName.DefaultValue.Kind = "walletobjects#translatedString";
            this.FlightHeader.Carrier.AirlineName.DefaultValue.Language = "en-US";
            this.FlightHeader.Carrier.AirlineName.DefaultValue.Value = "United Airlines"; //2

            this.FlightHeader.Carrier.AirlineLogo = new Airlinelogo();
            this.FlightHeader.Carrier.AirlineLogo.Kind = "walletobjects#image";
            this.FlightHeader.Carrier.AirlineLogo.SourceUri = new Sourceuri();
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.Kind = "walletobjects#uri";
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.Description = "United Airlines Logo";
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription = new Localizeddescription();
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription.Kind = "walletobjects#localizedString";
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription.DefaultValue = new Defaultvalue();
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription.DefaultValue.Kind = "walletobjects#translatedString";
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription.DefaultValue.Value = "United Airlines Logo";
            this.FlightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription.DefaultValue.Language = "en-US";
            //Translatedvalue translatedvalue3 = new Translatedvalue();
            //translatedvalue3.Kind = "walletobjects#translatedString";
            //translatedvalue3.Language = "en-US";
            //translatedvalue3.Value = "United Airlines";
            //this.flightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription.TranslatedValues = new List<Translatedvalue>();
            //this.flightHeader.Carrier.AirlineLogo.SourceUri.LocalizedDescription.TranslatedValues.Add(translatedvalue3);

            this.FlightHeader.Carrier.AirlineAllianceLogo = new Airlinealliancelogo();
            this.FlightHeader.Carrier.AirlineAllianceLogo.Kind = "walletobjects#image";
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri = new Sourceuri();
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.Kind = "walletobjects#uri";
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.Description = "Star Alliance Logo";
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription = new Localizeddescription();
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription.Kind = "walletobjects#localizedString";
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription.DefaultValue = new Defaultvalue();
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription.DefaultValue.Kind = "walletobjects#translatedString";
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription.DefaultValue.Value = "Star Alliance Logo";
            this.FlightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription.DefaultValue.Language = "en-US";
            //Translatedvalue translatedvalue4 = new Translatedvalue();
            //translatedvalue4.Kind = "walletobjects#translatedString";
            //translatedvalue4.Language = "en-US";
            //translatedvalue4.Value = "United Airlines";
            //this.flightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription.TranslatedValues = new List<Translatedvalue>();
            //this.flightHeader.Carrier.AirlineAllianceLogo.SourceUri.LocalizedDescription.TranslatedValues.Add(translatedvalue4);
            //this.FlightHeader.Carrier.AirlineAlliance = "starAlliance";
            this.BoardingAndSeatingPolicy = new BoardingAndSeatingPolicy();
            this.BoardingAndSeatingPolicy.Kind = "walletobjects#boardingAndSeatingPolicy";
            this.BoardingAndSeatingPolicy.BoardingPolicy = "groupBased";
            this.boardingAndSeatingPolicy.SeatClassPolicy = "cabinBased";

            this.Origin = new Origin();
            this.Origin.Kind = "walletobjects#airportInfo";
            this.Destination = new Destination();
            this.Destination.Kind = "walletobjects#airportInfo";

            this.FlightHeader.OperatingCarrier = new Operatingcarrier();
            this.FlightHeader.OperatingCarrier.Kind = "walletobjects#flightCarrier";
            this.FlightHeader.OperatingCarrier.AirlineName = new Airlinename();
            this.FlightHeader.OperatingCarrier.AirlineName.Kind = "walletobjects#localizedString";
            this.FlightHeader.OperatingCarrier.AirlineName.DefaultValue = new Defaultvalue();
            this.FlightHeader.OperatingCarrier.AirlineName.DefaultValue.Kind = "walletobjects#translatedString";
            this.FlightHeader.OperatingCarrier.AirlineName.DefaultValue.Language = "en-US";
            //this.FlightHeader.Carrier.AirlineName.DefaultValue.Value = "United Airlines"; //2
        }
    }
    [Serializable]
    public class MOBGooglePayFlightObjectDef
    {
        private string kind;
        private string id;
        private string classId;
        private long version;
        private string state;
        private string passengerName;
        private Boardingandseatinginfo boardingAndSeatingInfo;
        private Reservationinfo reservationInfo;
        private Securityprogramlogo securityProgramLogo;
        private Barcode barcode;
        private List<Textmodulesdata> textModulesData;
        private Linksmoduledata linksModuleData;
        private List<Imagemodulesdata> imageModulesData;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        public string ClassId
        {
            get { return classId; }
            set { classId = value; }
        }
        public long Version
        {
            get { return version; }
            set { version = value; }
        }
        public string State
        {
            get { return state; }
            set { state = value; }
        }
        public string PassengerName
        {
            get { return passengerName; }
            set { passengerName = value; }
        }
        public Boardingandseatinginfo BoardingAndSeatingInfo
        {
            get { return boardingAndSeatingInfo; }
            set { boardingAndSeatingInfo = value; }
        }
        public Reservationinfo ReservationInfo
        {
            get { return reservationInfo; }
            set { reservationInfo = value; }
        }
        public Securityprogramlogo SecurityProgramLogo
        {
            get { return securityProgramLogo; }
            set { securityProgramLogo = value; }
        }
        public Barcode BarCode
        {
            get { return barcode; }
            set { barcode = value; }
        }
        public List<Textmodulesdata> TextModulesData
        {
            get { return textModulesData; }
            set { textModulesData = value; }
        }
        public Linksmoduledata LinksModuleData
        {
            get { return linksModuleData; }
            set { linksModuleData = value; }
        }
        public List<Imagemodulesdata> ImageModulesData
        {
            get { return imageModulesData; }
            set { imageModulesData = value; }
        }

        public MOBGooglePayFlightObjectDef()
        {
            this.Kind = "walletobjects#flightObject";

            this.version = 1;
            this.State = "active";

            this.BoardingAndSeatingInfo = new Boardingandseatinginfo();
            this.BoardingAndSeatingInfo.Kind = "walletobjects#boardingAndSeatingInfo";
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage = new Boardingprivilegeimage();
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.Kind = "walletobjects#image";
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri = new Sourceuri();
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.Kind = "walletobjects#uri";
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.Description = "United Privilege Image";
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription = new Localizeddescription();
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription.Kind = "walletobjects#localizedString";
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription.DefaultValue = new Defaultvalue();
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription.DefaultValue.Kind = "walletobjects#translatedString";
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription.DefaultValue.Value = "United Privilege Image";
            this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription.DefaultValue.Language = "en-US";
            //Translatedvalue translatedvalue = new Translatedvalue();
            //translatedvalue.Kind = "walletobjects#translatedString";
            //translatedvalue.Language = "en-US";
            //translatedvalue.Value = "United Privilege Image";
            //this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription.TranslatedValues = new List<Translatedvalue>();
            //this.BoardingAndSeatingInfo.BoardingPrivilegeImage.SourceUri.LocalizedDescription.TranslatedValues.Add(translatedvalue);

            this.ReservationInfo = new Reservationinfo();
            this.ReservationInfo.Kind = "walletobjects#reservationInfo";
            this.ReservationInfo.FrequentFlyerInfo = new Frequentflyerinfo();
            this.ReservationInfo.FrequentFlyerInfo.Kind = "walletobjects#frequentFlyerInfo";
            this.ReservationInfo.FrequentFlyerInfo.FrequentFlyerProgramName = new Frequentflyerprogramname();
            this.ReservationInfo.FrequentFlyerInfo.FrequentFlyerProgramName.Kind = "walletobjects#localizedString";
            this.ReservationInfo.FrequentFlyerInfo.FrequentFlyerProgramName.DefaultValue = new Defaultvalue();
            this.ReservationInfo.FrequentFlyerInfo.FrequentFlyerProgramName.DefaultValue.Kind = "walletobjects#translatedString";
            this.ReservationInfo.FrequentFlyerInfo.FrequentFlyerProgramName.DefaultValue.Language = "en-US";

            this.BarCode = new Barcode();
            this.BarCode.Kind = "walletobjects#barcode";
            this.BarCode.Type = "aztec";

            this.SecurityProgramLogo = new Securityprogramlogo();
            this.SecurityProgramLogo.Kind = "walletobjects#image";
            this.SecurityProgramLogo.SourceUri = new Sourceuri();
            this.SecurityProgramLogo.SourceUri.Kind = "walletobjects#uri";
            this.SecurityProgramLogo.SourceUri.Description = "Security Program Logo";
            this.SecurityProgramLogo.SourceUri.LocalizedDescription = new Localizeddescription();
            this.SecurityProgramLogo.SourceUri.LocalizedDescription.Kind = "walletobjects#localizedString";
            this.SecurityProgramLogo.SourceUri.LocalizedDescription.DefaultValue = new Defaultvalue();
            this.SecurityProgramLogo.SourceUri.LocalizedDescription.DefaultValue.Kind = "walletobjects#translatedString";
            this.SecurityProgramLogo.SourceUri.LocalizedDescription.DefaultValue.Value = "Security Program Logo";
            this.SecurityProgramLogo.SourceUri.LocalizedDescription.DefaultValue.Language = "en-US";
            //Translatedvalue translatedvalue5 = new Translatedvalue();
            //translatedvalue5.Kind = "walletobjects#translatedString";
            //translatedvalue5.Language = "en-US";
            //translatedvalue5.Value = "United Privilege Image";
            //this.SecurityProgramLogo.SourceUri.LocalizedDescription.TranslatedValues = new List<Translatedvalue>();
            //this.SecurityProgramLogo.SourceUri.LocalizedDescription.TranslatedValues.Add(translatedvalue5);            
            this.LinksModuleData = new Linksmoduledata();
            Localizeddescription1 localizeddescription0 = new Localizeddescription1();
            localizeddescription0.Kind = "walletobjects#localizedString";
            localizeddescription0.DefaultValue = new Defaultvalue();
            localizeddescription0.DefaultValue.Kind = "walletobjects#translatedString";
            localizeddescription0.DefaultValue.Value = "www.united.com";
            localizeddescription0.DefaultValue.Language = "en-US";
            Uris uris0 = new Uris();
            uris0.Kind = "walletobjects#uri";
            uris0.Uri = "https://www.united.com";
            uris0.Description = "Website";
            uris0.LocalizedDescription = localizeddescription0;

            Localizeddescription1 localizeddescription1 = new Localizeddescription1();
            localizeddescription1.Kind = "walletobjects#localizedString";
            localizeddescription1.DefaultValue = new Defaultvalue();
            localizeddescription1.DefaultValue.Kind = "walletobjects#translatedString";
            localizeddescription1.DefaultValue.Value = "Help";
            localizeddescription1.DefaultValue.Language = "en-US";
            Uris uris1 = new Uris();
            uris1.Kind = "walletobjects#uri";
            uris1.Uri = "https://www.united.com/ual/en/us/fly/help.html";
            uris1.Description = "Help";
            uris1.LocalizedDescription = localizeddescription1;

            Localizeddescription1 localizeddescription2 = new Localizeddescription1();
            localizeddescription2.Kind = "walletobjects#localizedString";
            localizeddescription2.DefaultValue = new Defaultvalue();
            localizeddescription2.DefaultValue.Kind = "walletobjects#translatedString";
            localizeddescription2.DefaultValue.Value = "(800) 864-8331";
            localizeddescription2.DefaultValue.Language = "en-US";
            Uris uris2 = new Uris();
            uris2.Kind = "walletobjects#uri";
            uris2.Uri = "tel:8008648331";
            uris2.Description = "Telephone";
            uris2.LocalizedDescription = localizeddescription2;
            this.LinksModuleData.Uris = new List<Uris>();
            this.LinksModuleData.Uris.Add(uris0);
            this.LinksModuleData.Uris.Add(uris1);
            this.LinksModuleData.Uris.Add(uris2);

            Localizeddescription localizeddescription3 = new Localizeddescription();
            localizeddescription3.Kind = "walletobjects#localizedString";
            localizeddescription3.DefaultValue = new Defaultvalue();
            localizeddescription3.DefaultValue.Kind = "walletobjects#translatedString";
            localizeddescription3.DefaultValue.Value = "Main Image";
            localizeddescription3.DefaultValue.Language = "en-US";
            Mainimage mainImage = new Mainimage();
            mainImage.Kind = "walletobjects#image";
            mainImage.SourceUri = new Sourceuri();
            mainImage.SourceUri.Kind = "walletobjects#uri";
            mainImage.SourceUri.Uri = ConfigurationManager.AppSettings["GooglePay_MainImage_ImageUrl"].ToString();
            mainImage.SourceUri.Description = "Main Image";
            mainImage.SourceUri.LocalizedDescription = localizeddescription3;
            Imagemodulesdata imagemodulesdata = new Imagemodulesdata();
            imagemodulesdata.MainImage = mainImage;
            this.ImageModulesData = new List<Imagemodulesdata>();
            this.imageModulesData.Add(imagemodulesdata);
        }
    }

    [Serializable]
    public class Imagemodulesdata
    {
        private Mainimage mainImage;

        public Mainimage MainImage
        {
            get { return mainImage; }
            set { mainImage = value; }
        }
    }

    [Serializable]
    public class Mainimage
    {
        private string kind;
        private Sourceuri sourceUri;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public Sourceuri SourceUri
        {
            get { return sourceUri; }
            set { sourceUri = value; }
        }
    }

    [Serializable]
    public class Linksmoduledata
    {
        private List<Uris> uris;

        public List<Uris> Uris
        {
            get { return uris; }
            set { uris = value; }
        }
    }

    [Serializable]
    public class Uris
    {
        private string kind;
        private string uri;
        private string description;
        private Localizeddescription1 localizedDescription;
        //private List<Translatedvalue> translatedValues;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        public Localizeddescription1 LocalizedDescription
        {
            get { return localizedDescription; }
            set { localizedDescription = value; }
        }
        //public List<Translatedvalue> TranslatedValues
        //{
        //    get { return translatedValues; }
        //    set { translatedValues = value; }
        //}
    }

    [Serializable]
    public class Textmodulesdata
    {
        private string header;
        private string body;

        public string Header
        {
            get { return header; }
            set { header = value; }
        }
        public string Body
        {
            get { return body; }
            set { body = value; }
        }
    }

    [Serializable]
    public class Securityprogramlogo
    {
        private string kind;
        private Sourceuri sourceUri;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public Sourceuri SourceUri
        {
            get { return sourceUri; }
            set { sourceUri = value; }
        }
    }

    [Serializable]
    public class Flightheader
    {
        private string kind;
        private Carrier carrier;
        private string flightNumber;
        private Operatingcarrier operatingCarrier;
        private string operatingFlightNumber;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public Carrier Carrier
        {
            get { return carrier; }
            set { carrier = value; }
        }
        public string FlightNumber
        {
            get { return flightNumber; }
            set { flightNumber = value; }
        }
        public Operatingcarrier OperatingCarrier
        {
            get { return operatingCarrier; }
            set { operatingCarrier = value; }
        }
        public string OperatingFlightNumber
        {
            get { return operatingFlightNumber; }
            set { operatingFlightNumber = value; }
        }
    }
    [Serializable]
    public class Operatingcarrier
    {
        private string kind;
        private string carrierIataCode;
        private Airlinename airlineName;
        //private Airlinelogo airlineLogo;
        //private Airlinealliancelogo airlineAllianceLogo;
        //private string airlineAlliance;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string CarrierIataCode
        {
            get { return carrierIataCode; }
            set { carrierIataCode = value; }
        }
        public Airlinename AirlineName
        {
            get { return airlineName; }
            set { airlineName = value; }
        }
        //public Airlinelogo AirlineLogo
        //{
        //    get { return airlineLogo; }
        //    set { airlineLogo = value; }
        //}
        //public Airlinealliancelogo AirlineAllianceLogo
        //{
        //    get { return airlineAllianceLogo; }
        //    set { airlineAllianceLogo = value; }
        //}
        //public string AirlineAlliance
        //{
        //    get { return airlineAlliance; }
        //    set { airlineAlliance = value; }
        //}
    }
    [Serializable]
    public class BoardingAndSeatingPolicy
    {
        private string kind;
        private string boardingPolicy;
        //private string seatingPolicy;
        private string seatClassPolicy;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string BoardingPolicy
        {
            get { return boardingPolicy; }
            set { boardingPolicy = value; }
        }
        //public string SeatingPolicy
        //{
        //    get { return seatingPolicy; }
        //    set { seatingPolicy = value; }
        //}
        public string SeatClassPolicy
        {
            get { return seatClassPolicy; }
            set { seatClassPolicy = value; }
        }
    }
    [Serializable]
    public class Barcode
    {
        private string kind;
        private string type;
        private string value;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
    [Serializable]
    public class Carrier
    {
        private string kind;
        private string carrierIataCode;
        private Airlinename airlineName;
        private Airlinelogo airlineLogo;
        private Airlinealliancelogo airlineAllianceLogo;
        //private string airlineAlliance;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string CarrierIataCode
        {
            get { return carrierIataCode; }
            set { carrierIataCode = value; }
        }
        public Airlinename AirlineName
        {
            get { return airlineName; }
            set { airlineName = value; }
        }
        public Airlinelogo AirlineLogo
        {
            get { return airlineLogo; }
            set { airlineLogo = value; }
        }
        public Airlinealliancelogo AirlineAllianceLogo
        {
            get { return airlineAllianceLogo; }
            set { airlineAllianceLogo = value; }
        }
        //public string AirlineAlliance
        //{
        //    get { return airlineAlliance; }
        //    set { airlineAlliance = value; }
        //}
    }
    [Serializable]
    public class Airlinename
    {
        private string kind;
        private List<Translatedvalue> translatedValues;
        private Defaultvalue defaultValue;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public List<Translatedvalue> TranslatedValues
        {
            get { return translatedValues; }
            set { translatedValues = value; }
        }
        public Defaultvalue DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }
    }
    [Serializable]
    public class Defaultvalue
    {
        private string kind;
        private string language;
        private string value;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string Language
        {
            get { return language; }
            set { language = value; }
        }
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
    [Serializable]
    public class Translatedvalue
    {
        private string kind;
        private string language;
        private string value;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string Language
        {
            get { return language; }
            set { language = value; }
        }
        public string Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
    [Serializable]
    public class Airlinelogo
    {
        private string kind;
        private Sourceuri sourceUri;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public Sourceuri SourceUri
        {
            get { return sourceUri; }
            set { sourceUri = value; }
        }
    }
    [Serializable]
    public class Sourceuri
    {
        private string kind;
        private string uri;
        private string description;
        private Localizeddescription localizedDescription;
        //private List<Translatedvalue> translatedValues;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string Uri
        {
            get { return uri; }
            set { uri = value; }
        }
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        public Localizeddescription LocalizedDescription
        {
            get { return localizedDescription; }
            set { localizedDescription = value; }
        }
        //public List<Translatedvalue> TranslatedValues
        //{
        //    get { return translatedValues; }
        //    set { translatedValues = value; }
        //}
    }
    [Serializable]
    public class Localizeddescription
    {
        private string kind;
        //private List<Translatedvalue> translatedValues;
        private Defaultvalue defaultValue;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        //public List<Translatedvalue> TranslatedValues
        //{
        //    get { return translatedValues; }
        //    set { translatedValues = value; }
        //}
        public Defaultvalue DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }
    }
    [Serializable]
    public class Localizeddescription1
    {
        private string kind;
        //private List<Translatedvalue> translatedValues;
        private Defaultvalue defaultValue;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        //public List<Translatedvalue> TranslatedValues
        //{
        //    get { return translatedValues; }
        //    set { translatedValues = value; }
        //}
        public Defaultvalue DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }
    }
    [Serializable]
    public class Airlinealliancelogo
    {
        private string kind;
        private Sourceuri sourceUri;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public Sourceuri SourceUri
        {
            get { return sourceUri; }
            set { sourceUri = value; }
        }
    }
    [Serializable]
    public class Origin
    {
        private string kind;
        private string airportIataCode;
        //private string terminal;
        private string gate;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string AirportIataCode
        {
            get { return airportIataCode; }
            set { airportIataCode = value; }
        }
        //public string Terminal
        //{
        //    get { return terminal; }
        //    set { terminal = value; }
        //}
        public string Gate
        {
            get { return gate; }
            set { gate = value; }
        }
    }
    [Serializable]
    public class Destination
    {
        private string kind;
        private string airportIataCode;
        //private string terminal;
        //private string gate;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string AirportIataCode
        {
            get { return airportIataCode; }
            set { airportIataCode = value; }
        }
        //public string Terminal
        //{
        //    get { return terminal; }
        //    set { terminal = value; }
        //}
        //public string Gate
        //{
        //    get { return gate; }
        //    set { gate = value; }
        //}
    }
    [Serializable]
    public class Boardingandseatinginfo
    {
        private string kind;
        private string boardingGroup;
        private string seatNumber;
        //private string boardingPosition;
        private string sequenceNumber;
        private string seatClass;
        private Boardingprivilegeimage boardingPrivilegeImage;
        //private string boardingDoor;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string BoardingGroup
        {
            get { return boardingGroup; }
            set { boardingGroup = value; }
        }
        public string SeatNumber
        {
            get { return seatNumber; }
            set { seatNumber = value; }
        }
        //public string BoardingPosition
        //{
        //    get { return boardingPosition; }
        //    set { boardingPosition = value; }
        //}
        public string SequenceNumber
        {
            get { return sequenceNumber; }
            set { sequenceNumber = value; }
        }
        public string SeatClass
        {
            get { return seatClass; }
            set { seatClass = value; }
        }
        public Boardingprivilegeimage BoardingPrivilegeImage
        {
            get { return boardingPrivilegeImage; }
            set { boardingPrivilegeImage = value; }
        }
        //public string BoardingDoor
        //{
        //    get { return boardingDoor; }
        //    set { boardingDoor = value; }
        //    }
    }
    [Serializable]
    public class Boardingprivilegeimage
    {
        private string kind;
        private Sourceuri sourceUri;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public Sourceuri SourceUri
        {
            get { return sourceUri; }
            set { sourceUri = value; }
        }
    }
    [Serializable]
    public class Reservationinfo
    {
        private string kind;
        private string confirmationCode;
        //private string eticketNumber;
        private Frequentflyerinfo frequentFlyerInfo;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public string ConfirmationCode
        {
            get { return confirmationCode; }
            set { confirmationCode = value; }
        }
        //public string ETicketNumber
        //{
        //    get { return eticketNumber; }
        //    set { eticketNumber = value; }
        //}
        public Frequentflyerinfo FrequentFlyerInfo
        {
            get { return frequentFlyerInfo; }
            set { frequentFlyerInfo = value; }
        }
    }
    [Serializable]
    public class Frequentflyerinfo
    {
        private string kind;
        private Frequentflyerprogramname frequentFlyerProgramName;
        private string frequentFlyerNumber;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public Frequentflyerprogramname FrequentFlyerProgramName
        {
            get { return frequentFlyerProgramName; }
            set { frequentFlyerProgramName = value; }
        }
        public string FrequentFlyerNumber
        {
            get { return frequentFlyerNumber; }
            set { frequentFlyerNumber = value; }
        }
    }
    [Serializable]
    public class Frequentflyerprogramname
    {
        private string kind;
        private List<Translatedvalue> translatedValues;
        private Defaultvalue defaultValue;

        public string Kind
        {
            get { return kind; }
            set { kind = value; }
        }
        public List<Translatedvalue> TranslatedValues
        {
            get { return translatedValues; }
            set { translatedValues = value; }
        }
        public Defaultvalue DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }
    }
}
