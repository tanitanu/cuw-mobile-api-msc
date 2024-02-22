using System;

namespace United.Mobile.Model.DynamoDb.Common
{
    [Serializable]
    public class MOBLegalDocument
    {     
        public string Title { get; set; } = string.Empty;
        //public string LegalDocument { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;

    }
}
