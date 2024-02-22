using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBLegalDocument
    {
        private string title = string.Empty;
        private string document = string.Empty;
        public string LegalDocument { get; set; } = string.Empty;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Document
        {
            get
            {
                return this.document;
            }
            set
            {
                this.document = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
