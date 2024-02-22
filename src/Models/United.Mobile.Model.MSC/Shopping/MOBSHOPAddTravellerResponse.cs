using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Reward.Configuration;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAddTravellerResponse : MOBResponse
    {
        public MOBSHOPAddTravellerResponse()
        {
            rewardPrograms = new List<MOBSHOPRewardProgram>();
            ConfigurationParameter.ConfigParameter parm = ConfigurationParameter.ConfigParameter.Configuration;
            for (int i = 0; i < parm.RewardTypes.Count; i++)
            {
                MOBSHOPRewardProgram p = new MOBSHOPRewardProgram();
                p.Type = parm.RewardTypes[i].Type;
                p.Description = parm.RewardTypes[i].Description;
                rewardPrograms.Add(p);
            }
        }
        
        private string sessionId = string.Empty;

        private MOBSHOPAddTravellerRequest addTravellerRequest;

        private MOBSHOPTraveler currentTraveler;

        private List<MOBSHOPTraveler> travelers;

        private List<MOBSHOPRewardProgram> rewardPrograms;

        private bool isLastTraveler;

        private string phoneNumberDisclaimer = ConfigurationManager.AppSettings["PhoneNumberDisclaimer"];

        private List<MOBTypeOption> disclaimer = null;

        private long profileOwnerCustomerId;

        private string profileOwnerMPAccountNumber;

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPAddTravellerRequest AddTravellerRequest
        {
            get
            {
                return this.addTravellerRequest;
            }
            set
            {
                this.addTravellerRequest = value;
            }
        }

        public MOBSHOPTraveler CurrentTraveler
        {
            get { return this.currentTraveler; }
            set { this.currentTraveler = value; }
        }

        public List<MOBSHOPTraveler> Travelers
        {
            get { return this.travelers; }
            set { this.travelers = value; }
        }

        public List<MOBSHOPRewardProgram> RewardPrograms
        {
            get { return this.rewardPrograms; }
            set { this.rewardPrograms = value; }
        }

        public bool IsLastTraveler
        {
            get { return this.isLastTraveler; }
            set { this.isLastTraveler = value; }
        }

        public string PhoneNumberDisclaimer
        {
            get
            {
                return this.phoneNumberDisclaimer;
            }
            set
            {

            }
        }

        public long ProfileOwnerCustomerId
        {
            get { return this.profileOwnerCustomerId; }
            set { this.profileOwnerCustomerId = value; }
        }

        public string ProfileOwnerMPAccountNumber
        {
            get { return this.profileOwnerMPAccountNumber; }
            set { this.profileOwnerMPAccountNumber = value; }
        }

        public List<MOBTypeOption> Disclaimer
        {
            get { return disclaimer; }
            set { disclaimer = value; }
        }
    }
}
