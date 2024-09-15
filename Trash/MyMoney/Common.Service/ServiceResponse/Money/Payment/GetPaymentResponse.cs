﻿using System;
using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRespone.Money
{
    [DataContract]
    public class GetPaymentResponse
    {
        [DataMember]
        public PaymentValue Payment { get; set; }

        [DataContract]
        public class PaymentValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public int? CategoryId { get; set; }

            [DataMember]
            public decimal Sum { get; set; }

            [DataMember]
            public string Comment { get; set; }

            [DataMember]
            public double Date { get; set; }

            [DataMember]
            public PaymentTypes PaymentType { get; set; }

            [DataMember]
            public int? CreatedTaskId { get; set; }

            [DataMember]
            public string Place { get; set; }
        }
    }
}
