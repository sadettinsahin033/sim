using UnityEditor;

using SetupWindowBase = Gley.UrbanSystem.Editor.SetupWindowBase;
using WindowProperties = Gley.UrbanSystem.Editor.WindowProperties;
using SettingsWindowBase = Gley.UrbanSystem.Editor.SettingsWindowBase;
using SettingsLoader = Gley.UrbanSystem.Editor.SettingsLoader;

namespace Gley.PedestrianSystem.Editor
{
    public class PedestrianSetupWindow : SetupWindowBase
    {
        protected PedestrianSettingsWindowData _editorSave;

        public override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            base.Initialize(windowProperties, window);
            _editorSave = new SettingsLoader(PedestrianSystemConstants.WindowSettingsPath).LoadSettingsAsset<PedestrianSettingsWindowData>();
            return this;
        }


        public override void DestroyWindow()
        {
            EditorUtility.SetDirty(_editorSave);
            base.DestroyWindow();
        }
    }
}