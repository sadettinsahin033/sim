using Gley.UrbanSystem.Editor;

namespace Gley.PedestrianSystem.Editor
{
    public class GridSetupWindow : GridSetupWindowBase
    {
        public override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            var pedestrianLayers = FileCreator.LoadOrCreateLayers<LayerSetup>(PedestrianSystemConstants.LayerPath);
            if (pedestrianLayers != null)
            {
               AddPedestrianGround(pedestrianLayers.GroundLayers);
            }
            return this;
        }

        public override void DrawInScene()
        {
            if (_viewGrid)
            {
                _gridDrawer.DrawGrid(false);
            }
            base.DrawInScene();
        }
    }
}
