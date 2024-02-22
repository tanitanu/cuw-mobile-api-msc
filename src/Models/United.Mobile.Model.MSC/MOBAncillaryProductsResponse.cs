using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBAncillaryProductsResponse : MOBResponse
    {
        private MOBPlacePass placePass;

        public MOBPlacePass PlacePass
        {
            get { return placePass; }
            set { this.placePass = value; }
        }
    }

}