using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class PrimaryOwnershipAcceptModel
    {
        [Required]
        public Guid CorrelationId { get; set; }
        [Required]
        public string Token { get; set; }
    }
}