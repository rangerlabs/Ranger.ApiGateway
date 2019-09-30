using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiGateway;

namespace Ranger.ApiGateway
{

    [ApiController]
    [Authorize(Roles = "User")]
    public class WebhookIntegrationController : IntegrationBaseController<WebhookIntegrationResponseModel>
    {
        [HttpGet("/integration/webhook")]
        public async Task<IActionResult> Index(string name)
        {

            return Ok();
        }

        [HttpGet("/integration/webhook/all")]
        public async Task<IActionResult> All(string email)
        {
            return Ok();
        }

        [HttpPost("{projectName}/integration/webhook")]
        public async Task<IActionResult> Post([FromRoute]string projectName, [FromBody]WebhookIntegrationModel webhookIntegrationModel)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                throw new ArgumentException("message", nameof(projectName));
            }

            if (webhookIntegrationModel == null)
            {
                throw new ArgumentNullException(nameof(webhookIntegrationModel));
            }
            var apiIntegrationResponseModel = new WebhookIntegrationResponseModel()
            {
                ProjectName = projectName,
                Name = webhookIntegrationModel.Name,
                Description = webhookIntegrationModel.Description,
                URL = webhookIntegrationModel.URL,
                AuthKey = webhookIntegrationModel.AuthKey
            };
            return Created("/integration/webhook", apiIntegrationResponseModel);
        }
    }
}