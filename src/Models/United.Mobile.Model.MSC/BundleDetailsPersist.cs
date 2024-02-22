using System;
using System.Collections.Generic;
using System.Text;
using United.Definition.Shopping;

namespace United.Mobile.Model.MSC
{
    public class BundleDetailsPersist
    {
        public string Title { get; set; }
        public string ProductCode { get; set; }
        public List<BundleDescriptionPersist> BundleDescriptions { get; set; }
        public MOBMobileCMSContentMessages TermsAndCondition { get; set; }
    }
    public class BundleDescriptionPersist
    {
        public string Title { get; set; }
        public string ProductCode { get; set; }
        public string Description { get; set; }
    }
}
