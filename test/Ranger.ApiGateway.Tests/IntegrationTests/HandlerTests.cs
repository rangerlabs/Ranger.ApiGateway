using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Ranger.ApiGateway.Tests.IntegrationTests;
using Ranger.InternalHttpClient;
using Ranger.RabbitMQ.BusPublisher;
using Ranger.RabbitMQ.BusSubscriber;
using Shouldly;
using Xunit;

namespace Ranger.ApiGateway.Tests
{
    public class HandlerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly IBusPublisher busPublisher;
        private readonly IBusSubscriber busSubscriber;
        private readonly CustomWebApplicationFactory _factory;

        public HandlerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            this.busPublisher = factory.Services.GetService(typeof(IBusPublisher)) as IBusPublisher;
            this.busSubscriber = factory.Services.GetService(typeof(IBusSubscriber)) as IBusSubscriber;
        }

        [Fact]
        public async Task GetAllGeofences_Returns200_WhenNoQueryParams()
        {
            var mockClient = new Mock<IGeofencesHttpClient>();
            mockClient
                .Setup(x => x.GetGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RangerApiResponse<IEnumerable<GeofenceResponseModel>>());
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy(AuthorizationPolicyNames.BelongsToProject, policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser().AddRequirements(new BelongsToProjectRequirementStub());
                        });
                    });
                    services.AddScoped<IAuthorizationHandler, BelongsToProjectHandlerStub>();
                    services.AddTransient<IGeofencesHttpClient>((_) => mockClient.Object);
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => {});
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await client.GetAsync($"/{Guid.NewGuid()}/geofences");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.ShouldBe(HttpStatusCode.OK);

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
       }
        
        [Fact]
        public async Task GetGeofences_Returns400_WhenInvalidQueryParam()
        {
            var mockClient = new Mock<IGeofencesHttpClient>();
            mockClient
                .Setup(x => x.GetGeofencesByProjectId<IEnumerable<GeofenceResponseModel>>(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RangerApiResponse<IEnumerable<GeofenceResponseModel>>());
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy(AuthorizationPolicyNames.BelongsToProject, policyBuilder =>
                        {
                            policyBuilder.RequireAuthenticatedUser().AddRequirements(new BelongsToProjectRequirementStub());
                        });
                    });
                    services.AddScoped<IAuthorizationHandler, BelongsToProjectHandlerStub>();
                    services.AddTransient<IGeofencesHttpClient>((_) => mockClient.Object);
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                            "Test", options => {});
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
            client.DefaultRequestHeaders.Add("api-version", "1.0");

            var response = await client.GetAsync($"/{Guid.NewGuid()}/geofences?page=0");
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

            var result = JsonConvert.DeserializeObject<RangerApiResponse<IEnumerable<GeofenceResponseModel>>>(content);
       }
    }
}