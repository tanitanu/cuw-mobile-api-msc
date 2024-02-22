using Newtonsoft.Json;

namespace United.Mobile.Model.Common
{
    public class DPTokenRequest
    {
        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }
        [JsonProperty(PropertyName = "client_secret")]
        public string ClientSecret { get; set; }
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }
        [JsonProperty(PropertyName = "userType")]
        public string UserType { get; set; }
        [JsonProperty(PropertyName = "grant_type")]
        public string GrantType { get; set; }
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }
}
