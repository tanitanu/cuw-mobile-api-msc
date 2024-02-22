using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBDOTBaggageFAQ
    {
        private string question = string.Empty;
        private string answer = string.Empty;

        public string Question
        {
            get { return this.question; }
            set { this.question = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string Answer
        {
            get { return this.answer; }
            set { this.answer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
