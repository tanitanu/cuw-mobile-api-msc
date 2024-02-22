using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using United.Services.FlightShopping.Common;

namespace United.ShoppingCart.Model.ManagePayment
{
    [Serializable]
    [DataContract]
    public class ManagePaymentResponse
    {
        [DataMember(EmitDefaultValue = false)]
        public List<PaymentProduct> Products { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual string CartId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual string Pos { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual List<ErrorInfo> Errors { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual StatusType Status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual CreditDetails CreditDetails { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public virtual List<PaymentsApplied> PaymentsApplied { get; set; }
    }

    [Serializable]
    [DataContract]
    public class PaymentsApplied
    {
        [DataMember(EmitDefaultValue = false)]
        public string Type { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Id { get; set; }

    }

    [Serializable]
    [DataContract]
    public class PaymentProduct
    {
        [DataMember(EmitDefaultValue = false)]
        public string ProductCode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Total Totals { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public double RemainingDue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<PriceDetail> PriceDetails { get; set; }
    }

    [Serializable]
    [DataContract]
    public class Total
    {
        [DataMember(EmitDefaultValue = true)]
        public double TotalAmount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AmountCurrencyCode { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public double TotalMiles { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public MilesMoney MilesMoney { get; set; }
    }

    [Serializable]
    [DataContract]
    public class MilesMoney
    {
        [DataMember(EmitDefaultValue = true)]
        public double? Miles { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double? MilesMoneyEquivalentAmt { get; set; }
    }

    [Serializable]
    [DataContract]
    public class PriceDetail
    {
        [DataMember(EmitDefaultValue = true)]
        public double Amount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string TravelerIndex { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public string PaxTypeCode { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public double RemainingDue { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public string DateOfBirth { get; set; }
    }



    [Serializable]
    [DataContract]
    public class AppliedPaymentInfo
    {
        [DataMember(EmitDefaultValue = true)]
        public double CurrentValue { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public double InitialValue { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PaymentId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Pin { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Promo { get; set; }
    }

    [Serializable]
    [DataContract]
    public class CreditDetails
    {
        [DataMember(EmitDefaultValue = false)]
        public bool IsEtcWillCombine { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool IsFfcWillCombine { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<AppliedPaymentInfo> AppliedPaymentInfos { get; set; }
    }
}
