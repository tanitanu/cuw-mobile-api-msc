using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPAffinitySearch
    {
        private MOBSHOPResultsSolution[] solution;

        private int count;

        private string id;

        private string version;

        private string type;

        public MOBSHOPResultsSolution[] Solution
        {
            get
            {
                return this.solution;
            }
            set
            {
                this.solution = value;
            }
        }

        public int Count
        {
            get
            {
                return this.count;
            }
            set
            {
                this.count = value;
            }
        }

        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }
    }

    public class MOBSHOPResultsSolution
    {
        private decimal price;

        private string currency;

        private string from;

        private string to;

        private System.DateTime departs;

        private System.DateTime returns;

        public decimal Price
        {
            get
            {
                return this.price;
            }
            set
            {
                this.price = value;
            }
        }

       public string Currency
        {
            get
            {
                return this.currency;
            }
            set
            {
                this.currency = value;
            }
        }

        public string From
        {
            get
            {
                return this.from;
            }
            set
            {
                this.from = value;
            }
        }

        public string To
        {
            get
            {
                return this.to;
            }
            set
            {
                this.to = value;
            }
        }

        public System.DateTime Departs
        {
            get
            {
                return this.departs;
            }
            set
            {
                this.departs = value;
            }
        }

        public System.DateTime Returns
        {
            get
            {
                return this.returns;
            }
            set
            {
                this.returns = value;
            }
        }
    }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class results
        {

            private resultsSolution[] solutionField;

            private int countField;

            private string idField;

            private string versionField;

            private string typeField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute("solution")]
            public resultsSolution[] solution
            {
                get
                {
                    return this.solutionField;
                }
                set
                {
                    this.solutionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public int count
            {
                get
                {
                    return this.countField;
                }
                set
                {
                    this.countField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string id
            {
                get
                {
                    return this.idField;
                }
                set
                {
                    this.idField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string version
            {
                get
                {
                    return this.versionField;
                }
                set
                {
                    this.versionField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }

            //method added to force sample data due to dev unavailability
            public static results getSampleResults()
            {
                results ret = new results();
                resultsSolution[] resultsSoln = new resultsSolution[20];
                ret.count = resultsSoln.Count();
                ret.type = "prices";
                ret.id = new Guid().ToString();
                ret.version = "ITA.IS.API/vSample";

                for (int i = 0; i < ret.count; i++)
                {
                    resultsSoln[i] = new resultsSolution();
                    resultsSoln[i].currency = "USD";
                    resultsSoln[i].departs = DateTime.Now.AddDays(1);
                    resultsSoln[i].returns = DateTime.Now.AddDays(1+i);
                    resultsSoln[i].from = "IAH";
                    resultsSoln[i].to = "LAS";
                    resultsSoln[i].price = 999.99M;
                }
                ret.solution = resultsSoln;

                return ret;
            }

        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class resultsSolution
        {

            private decimal priceField;

            private string currencyField;

            private string fromField;

            private string toField;

            private System.DateTime departsField;

            private System.DateTime returnsField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public decimal price
            {
                get
                {
                    return this.priceField;
                }
                set
                {
                    this.priceField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string currency
            {
                get
                {
                    return this.currencyField;
                }
                set
                {
                    this.currencyField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string from
            {
                get
                {
                    return this.fromField;
                }
                set
                {
                    this.fromField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string to
            {
                get
                {
                    return this.toField;
                }
                set
                {
                    this.toField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime departs
            {
                get
                {
                    return this.departsField;
                }
                set
                {
                    this.departsField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute(DataType = "date")]
            public System.DateTime returns
            {
                get
                {
                    return this.returnsField;
                }
                set
                {
                    this.returnsField = value;
                }
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class error
        {
            private string typeField;

            private string reasonField;

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string type
            {
                get
                {
                    return this.typeField;
                }
                set
                {
                    this.typeField = value;
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlAttributeAttribute()]
            public string reason
            {
                get
                {
                    return this.reasonField;
                }
                set
                {
                    this.reasonField = value;
                }
            }
        }
    //}
}
