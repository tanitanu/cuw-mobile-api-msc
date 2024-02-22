using System;


namespace United.Definition.CSLModels.CustomerProfile
{
    
    public class CommonData
    {
        /// <summary>
        /// Start date time of a service
        /// </summary>
       
        public string StartTime { get; set; } = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss:FFF");

        /// <summary>
        /// End date time of a service.
        /// </summary>
     
        public string EndTime { get; set; }
    }
}
