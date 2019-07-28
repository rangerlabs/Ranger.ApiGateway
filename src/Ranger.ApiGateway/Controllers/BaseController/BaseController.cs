using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway {
    [Route ("[controller]")]
    [ApiController]
    public class BaseController<TController> : ControllerBase {
        private static readonly string AcceptLanguageHeader = "accept-language";
        private static readonly string OperationHeader = "X-Operation";
        private static readonly string ResourceHeader = "X-Resource";
        private static readonly string TenantDomainHeader = "X-Tenant-Domain";
        private static readonly string DefaultCulture = "en-us";
        private static readonly string PageLink = "page";
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<TController> logger;

        public BaseController (IBusPublisher busPublisher, ILogger<TController> logger) {
            this.busPublisher = busPublisher;
            this.logger = logger;
        }

        protected IActionResult Send<T> (T command, Guid? resourceId = null, string resource = "") where T : ICommand {
            var context = GetContext<T> (resourceId, resource);
            busPublisher.Send (command, context);

            return Accepted (context);
        }

        protected IActionResult Accepted (ICorrelationContext context) {
            Response.Headers.Add (OperationHeader, $"operations/{context.CorrelationContextId}");
            if (!string.IsNullOrWhiteSpace (context.Resource)) {
                Response.Headers.Add (ResourceHeader, context.Resource);
            }

            return base.Accepted ();
        }

        private ICorrelationContext GetContext<T> (Guid? resourceId, string resource) {
            if (!string.IsNullOrWhiteSpace (resource)) {
                resource = $"{resource}/{resourceId}";
            }

            return CorrelationContext.Create<T> (
                Guid.NewGuid (),
                UserId,
                resourceId ?? Guid.Empty,
                Request.Path.ToString (),
                HttpContext.TraceIdentifier,
                "",
                this.HttpContext.Connection.Id,
                Culture
            );
        }

        protected string UserId {
            get {
                return string.IsNullOrWhiteSpace (User?.Identity?.Name) ? "" : User.Identity.Name;
            }
        }

        protected string Culture {
            get {
                return Request.Headers.ContainsKey (AcceptLanguageHeader) ? Request.Headers[AcceptLanguageHeader].First ().ToLowerInvariant () : DefaultCulture;
            }
        }
    }
}