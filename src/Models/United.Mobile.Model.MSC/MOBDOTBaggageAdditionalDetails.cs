using System;
using System.Collections.Generic;
using System.Configuration;

namespace United.Definition
{
    [Serializable]
    public class MOBDOTBaggageAdditionalDetails
    {
        public MOBDOTBaggageAdditionalDetails()
        {

        }
        public MOBDOTBaggageAdditionalDetails(bool getDOTStaticInfoText)
        {
            if (getDOTStaticInfoText)
            {
                title1 = ConfigurationManager.AppSettings["DOTBaggageAdditionalDetailsTitle1"].Split('|')[0];
                title2 = ConfigurationManager.AppSettings["DOTBaggageAdditionalDetailsTitle2"].Split('|')[0];

                additionalOtherBagFeesTitle = ConfigurationManager.AppSettings["AdditionalOtherBagFeeTitle"].Split('|')[0];
                additionalOtherBagFeesNote = ConfigurationManager.AppSettings["AdditionalOtherBagFeeNote"].Split('|')[0];

                defaultCheckInBagDimensions = ConfigurationManager.AppSettings["DefaultCheckInBagDimensions"].Split('|')[0];
            }
        }
        private string title1 = string.Empty;
        private string title2 = string.Empty;
        private string freeBagsHeaderText = string.Empty;
        private string freeBagsDescriptionText = string.Empty;
        private string oaCarrierText = string.Empty;
        private MOBBagFeesPerSegment baggageFeesPerSegment;
        private string additionalOtherBagFeesTitle = string.Empty;
        private List<MOBAdditionalBagDetails> additionalAndOverSizeOverWeightBagDetails;
        private string additionalOtherBagFeesNote = string.Empty;
        private string chaseCardFreeFirstBagHeaderText = string.Empty;
        private string chaseCardFreeFirstBagDescriptionText = string.Empty;
        private string defaultCheckInBagDimensions = string.Empty;
        private MOBBagFeesPerSegment iBeLiteBaggageFeesPerSegment;
        private MOBBagFeesPerSegment iBeBaggageFeesPerSegment;

        public string Title1
        {
            get { return title1; }
            set { title1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Title2
        {
            get { return title2; }
            set { title2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FreeBagsHeaderText
        {
            get { return freeBagsHeaderText; }
            set { freeBagsHeaderText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string FreeBagsDescriptionText
        {
            get { return freeBagsDescriptionText; }
            set { freeBagsDescriptionText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string OACarrierText
        {
            get { return oaCarrierText; }
            set { oaCarrierText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public MOBBagFeesPerSegment BaggageFeesPerSegment
        {
            get { return baggageFeesPerSegment; }
            set { baggageFeesPerSegment = value; }
        }

        public string AdditionalOtherBagFeesTitle
        {
            get { return additionalOtherBagFeesTitle; }
            set { additionalOtherBagFeesTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public List<MOBAdditionalBagDetails> AdditionalAndOverSizeOverWeightBagDetails
        {
            get { return additionalAndOverSizeOverWeightBagDetails; }
            set { additionalAndOverSizeOverWeightBagDetails = value; }
        }

        public string AdditionalOtherBagFeesNote
        {
            get { return additionalOtherBagFeesNote; }
            set { additionalOtherBagFeesNote = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ChaseCardFreeFirstBagHeaderText
        {
            get { return chaseCardFreeFirstBagHeaderText; }
            set { chaseCardFreeFirstBagHeaderText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ChaseCardFreeFirstBagDescriptionText
        {
            get { return chaseCardFreeFirstBagDescriptionText; }
            set { chaseCardFreeFirstBagDescriptionText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string DefaultCheckInBagDimensions
        {
            get { return defaultCheckInBagDimensions; }
            set { defaultCheckInBagDimensions = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }


        public MOBBagFeesPerSegment IBeLiteBaggageFeesPerSegment
        {
            get { return iBeLiteBaggageFeesPerSegment; }
            set { iBeLiteBaggageFeesPerSegment = value; }
        }

        public MOBBagFeesPerSegment IBeBaggageFeesPerSegment
        {
            get { return iBeBaggageFeesPerSegment; }
            set { iBeBaggageFeesPerSegment = value; }
        }
    }
}
