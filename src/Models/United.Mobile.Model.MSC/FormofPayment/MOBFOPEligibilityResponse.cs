﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBFOPEligibilityResponse : MOBResponse
    {
        private string sessionId = string.Empty;
        private string flow = string.Empty;
        private bool isDefaultPaymentOption = false;
        private List<FormofPaymentOption> eligibleFormofPayments;
        private MOBShoppingCart shoppingCart;
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }
        public bool IsDefaultPaymentOption
        {
            get { return isDefaultPaymentOption; }
            set { isDefaultPaymentOption = value; }
        }

        public List<FormofPaymentOption> EligibleFormofPayments
        {
            get { return eligibleFormofPayments; }
            set { eligibleFormofPayments = value; }
        }
        public MOBShoppingCart ShoppingCart
        {
            get { return shoppingCart; }
            set { shoppingCart = value; }
        }
    }
    [Serializable()]
    public class FormofPaymentOption
    {

        private string category = string.Empty;
        private string fullName = string.Empty;
        private string code = string.Empty;
        private string fopDescription = string.Empty;
        private bool deleteOrder;

        public string Category
        {
            get { return category; }
            set { category = value; }
        }
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }
        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        public bool DeleteOrder
        {
            get { return deleteOrder; }
            set { deleteOrder = value; }
        }

        [JsonPropertyName("fopDescription")]
        [JsonProperty("fopDescription")]
        public string FoPDescription
        {
            get { return fopDescription; }
            set { fopDescription = value; }
        }

    }
}
