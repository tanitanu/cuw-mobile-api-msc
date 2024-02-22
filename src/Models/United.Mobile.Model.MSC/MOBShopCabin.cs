using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopCabin
    {
        private MOBShopBoardingTotal boardingTotals;
        private List<MOBShopMeal> meals;
        private string type = string.Empty;

        public MOBShopBoardingTotal BoardingTotals
        {
            get
            {
                return this.boardingTotals;
            }
            set
            {
                this.boardingTotals = value;
            }
        }

        public List<MOBShopMeal> Meals
        {
            get
            {
                return this.meals;
            }
            set
            {
                this.meals = value;
            }
        }

        public string Type
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
