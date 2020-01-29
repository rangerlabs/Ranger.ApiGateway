using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.ApiUtilities;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    public class OperationsController : BaseController<OperationsController>
    {
        private readonly IOperationsClient operationsClient;
        private readonly ILogger<OperationsController> logger;

        public OperationsController(IBusPublisher busPublisher, IOperationsClient operationsClient, ILogger<OperationsController> logger) : base(busPublisher, logger)
        {
            this.operationsClient = operationsClient;
            this.logger = logger;
        }

        [HttpGet("/operations/{id}")]
        public async Task<IActionResult> GetOperationState([FromRoute] Guid id)
        {
            try
            {
                return Ok(await this.operationsClient.GetOperationStateById<OperationStateResponseModel>(Domain, id.ToString()));
            }
            catch (HttpClientException<OperationStateResponseModel> ex)
            {
                if ((int)ex.ApiResponse.StatusCode == StatusCodes.Status404NotFound)
                {
                    return NotFound();
                }
                return InternalServerError();
            }
        }
    }
}