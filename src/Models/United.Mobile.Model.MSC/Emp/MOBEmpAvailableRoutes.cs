using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpAvailableRoutes
    {
        private List<MOBEmpRoute> routes;

        public List<MOBEmpRoute> Routes
        {
            get { return this.routes; }
            set { this.routes = value; }
        }

    }
}
