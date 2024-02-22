using System;
using System.Runtime.Serialization;

namespace United.Definition.FFC
{
    [Serializable]
    public class PaxDetail
    {
        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        public virtual string Email { get; set; }

        public virtual string Phone { get; set; }

        public virtual string ZipCode { get; set; }
        public int PassengerIndex { get; set; }
        public string DateOfBirth { get; set; }
    }
}
