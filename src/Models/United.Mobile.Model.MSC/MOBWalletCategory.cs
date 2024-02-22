using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBWalletCategory
    {
        private int categoryId;
        private string categoryName = string.Empty;
        private List<MOBWalletItem> walletItems;

        public MOBWalletCategory()
        {
        }

        public MOBWalletCategory(int categoryId, string categoryName)
        {
            this.categoryId = categoryId;
            this.categoryName = categoryName;
        }

        public int CategoryId
        {
            get
            {
                return this.categoryId;
            }
            set
            {
                this.categoryId = value;
            }
        }

        public string CategoryName
        {
            get
            {
                return this.categoryName;
            }
            set
            {
                this.categoryName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBWalletItem> WalletItems
        {
            get
            {
                return this.walletItems;
            }
            set
            {
                this.walletItems = value;
            }
        }
    }
}
