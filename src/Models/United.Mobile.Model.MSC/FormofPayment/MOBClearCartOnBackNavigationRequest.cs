namespace United.Definition
{
    public class MOBClearCartOnBackNavigationRequest : MOBShoppingRequest
    {
        private string clearOption;
        public string ClearOption
        {
            get { return clearOption; }
            set { clearOption = value; }   
        }
        private bool isBeforeRTI;
        public bool IsBeforeRTI
        {
            get { return isBeforeRTI; }
            set { isBeforeRTI = value; }
        }

    }

    public enum CartClearOption
    {
        ClearBundles,
        ClearSeats,
        ClearTPI
    }
}
