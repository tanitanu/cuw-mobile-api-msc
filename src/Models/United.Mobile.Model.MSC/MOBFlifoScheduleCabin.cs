using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBFlifoScheduleCabin
    {
        public string BoardingTotals = string.Empty;

        public MOBFlifoScheduleCabinMeal[] Meals;

        public string Type = string.Empty;

        ///// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        //public string BoardingTotals
        //{
        //    get
        //    {
        //        return this.boardingTotals;
        //    }
        //    set
        //    {
        //        this.boardingTotals = value;
        //    }
        //}

        ///// <remarks/>
        //[System.Xml.Serialization.XmlArrayItemAttribute("Meal", IsNullable = false)]
        //public MOBFlifoScheduleCabinMeal[] Meals
        //{
        //    get
        //    {
        //        return this.meals;
        //    }
        //    set
        //    {
        //        this.meals = value;
        //    }
        //}

        ///// <remarks/>
        //public string Type
        //{
        //    get
        //    {
        //        return this.type;
        //    }
        //    set
        //    {
        //        this.type = value;
        //    }
        //}

    }
}
