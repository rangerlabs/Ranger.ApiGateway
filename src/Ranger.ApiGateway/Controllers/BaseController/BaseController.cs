using System;
using System.Linq;
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

        protected IActionResult Send<T>(T command, Guid? resourceId = null, string resource = "") where T : ICommand
        {
            var context = GetContext<T>(resourceId, resource);
            try
            {
                busPublisher.Send(command, context);
                return Accepted(context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An exception occurred publishing a command. Ensure the service is connect to a running RabbitMQ instance.");
                return InternalServerError();
            }
        }

        protected IActionResult Accepted(ICorrelationContext context)
        {
            Response.Headers.Add(OperationHeader, $"operations/{context.CorrelationContextId}");
            if (!string.IsNullOrWhiteSpace(context.Resource))
            {
                Response.Headers.Add(ResourceHeader, context.Resource);
            }

            return base.Accepted();
        }

        protected IActionResult InternalServerError(string errors = "")
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = errors });
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

        protected User UserFromClaims
        {
            get
            {
                return User.UserFromClaims();
            }
        }

        protected string Culture
        {
            get
            {
                return Request.Headers.ContainsKey(AcceptLanguageHeader) ? Request.Headers[AcceptLanguageHeader].First().ToLowerInvariant() : DefaultCulture;
            }
        }
    }
}