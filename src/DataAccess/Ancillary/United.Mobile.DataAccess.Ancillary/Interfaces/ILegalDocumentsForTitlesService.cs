using System.Collections.Generic;
using System.Threading.Tasks;
using United.Definition;

namespace United.Mobile.DataAccess.Product.Interfaces
{
    public interface ILegalDocumentsForTitlesService
    {
        Task<List<MOBLegalDocument>> GetLegalDocumentsForTitles(string titles, string transactionId);
        Task<List<MOBLegalDocument>> GetNewLegalDocumentsForTitles(string titles, string transactionId, bool isTermsnConditions);
    }
}
