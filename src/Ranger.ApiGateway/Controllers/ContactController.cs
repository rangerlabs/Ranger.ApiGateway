using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ranger.RabbitMQ;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [AllowAnonymous]
    public class ContactController : BaseController<ContactController>
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<ContactController> logger;
        public ContactController(IBusPublisher busPublisher, ILogger<ContactController> logger) : base(busPublisher, logger)
        {
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        ///<summary>
        /// Accepts new contact form requests
        ///</summary>
        ///<param name="contactFormModel">The model necessary to send a contact email</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/contact")]
        public ApiResponse PostBreadcrumb([FromBody] ContactFormModel contactFormModel)
        {
            logger.LogDebug("Contact form received");
            return base.SendAndAccept(new SendContactFormEmail(contactFormModel.Organization, contactFormModel.Email, contactFormModel.Message),
                clientMessage: "Contact Form accepted"
            );
        }
    }
}