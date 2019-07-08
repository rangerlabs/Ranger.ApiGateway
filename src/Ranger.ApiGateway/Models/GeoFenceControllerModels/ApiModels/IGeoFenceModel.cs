namespace Ranger.ApiGateway {
    public interface IGeoFenceModel {
        string Name { get; set; }
        string Description { get; set; }
        GeoFenceShapeEnum Shape { get; }
        bool OnEnter { get; set; }
        bool OnExit { get; set; }

    }
}