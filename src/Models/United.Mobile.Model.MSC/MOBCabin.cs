using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBCabin
    {
        public MOBCabin() { }
        public MOBCabin(List<MOBRow> rows, string cos)
        {
            Rows = rows;
            COS = cos;
        }

        private List<MOBRow> rows = new List<MOBRow>();
        public List<MOBRow> Rows
        {
            get { return rows; }
            set { rows = value; }
        }

        private string cos = string.Empty;
        public string COS
        {
            get { return cos; }
            set { cos = value; }
        }

        private string configuration;
        public string Configuration
        {
            get { return configuration; }
            set { configuration = value; }
        }


    }
}
