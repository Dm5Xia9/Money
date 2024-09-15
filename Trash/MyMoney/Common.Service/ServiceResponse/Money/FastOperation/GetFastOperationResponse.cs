﻿using Common.Enums;
using System;
using System.Runtime.Serialization;

namespace ServiceRespone.Money
{
    [DataContract]
    public class GetFastOperationResponse
    {
        [DataMember]
        public FastOperationValue FastOperation { get; set; }

        [DataContract]
        public class FastOperationValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public int? CategoryId { get; set; }

            [DataMember]
            public decimal Sum { get; set; }

            [DataMember]
            public PaymentTypes PaymentType { get; set; }

            [DataMember]
            public string Place { get; set; }

            [DataMember]
            public string Comment { get; set; }

            [DataMember]
            public int? Order { get; set; }
        }
    }
}
