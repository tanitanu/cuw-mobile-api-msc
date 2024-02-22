using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Fitbit
{
    [Serializable]
    public class Barcode
    {
        private string barcodeType = string.Empty;
        private string base64EncodedImage = string.Empty;
        private string height;
        private string width;

        public string BarcodeType
        {
            get
            {
                return barcodeType;
            }
            set
            {
                barcodeType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Base64EncodedImage
        {
            get
            {
                return this.base64EncodedImage;
            }
            set
            {
                this.base64EncodedImage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Height
        {
            get
            {
                return this.height;
            }
            set
            {
                this.height = value;
            }
        }

        public string Width
        {
            get
            {
                return this.width;
            }
            set
            {
                this.width = value;
            }
        }
    }
}
