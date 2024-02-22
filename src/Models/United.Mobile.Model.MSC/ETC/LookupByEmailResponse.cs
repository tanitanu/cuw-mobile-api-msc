using System;
using System.Collections.Generic;

namespace United.Definition.ETC
{
    [Serializable()]
    public class LookupByEmailResponse
    {
        public bool Success { get; set; }
        public int Status { get; set; }
        public string Body { get; set; }
        public object Results { get; set; }
        public object OtherResults { get; set; }
        public object Page { get; set; }
        public object PageLayout { get; set; }
        public object PageMetadata { get; set; }
        public object PageData { get; set; }
        public object Navigation { get; set; }
        public object ComponentPresentations { get; set; }
        public List<ErrorList> ErrorList { get; set; }
        public string LastCallDateTime { get; set; }
        public string CallTime { get; set; }
        public object SessionId { get; set; }
        public string Message { get; set; }
    }

    public class ErrorList
    {
        public int Code { get; set; }
        public object Message { get; set; }
        public object FieldName { get; set; }
    }
}
