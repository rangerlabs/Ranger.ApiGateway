using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ApplicationApiResponseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ApiKey { get; set; }
    }
}