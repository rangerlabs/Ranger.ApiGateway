using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PrimaryOwnershipRefuseModel
    {
        [Required]
        public Guid CorrelationId { get; set; }
    }
}