using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Gamification
{
    public class MOBInBoxMessageRequest<T>
    {
        public string DeviceID { get; set; }
        public T Data { get; set; }
        public MOBApplication Application {get; set; }
        public string LanguageCode { get; set; }
        public string AccessCode { get; set; }
        public string TransactionId { get; set; }      

    }
}
