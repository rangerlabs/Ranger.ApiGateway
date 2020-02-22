using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class CancelPrimaryOwnershipModel
    {
        [Required]
        public Guid CorrelationId { get; set; }
    }
}