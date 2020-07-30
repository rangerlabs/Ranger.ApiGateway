using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PrimaryOwnershipRefuseModel
    {
        public Guid CorrelationId { get; set; }
    }
}