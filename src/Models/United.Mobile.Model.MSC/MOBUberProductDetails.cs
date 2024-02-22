using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBUberProductDetails
    {
        private string image;
        private string display_name;
        private string id;
        private string product_Id;
        private string description;

        public string Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Display_name
        {
            get
            {
                return this.display_name;
            }
            set
            {
                this.display_name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Id
        {
            get
            {
                return this.product_Id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Product_Id
        {
            get
            {
                return this.product_Id;
            }
            set
            {
                this.product_Id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

    }
}
