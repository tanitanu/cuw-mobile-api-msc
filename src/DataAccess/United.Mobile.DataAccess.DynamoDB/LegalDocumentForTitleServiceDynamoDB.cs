using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.Product.Interfaces;

namespace United.Mobile.DataAccess.DynamoDB
{
    public class LegalDocumentForTitleServiceDynamoDB : ILegalDocumentsForTitlesService
    {
        private readonly IConfiguration _configuration;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly string _tableName;

        public LegalDocumentForTitleServiceDynamoDB(IConfiguration configuration, IDynamoDBService dynamoDBService)
        {
            _configuration = configuration;
            _dynamoDBService = dynamoDBService;
            _tableName = _configuration.GetValue<string>("DynamoDBTables:uatb_documentlibrary");
        }

        public Task<List<MOBLegalDocument>> GetLegalDocumentsForTitles(string titles, string transactionId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(string docTitles, string transactionId, bool isTermsnConditions)
        {
            if (isTermsnConditions)
            {
                return await _dynamoDBService.GetLegalDocumentsByTitle<List<MOBLegalDocument>>(_tableName, docTitles, string.IsNullOrEmpty(transactionId) ?"trans01":transactionId);   //passing default transactionid if it is empty as it mandatory field for dynamo service call             
            }
            else
            {
                var title = docTitles.Replace("'", "");
                return await _dynamoDBService.GetRecords<List<MOBLegalDocument>>(_tableName, title.Split(','), string.IsNullOrEmpty(transactionId) ? "trans01" : transactionId);
            }
        }
    }
}
