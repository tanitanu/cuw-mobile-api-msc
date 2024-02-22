using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{

    [Serializable]
    public class MOBSHOPSegmentInfoAlerts
    {
        private string alertMessage;
        private bool alignLeft;
        private bool alignRight;
        private string alertType;
        private string sortOrder;
        private string visibility;

        public string AlertMessage
        {
            get { return alertMessage; }
            set { alertMessage = value; }
        }
        public bool AlignLeft
        {
            get { return alignLeft; }
            set { alignLeft = value; }
        }
        public bool AlignRight
        {
            get { return alignRight; }
            set { alignRight = value; }
        }
        public string AlertType
        {
            get { return alertType; }
            set { alertType = value; }
        }
        public string SortOrder
        {
            get { return sortOrder; }
            set { sortOrder = value; }
        }
        public string Visibility
        {
            get { return visibility; }
            set { visibility = value; }
        }
    }

    public enum MOBSHOPSegmentInfoAlertsOrder
    {
        ArrivesNextDay,
        TerminalChange,
        RedEyeFlight,
        LongLayover,
        RiskyConnection,
        GovAuthority,
        AirportChange,
        TicketsLeft
    }

    public enum MOBSHOPSegmentInfoDisplay
    {
        FSRCollapsed, // Display only at flgiht block level
        FSRExpanded, // Display only at Flight Details level
        FSRAll // Display in both Flight block and details level
    }
}
