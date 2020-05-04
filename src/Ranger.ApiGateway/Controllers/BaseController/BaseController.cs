using System;
using System.Linq;
using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ranger.Common;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway
{
    [Route("[controller]")]
    [ApiController]
    public class BaseController<TController> : ControllerBase
    {
        private static readonly string AcceptLanguageHeader = "accept-language";
        private static readonly string OperationHeader = "X-Operation";
        private static readonly string ResourceHeader = "X-Resource";
        private static readonly string DefaultCulture = "en-us";
        // private static readonly string PageLink = "page";
        private readonly IBusPublisher busPublisher;
        protected readonly ILogger<TController> Logger;

        public BaseController(IBusPublisher busPublisher, ILogger<TController> logger)
        {
            this.busPublisher = busPublisher;
            this.Logger = logger;
        }

        protected ApiResponse Send<T>(T command, string clientMessage = "", Guid? resourceId = null, string resource = "") where T : ICommand
        {
            var context = GetContext<T>(resourceId, resource);
            try
            {
                busPublisher.Send(command, context);
                return Accepted(context, clientMessage);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An exception occurred publishing a command. Ensure the service is connected to a running RabbitMQ instance");
                throw new ApiException("Failed to publishing command", statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        protected ApiResponse Accepted(ICorrelationContext context, string clientMessage = "")
        {
            Response.Headers.Add(OperationHeader, $"operations/{context.CorrelationContextId}");
            if (!string.IsNullOrWhiteSpace(context.Resource))
            {
                Response.Headers.Add(ResourceHeader, context.Resource);
            }

            return new ApiResponse(clientMessage, statusCode: StatusCodes.Status202Accepted);
        }

        private ICorrelationContext GetContext<T>(Guid? resourceId, string resource)
        {
            if (!string.IsNullOrWhiteSpace(resource))
            {
                resource = $"{resource}/{resourceId}";
            }

            return CorrelationContext.Create<T>(
                Guid.NewGuid(),
                UserFromClaims?.Domain ?? "",
                UserFromClaims?.Email ?? "",
                resourceId ?? Guid.Empty,
                Request.Path.ToString(),
                HttpContext.TraceIdentifier,
                "",
                this.HttpContext.Connection.Id,
                Culture
            );
        }

        protected User UserFromClaims => User.UserFromClaims();
        protected string Culture => Request.Headers.ContainsKey(AcceptLanguageHeader) ? Request.Headers[AcceptLanguageHeader].First().ToLowerInvariant() : DefaultCulture;
        protected string TenantId => HttpContext.Items["TenantId"] as string;
    }
}