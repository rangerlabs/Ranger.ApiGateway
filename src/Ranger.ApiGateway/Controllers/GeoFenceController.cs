using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ranger.ApiGateway;
using Ranger.ApiUtilities;

namespace Ranger.ApiGateway {
    [ApiController]
    [Authorize (Roles = "User")]
    [TenantDomainRequired]
    public class GeoFenceController : ControllerBase {
        [HttpGet ("/geofence/{name}")]
        public async Task<IActionResult> Index (string name) {
            IActionResult response = new StatusCodeResult (200);
            var geoFenceModel = new CircularGeoFenceApiResponseModel () {
                Name = "Test Geo Fence",
                Description = "Test Geo Fence Description",
                Center = new LatLng (
                41.505493, -81.681290),
                Radius = 100,
                OnEnter = true,
                OnExit = true
            };
            return Ok (geoFenceModel);
        }

        [HttpGet ("/geofence/all")]
        public async Task<IActionResult> All (string name) {
            IActionResult response = new StatusCodeResult (200);
            var geoFenceResponseCollection = new List<IGeoFenceApiResponseModel> ();
            var polygonPoints = new List<LatLng> ();
            polygonPoints.Add (new LatLng (
                40.74530871346847, -74.01628833886718));
            polygonPoints.Add (new LatLng (
                40.75363163725995, -73.95552021142578));
            polygonPoints.Add (new LatLng (
                40.77490733099531, -73.98580335067368));

            var geoFenceModel = new PolygonGeoFenceApiResponseModel () {
                AppName = "TestApp",
                Name = "Hoboken",
                Integrations = new List<string> () { "API Integration" },
                Description = "Test Geo Fence Description",
                LatLngPath = polygonPoints,
                OnEnter = true,
                OnExit = true
            };
            var geoFenceModel2 = new CircularGeoFenceApiResponseModel () {
                AppName = "TestApp",
                Name = "Manhattan Circle",
                Integrations = new List<string> () { "API Integration" },
                Description = "Test Geo Fence 2 Description",
                Center = new LatLng (
                40.7745457, -73.9718052),
                Radius = 100,
                OnEnter = true,
                OnExit = true
            };
            var geoFenceModel3 = new CircularGeoFenceApiResponseModel () {
                AppName = "TestApp2",
                Name = "Hoboken Circle",
                Integrations = new List<string> () { "API Integration 2" },
                Description = "Test Geo Fence 2 Description",
                Center = new LatLng (
                40.7426017, -74.0284736),
                Radius = 100,
                OnEnter = true,
                OnExit = true
            };
            geoFenceResponseCollection.Add (geoFenceModel);
            geoFenceResponseCollection.Add (geoFenceModel2);
            geoFenceResponseCollection.Add (geoFenceModel3);
            return Ok (geoFenceResponseCollection);
        }

        [HttpPost ("/geofence/circle")]
        public async Task<IActionResult> Post (CircularGeoFenceModel geoFenceModel) {
            if (geoFenceModel == null) {
                throw new ArgumentNullException (nameof (geoFenceModel));
            }
            var applicationApiResponseModel = new CircularGeoFenceApiResponseModel () {
                Name = geoFenceModel.Name,
                Center = geoFenceModel.Center,
                Radius = geoFenceModel.Radius,
                OnEnter = geoFenceModel.OnEnter,
                OnExit = geoFenceModel.OnExit
            };
            return Created ("/geofence", geoFenceModel);
        }

        [HttpPost ("/geofence/polygon")]
        public async Task<IActionResult> Post (PolygonGeoFenceModel geoFenceModel) {
            if (geoFenceModel == null) {
                throw new ArgumentNullException (nameof (geoFenceModel));
            }
            var applicationApiResponseModel = new PolygonGeoFenceApiResponseModel () {
                Name = geoFenceModel.Name,

                LatLngPath = geoFenceModel.LatLngPath,
                OnEnter = geoFenceModel.OnEnter,
                OnExit = geoFenceModel.OnExit
            };
            return Created ("/geofence", geoFenceModel);
        }

    }
}