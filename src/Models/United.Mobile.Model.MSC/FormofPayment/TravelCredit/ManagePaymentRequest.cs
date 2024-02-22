using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using United.Service.Presentation.PaymentModel;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.ShoppingCart.Model.ManagePayment
{
    [Serializable]
    [DataContract]
    public class ManagePaymentRequest
    {

        [Required]
        [DataMember(EmitDefaultValue = false)]
        public string CartId { get; set; }

        [Required]
        [DataMember(EmitDefaultValue = false)]
        public WorkFlowType WorkFlowType { get; set; }

        [Required]
        [DataMember(EmitDefaultValue = false)]
        public United.Service.Presentation.PaymentModel.FormOfPayment FormOfPayment { get; set; }
        
        [DataMember(EmitDefaultValue = false)]
        public string PaymentTarget { get; set; }
    }
}
