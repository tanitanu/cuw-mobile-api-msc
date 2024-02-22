using System;
using System.Collections.Generic;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBSHOPChasePromoRedirectResponse: MOBResponse
    {
        private string webSessionShareURL;
        private string redirectURL;
        private string returnURL;
        private List<string> returnURLs;
        private string token;


        public string WebSessionShareUrl
        {
            get { return webSessionShareURL; }
            set { webSessionShareURL = value; }
        }

        public string RedirectUrl
        {
            get { return redirectURL; }
            set { redirectURL = value; }
        }

        public string ReturnUrl
        {
            get { return returnURL; }
            set { returnURL = value; }
        }

        public List<string> ReturnURLs
        {
            get { return returnURLs; }
            set { returnURLs = value; }
        }

        public string Token
        {
            get { return token; }
            set { token = value; }
        }
    }
}