using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.Common;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class DocumentLibraryDynamoDB
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly string _tableName;
        private readonly string _transactionId = "legalDocument01";
        public DocumentLibraryDynamoDB(IConfiguration configuration, IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
            _tableName = _configuration.GetValue<string>("DynamoDBTables:uatb_documentlibrary");

        }

        public async Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(List<string> titles, string sessionId)
        {
            try
            {
                var returnValue = new List<MOBLegalDocument>();
                foreach (var title in titles)
                {
                    var responseLegalDocument = await _dynamoDBService.GetRecords<List<MOBLegalDocument>>(_tableName, _transactionId, title, sessionId);
                    if (responseLegalDocument != null)
                        returnValue.AddRange(responseLegalDocument);
                }
                return returnValue;
            }
            catch
            {

            }

            return default;
        }
    }
}
