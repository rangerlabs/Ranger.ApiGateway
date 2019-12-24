using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiUtilities;

namespace Ranger.ApiGateway
{
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize(Roles = "User")]
    [TenantDomainRequired]
    public class GeofenceController : ControllerBase
    {
        [HttpGet("{projectName}/geofences/{name}")]
        public async Task<IActionResult> Index(string projectName, string name)
        {
            return Ok();
        }

        [HttpGet("{projectName}/geofences")]
        public async Task<IActionResult> All(string projectName)
        {
            IActionResult response = new StatusCodeResult(200);
            var geoFenceResponseCollection = new List<GeofenceResponseModel>();
            var polygonPoints = new List<LatLng>();
            polygonPoints.Add(new LatLng(
                40.74530871346847, -74.01628833886718));
            polygonPoints.Add(new LatLng(
                40.75363163725995, -73.95552021142578));
            polygonPoints.Add(new LatLng(
                40.77490733099531, -73.98580335067368));

            var geoFenceModel = new GeofenceResponseModel()
            {
                ProjectName = "APP_ID_1",
                Name = "Hoboken",
                Labels = new List<string> { "Polygon" },
                OnEnter = true,
                OnExit = true,
                Enabled = true,
                Description = "Test Geo Fence Description",
                IntegrationIds = new List<string> { "GUID1" },
                Coordinates = polygonPoints,
                Metadata = new Dictionary<string, object>(),
                Shape = Enum.GetName(typeof(GeofenceShapeEnum), GeofenceShapeEnum.Polygon),
            };
            var geoFenceModel2 = new GeofenceResponseModel()
            {
                ProjectName = "APP_ID_2",
                Name = "Manhattan Circle",
                Labels = new List<string> { "NYC" },
                OnEnter = true,
                OnExit = true,
                Enabled = true,
                Description = "Test Geo Fence 2 Description",
                IntegrationIds = new List<string> { "GUID1", "GUID2" },
                Coordinates = new List<LatLng>{new LatLng(
                40.7745457, -73.9718052)},
                Metadata = new Dictionary<string, object>(),
                Radius = 100,
                Shape = Enum.GetName(typeof(GeofenceShapeEnum), GeofenceShapeEnum.Circle),
            };
            var geoFenceModel3 = new GeofenceResponseModel()
            {
                ProjectName = "APP_ID_2",
                Name = "Hoboken Circle",
                Labels = new List<string> { "Hoboken", "NYC" },
                OnEnter = true,
                OnExit = true,
                Enabled = true,
                Description = "Test Geo Fence 2 Description",
                IntegrationIds = new List<string> { "GUID3" },
                Coordinates = new List<LatLng> { new LatLng(40.7426017, -74.0284736) },
                Metadata = new Dictionary<string, object>(),
                Radius = 100,
                Shape = Enum.GetName(typeof(GeofenceShapeEnum), GeofenceShapeEnum.Circle),
            };
            geoFenceResponseCollection.Add(geoFenceModel);
            geoFenceResponseCollection.Add(geoFenceModel2);
            geoFenceResponseCollection.Add(geoFenceModel3);
            return Ok(geoFenceResponseCollection);
        }

        [HttpPost("/{projectName}/geofences/circle")]
        public async Task<IActionResult> Post([FromRoute]string projectName, [FromBody]GeofenceModel geoFenceModel)
        {
            var applicationApiResponseModel = new GeofenceResponseModel()
            {
                ProjectName = projectName,
                Name = geoFenceModel.Name,
                Labels = geoFenceModel.Labels,
                OnEnter = geoFenceModel.OnEnter,
                OnExit = geoFenceModel.OnExit,
                Enabled = geoFenceModel.Enabled,
                Description = geoFenceModel.Description,
                IntegrationIds = geoFenceModel.IntegrationIds,
                Coordinates = geoFenceModel.Coordinates,
                Metadata = geoFenceModel.Metadata,
                Radius = geoFenceModel.Radius,
            };
            return Created($"{projectName}/geofence", geoFenceModel);
        }
    }
}