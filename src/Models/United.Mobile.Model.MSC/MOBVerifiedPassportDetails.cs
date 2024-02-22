using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBVerifiedPassportDetails
    {
        private int id;
        private string idExpiry = "";
        private string idType = "";
        private string idDob = "";
        private string idCheckSignature = "";
        private string idCheckDataPositions = "";
        private string idCheckHologram = "";
        private string idCheckMicroprint = "";
        private string idCheckDocumentValidation = "";
        private string idCountry = "";
        private string idScanSource = "";
        private string idFirstName = "";
        private string verificationStatus = "";
        private string jumioIdScanReference = "";
        private string personalNumber = "";
        private string merchantIdScanReference = "";
        private string idCheckSecurityFeatures = "";
        private string idCheckMRZcode = "";
        private string idScanImage = "";
        private string callBackType = "";
        private string clientIp = "";
        private string idLastName = "";
        private string idAddress = "";
        private string idScanStatus = "";
        private string idNumber = "";
        private string rejectReason = "";
        private string idGender = ""; // This is for the Jumio Call Back
        private string gender = "";
        private string countryName = "";
        private string insertedDateTime;
        private string updatedDateTime;
        private string durationOfStay = "";
        private string visaCategory = "";
        private string numberOfEntries = "";
        private string passportNumber = "";
        private string issuingDate = "";
        private string nationality = "";


        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string IDExpiry
        {
            get
            {
                return idExpiry;
            }
            set
            {
                idExpiry = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //
        public string IDType
        {
            get
            {
                return idType;
            }
            set
            {
                idType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDDob
        {
            get
            {
                return idDob;
            }
            set
            {
                idDob = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCheckSignature
        {
            get
            {
                return idCheckSignature;
            }
            set
            {
                idCheckSignature = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCheckDataPositions
        {
            get
            {
                return idCheckDataPositions;
            }
            set
            {
                idCheckDataPositions = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCheckHologram
        {
            get
            {
                return idCheckHologram;
            }
            set
            {
                idCheckHologram = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCheckMicroprint
        {
            get
            {
                return idCheckMicroprint;
            }
            set
            {
                idCheckMicroprint = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCheckDocumentValidation
        {
            get
            {
                return idCheckDocumentValidation;
            }
            set
            {
                idCheckDocumentValidation = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCountry
        {
            get
            {
                return idCountry;
            }
            set
            {
                idCountry = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDScanSource
        {
            get
            {
                return idScanSource;
            }
            set
            {
                idScanSource = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDFirstName
        {
            get
            {
                return idFirstName;
            }
            set
            {
                idFirstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string VerificationStatus
        {
            get
            {
                return verificationStatus;
            }
            set
            {
                verificationStatus = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string JumioIdScanReference
        {
            get
            {
                return jumioIdScanReference;
            }
            set
            {
                jumioIdScanReference = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PersonalNumber
        {
            get
            {
                return personalNumber;
            }
            set
            {
                personalNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MerchantIdScanReference
        {
            get
            {
                return merchantIdScanReference;
            }
            set
            {
                merchantIdScanReference = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCheckSecurityFeatures
        {
            get
            {
                return idCheckSecurityFeatures;
            }
            set
            {
                idCheckSecurityFeatures = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDCheckMRZcode
        {
            get
            {
                return idCheckMRZcode;
            }
            set
            {
                idCheckMRZcode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDScanImage
        {
            get
            {
                return idScanImage;
            }
            set
            {
                idScanImage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CallBackType
        {
            get
            {
                return callBackType;
            }
            set
            {
                callBackType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClientIp
        {
            get
            {
                return clientIp;
            }
            set
            {
                clientIp = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDLastName
        {
            get
            {
                return idLastName;
            }
            set
            {
                idLastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDAddress
        {
            get
            {
                return idAddress;
            }
            set
            {
                idAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDScanStatus
        {
            get
            {
                return idScanStatus;
            }
            set
            {
                idScanStatus = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDNumber
        {
            get
            {
                return idNumber;
            }
            set
            {
                idNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string RejectReason
        {
            get
            {
                return rejectReason;
            }
            set
            {
                rejectReason = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string IDGender
        {
            get
            {
                return this.idGender;
            }
            set
            {
                this.idGender = value;
            }
        }

        public string Gender
        {
            get
            {
                return this.gender;
            }
            set
            {
                this.gender = value;
            }
        }

        public string CountryName
        {
            get
            {
                return this.countryName;
            }
            set
            {
                this.countryName = value;
            }
        }


        public string InsertedDateTime
        {
            get
            {
                return this.insertedDateTime;
            }
            set
            {
                this.insertedDateTime = value;
            }
        }

        public string UpdatedDateTime
        {
            get
            {
                return this.updatedDateTime;
            }
            set
            {
                this.updatedDateTime = value;
            }
        }

        public string DurationOfStay
        {
            get
            {
                return this.durationOfStay;
            }
            set
            {
                this.durationOfStay = value;
            }
        }

        public string VisaCategory
        {
            get
            {
                return this.visaCategory;
            }
            set
            {
                this.visaCategory = value;
            }
        }
        public string NumberOfEntries
        {
            get
            {
                return this.numberOfEntries;
            }
            set
            {
                this.numberOfEntries = value;
            }
        }

        public string PassportNumber
        {
            get
            {
                return this.passportNumber;
            }
            set
            {
                this.passportNumber = value;
            }
        }


        public string IssuingDate
        {
            get
            {
                return this.issuingDate;
            }
            set
            {
                this.issuingDate = value;
            }
        }

        public string Nationality
        {
            get
            {
                return this.nationality;
            }
            set
            {
                this.nationality = value;
            }
        }
    }

    [Serializable()]
    public class MOBDeleteScanJumioResponse
    {
        public DateTime timestamp { get; set; }
        public string jumioIdScanReference { get; set; }
        public string status { get; set; }
    }

}
