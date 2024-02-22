using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBFOPTravelerCertificateRequest : MOBShoppingRequest
    {
		private MOBFOPCertificate certificate;
		private bool isRemove;

		private List<MOBFOPCertificate> combineCertificates;
		public List<MOBFOPCertificate> CombineCertificates
		{
			get { return combineCertificates; }
			set { combineCertificates = value; }
		}

		public bool IsRemove
		{
			get { return isRemove; }
			set { isRemove = value; }
		}

		public MOBFOPCertificate Certificate
		{
			get { return certificate; }
			set { certificate = value; }
		}

	}
}
