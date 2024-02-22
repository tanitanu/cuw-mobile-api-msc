using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Kochava
{
    [Serializable()]
    public class MOBEvent_Data
    {
        private string id;
        private string name;
        private int sum;

        /// <summary>
        /// Origination IP
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// app ver
        /// </summary>       
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        ///Data
        /// </summary>        
        public int Sum
        {
            get { return sum; }
            set { sum = value; }
        }
    }
}

