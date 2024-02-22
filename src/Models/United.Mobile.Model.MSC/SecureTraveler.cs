

using System;

namespace United.Definition.CSLModels.CustomerProfile
{
   
    public class SecureTraveler : SecureTravelerSharedMembers
    {
        
       
        public long CustomerId
        {
            get;
            set;
        }

       
      
      
        public string Description
        {
            get;
            set;
        }

       
      
      
        public string Suffix
        {
            get;
            set;
        }

       
      
      
        public string InsertId
        {
            get;
            set;
        }

       
      
        public DateTime? InsertDtmz
        {
            get;
            set;
        }

       
      
      
        public string UpdateId
        {
            get;
            set;
        }

       
      
        public DateTime? UpdateDtmz
        {
            get;
            set;
        }
    }
}

