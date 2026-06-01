namespace Gley.PedestrianSystem.Editor
{
    public class PedestrianPathEditorData : UrbanSystem.Editor.RoadEditorData<PedestrianPath>
    {
        public override PedestrianPath[] GetAllRoads()
        {
            return _allRoads;
        }
    }
}
