using SetupWindowBase = Gley.UrbanSystem.Editor.SetupWindowBase;
using WindowProperties = Gley.UrbanSystem.Editor.WindowProperties;
using SettingsWindowBase = Gley.UrbanSystem.Editor.SettingsWindowBase;

namespace Gley.PedestrianSystem.Editor
{
    public class ShowAllWaypoints : ShowWaypointsWindow
    {
        public override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            return this;
        }


        public override void DrawInScene()
        {
            _pedestrianWaypointDrawer.ShowAllWaypoints(_editorSave.EditorColors.WaypointColor, _editorSave.ShowConnections, _editorSave.ShowVehicles, _editorSave.EditorColors.AgentColor, _editorSave.ShowPriority, _editorSave.EditorColors.PriorityColor);
            base.DrawInScene();
        }
    }
}