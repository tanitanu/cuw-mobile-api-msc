using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Gamification
{
    [Serializable]
    public class MOBMileagePlusEligiblePromotionsResponse : MOBResponse
    {
        private MOBGamificationPromotionResponse milesPlayPromotionResponse;
        private MOBPersonalThresholdPromotionResponse personalThresholdPromotionResponse;
        private MOBStatusLiftPromotionResponse statusLiftResponse;
        public MOBGamificationPromotionResponse MilesPlayPromotionResponse
        {
            get { return milesPlayPromotionResponse; }
            set { milesPlayPromotionResponse = value; }
        }       

        public MOBPersonalThresholdPromotionResponse PersonalThresholdPromotionResponse
        {
            get { return personalThresholdPromotionResponse; }
            set { personalThresholdPromotionResponse = value; }
        }

        public MOBStatusLiftPromotionResponse StatusLiftResponse
        {
            get { return statusLiftResponse; }
            set { statusLiftResponse = value; }
        }

    }
}
