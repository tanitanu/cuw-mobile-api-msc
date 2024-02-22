using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Services.FlightShopping.Common.Extensions;
using United.Utility.Helper;

namespace United.Common.Helper.MSCPayment.Services
{
    public class MSCBaggageInfo : IMSCBaggageInfo
    {
        private const string DOTBaggageInfoDBTitle1 = "DOTBaggageInfoText1";
        private const string DOTBaggageInfoDBTitle1ELF = "DOTBaggageInfoText1 - ELF";
        private const string DOTBaggageInfoDBTitle1IBE = "DOTBaggageInfoText1 - IBE";
        private const string DOTBaggageInfoDBTitle2 = "DOTBaggageInfoText2";
        private const string DOTBaggageInfoDBTitle3 = "DOTBaggageInfoText3";
        private const string DOTBaggageInfoDBTitle3IBE = "DOTBaggageInfoText3IBE";
        private const string DOTBaggageInfoDBTitle4 = "DOTBaggageInfoText4";
        private List<United.Definition.MOBLegalDocument> cachedLegalDocuments = null;
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly DocumentLibraryDynamoDB _documentLibraryDynamoDB;

        private static readonly List<string> Titles = new List<string>
        {
            DOTBaggageInfoDBTitle1,
            DOTBaggageInfoDBTitle1ELF,
            DOTBaggageInfoDBTitle2,
            DOTBaggageInfoDBTitle3,
            DOTBaggageInfoDBTitle4,
            DOTBaggageInfoDBTitle1IBE,
            DOTBaggageInfoDBTitle3IBE
        };

        public MSCBaggageInfo(IConfiguration configuration
            , IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
            _documentLibraryDynamoDB = new DocumentLibraryDynamoDB(_configuration, _dynamoDBService);
        }

        public async System.Threading.Tasks.Task<MOBDOTBaggageInfo> GetBaggageInfo(MOBSHOPReservation reservation)
        {
            var isElf = reservation != null && reservation.IsELF;

            var isIBE = ConfigUtility.EnableIBEFull() && reservation != null && (reservation.ShopReservationInfo2 != null) && reservation.ShopReservationInfo2.IsIBE;

            return await GetBaggageInfo(isElf, isIBE);
        }

        public async System.Threading.Tasks.Task<MOBDOTBaggageInfo> GetBaggageInfo(MOBPNR pnr)
        {
            var isElf = pnr != null && pnr.IsElf;
            var isIbe = pnr != null && pnr.IsIBE;
            return await GetBaggageInfo(isElf, isIbe);
        }

        private async System.Threading.Tasks.Task<MOBDOTBaggageInfo> GetBaggageInfo(bool isElf, bool isIBE)
        {
            if (cachedLegalDocuments.IsNull())
            {
                cachedLegalDocuments = await GetLegalDocumentsForTitles(Titles);
            }
            var legalDocuments = cachedLegalDocuments.Clone();

            if (isElf || isIBE)
            {
                legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle1);
                if (isIBE)
                {
                    legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle1ELF);
                    legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle3);
                }
                else
                {
                    legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle1IBE);
                    legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle3IBE);
                }
            }
            else
            {
                legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle1ELF);
                legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle1IBE);
                legalDocuments.RemoveAll(l => l.Title == DOTBaggageInfoDBTitle3IBE);
            }

            var document1TitleAndDescription = legalDocuments.First(l => l.Title.Contains(DOTBaggageInfoDBTitle1)).Document.Split('|');
            var document2TitleAndDescription = legalDocuments.First(l => l.Title.Contains(DOTBaggageInfoDBTitle2)).Document.Split('|');
            var document3TitleAndDescription = legalDocuments.First(l => l.Title.Contains(DOTBaggageInfoDBTitle3)).Document.Split('|');
            var document4 = legalDocuments.First(l => l.Title.Contains(DOTBaggageInfoDBTitle4)).Document;

            return new MOBDOTBaggageInfo
            {
                Title1 = document1TitleAndDescription[0],
                Title2 = document2TitleAndDescription[0],
                Title3 = document3TitleAndDescription[0],
                Description1 = document1TitleAndDescription[1],
                Description2 = document2TitleAndDescription[1],
                Description3 = document3TitleAndDescription[1],
                Description4 = document4
            };
        }

        private async System.Threading.Tasks.Task<List<United.Definition.MOBLegalDocument>> GetLegalDocumentsForTitles(List<string> titles)
        {
            List<United.Definition.MOBLegalDocument> documents = null;

            if (titles != null && titles.Count > 0)
            {
                documents = await _documentLibraryDynamoDB.GetNewLegalDocumentsForTitles(titles, "trans0");
            }
            return documents;
        }
    }
}
