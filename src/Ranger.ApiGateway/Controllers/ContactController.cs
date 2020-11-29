using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using reCAPTCHA.AspNetCore.Attributes;
using Ranger.RabbitMQ;
using Ranger.RabbitMQ.BusPublisher;
using reCAPTCHA.AspNetCore;
using System.Threading.Tasks;

namespace Ranger.ApiGateway.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [AllowAnonymous]
    public class ContactController : BaseController
    {
        private readonly IBusPublisher busPublisher;
        private readonly ILogger<ContactController> logger;
        private readonly IRecaptchaService recaptcha;

        public ContactController(IBusPublisher busPublisher, IRecaptchaService recaptcha, ILogger<ContactController> logger) : base(busPublisher, logger)
        {
            this.recaptcha = recaptcha;
            this.logger = logger;
            this.busPublisher = busPublisher;
        }

        ///<summary>
        /// Accepts new contact form requests
        ///</summary>
        ///<param name="contactFormModel">The model necessary to send a contact email</param>
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [HttpPost("/contact")]
        public async Task<ApiResponse> PostContactForm([FromBody] ContactFormModel contactFormModel)
        {
            contactFormModel.Organization = contactFormModel.Organization.Trim();
            contactFormModel.Email = contactFormModel.Email.Trim();
            contactFormModel.Name = contactFormModel.Name.Trim();
            contactFormModel.Message = contactFormModel.Message.Trim();

            logger.LogDebug("Contact form received");
            var recaptcha = await this.recaptcha.Validate(contactFormModel.ReCaptchaToken);
            logger.LogInformation("Received reCaptcha response. {Success}, {Score}", recaptcha.success, recaptcha.score);
            if (!recaptcha.success || recaptcha.score != 0 && recaptcha.score < 0.5)
            {
                throw new ApiException("An error occurred validating the reCaptcha response. Please try again shortly.");
            }

            return base.SendAndAccept(new SendContactFormEmail(contactFormModel.Organization, contactFormModel.Email, contactFormModel.Name, contactFormModel.Message),
                clientMessage: "Contact Form accepted"
            );
        }
    }
}