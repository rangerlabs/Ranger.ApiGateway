using System;
using System.ComponentModel.DataAnnotations;

namespace Ranger.ApiGateway
{
    public class ProjectResponseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LiveApiKey { get; set; }
        public string TestApiKey { get; set; }
        public string ProjectApiKey { get; set; }
        public string LiveApiKeyPrefix { get; set; }
        public string TestApiKeyPrefix { get; set; }
        public string ProjectApiKeyPrefix { get; set; }
        public bool Enabled { get; set; }
        public int Version { get; set; }
    }
}