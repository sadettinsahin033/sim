namespace Gley.PedestrianSystem.Editor
{
    internal class PedestrianLaneEditorData : UrbanSystem.Editor.LaneEditorData<PedestrianPath, PedestrianWaypointSettings>
    {
        internal PedestrianLaneEditorData(UrbanSystem.Editor.RoadEditorData<PedestrianPath> roadData) : base(roadData)
        {
        }
    }
}