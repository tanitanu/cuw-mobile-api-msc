using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.DataPower
{
    [Serializable()]
    public class DPAccessTokenRequest
    {
        private string grantType = string.Empty;
        private string clientId;
        private string clientSecret;
        private string scope;
        private string userType;
        private string endUserAgentId;
        private string endUserAgentIP;
        private string accessToken ;
        private string userName;
        private string password ;
        private string refreshToken ;
        private string rokenTypeHint ;
        private string nonce ;

        public string Nonce
        {
            get { return this.nonce; }
            set { this.nonce = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string RokenTypeHint
        {
            get { return this.rokenTypeHint; }
            set { this.rokenTypeHint = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string RefreshToken
        {
            get { return this.refreshToken; }
            set { this.refreshToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string Password
        {
            get { return this.password; }
            set { this.password = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string AccessToken
        {
            get { return this.accessToken; }
            set { this.accessToken = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string EndUserAgentIP
        {
            get { return this.endUserAgentIP; }
            set { this.endUserAgentIP = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string EndUserAgentId
        {
            get { return this.endUserAgentId; }
            set { this.endUserAgentId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string UserType
        {
            get { return this.userType; }
            set { this.userType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string Scope
        {
            get { return this.scope; }
            set { this.scope = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ClientSecret
        {
            get { return this.clientSecret; }
            set { this.clientSecret = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string ClientId 
        {
            get
            {
                return this.clientId;
            }
           set
            {
                this.clientId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string GrantType
        {
            get
            {
                return this.grantType;
            }
            set
            {
                this.grantType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}

