using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBLegalDocumentResponse : MOBResponse
    {
        private List<MOBTypeOption> legalDocuments;

        public List<MOBTypeOption> LegalDocuments
        {
            get
            {
                return this.legalDocuments;
            }
            set
            {
                this.legalDocuments = value;
            }
        }
    }
}
