using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBBaggageCalculatorSearchResponse : MOBResponse
    {
        public MOBBaggageCalculatorSearchResponse()
            : base()
        {

        }

        public MOBBaggageCalculatorSearchResponse(int applicationID)
            : base()
        {
            loyaltyLevels = new List<MOBMemberShipStatus>();

            if (applicationID == 3)
            {
                #region
                loyaltyLevels.Add(new MOBMemberShipStatus("GeneralMember", "General Member", "1", ""));

                loyaltyLevels.Add(new MOBMemberShipStatus("PremierSilver", "Premier Silver", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("PremierGold", "Premier Gold", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("PremierPlatinum", "Premier Platinum", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("Premier1K", "Premier 1K", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("GlobalServices", "Global Services", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("StarAllianceSilver", "Star Alliance Silver", "3", "Star Alliance status"));

                loyaltyLevels.Add(new MOBMemberShipStatus("StarAllianceGold", "Star Alliance Gold", "3", "Star Alliance status"));

                loyaltyLevels.Add(new MOBMemberShipStatus("PPC", "Presidental Plus Card", "4", "MileagePlus cardmember"));

                loyaltyLevels.Add(new MOBMemberShipStatus("MEC", "MileagePlus Explorer Card", "4", "MileagePlus cardmember"));

                //loyaltyLevels.Add(new MemberShipStatus("OPC", "One Pass Club", "4", "MileagePlus cardmember"));

                loyaltyLevels.Add(new MOBMemberShipStatus("CCC", "Chase Club Card", "4", "MileagePlus cardmember"));

                loyaltyLevels.Add(new MOBMemberShipStatus("MIL", "Active U.S. military(leisure travel)", "5", "Active U.S. military"));

                loyaltyLevels.Add(new MOBMemberShipStatus("MIR", "Active U.S. military(on duty)", "5", "Active U.S. military"));
                #endregion
            }
            else
            {
                #region
                loyaltyLevels.Add(new MOBMemberShipStatus("GeneralMember", "General Member", "1", ""));

                loyaltyLevels.Add(new MOBMemberShipStatus("PremierSilver", "Premier Silver member", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("PremierGold", "Premier Gold member", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("PremierPlatinum", "Premier Platinum member", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("Premier1K", "Premier 1K member", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("GlobalServices", "Global Services member", "2", "MileagePlus Premier® member"));

                loyaltyLevels.Add(new MOBMemberShipStatus("StarAllianceGold", "Star Alliance Gold member", "3", "Star Alliance status"));

                loyaltyLevels.Add(new MOBMemberShipStatus("StarAllianceSilver", "Star Alliance Silver member", "3", "Star Alliance status"));

                loyaltyLevels.Add(new MOBMemberShipStatus("MEC", "MileagePlus Explorer Card member", "4", "MileagePlus cardmember"));

                //loyaltyLevels.Add(new MemberShipStatus("OPC", "OnePass Plus Card member", "4", "MileagePlus cardmember"));

                loyaltyLevels.Add(new MOBMemberShipStatus("CCC", "MileagePlus Club Card member", "4", "MileagePlus cardmember"));

                loyaltyLevels.Add(new MOBMemberShipStatus("PPC", "Presidental Plus Card member", "4", "MileagePlus cardmember"));

                loyaltyLevels.Add(new MOBMemberShipStatus("MIR", "U.S. Military on orders or relocating", "5", "Active U.S. military"));

                loyaltyLevels.Add(new MOBMemberShipStatus("MIL", "U.S. Military personal travel", "5", "Active U.S. military"));
                #endregion
            }
        }

        private List<MOBMemberShipStatus> loyaltyLevels;


        private List<MOBCarrierInfo> carriers;

        private List<MOBClassOfService> classOfServices;

        public List<MOBMemberShipStatus> LoyaltyLevels
        {
            get
            {
                return this.loyaltyLevels;
            }
            set
            {
                this.loyaltyLevels = value;
            }
        }

        public List<MOBCarrierInfo> Carriers
        {
            get
            {
                return this.carriers;
            }
            set
            {
                this.carriers = value;
            }
        }

        public List<MOBClassOfService> ClassOfServices
        {
            get
            {
                return this.classOfServices;
            }
            set
            {
                this.classOfServices = value;
            }
        }
    }
}
