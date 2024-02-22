using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Common
{
    [Serializable]
    public enum MOBEmpMessageCode
    {
        None,
        SpaceAvailableBeforeDepartureGoldAndHigher,
        SpaceAvailableBeforeDepartureSilver,
        SpaceAvailableCpuToCabinOutsideWindowGoldAndHigher,
        SpaceAvailableCpuToCabinOutsideWindowSilver,
        InstantUpgrade,
        SpaceAvailableInsideWindowGoldAndHigher,
        SpaceAvailableInsideWindowSilverAndDayOfDeparture,
        WaitlistUpgradeRequested,
        ConfirmedUpgrade,
        WaitlistedSameFlightDifferentNonUpgradeClass,
        WaitlistedSegmentOnAnotherFlight,
        WaitlistClassConfirmed,
    }
}
