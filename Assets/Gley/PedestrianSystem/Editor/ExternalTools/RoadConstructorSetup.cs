using Gley.UrbanSystem;
using Gley.UrbanSystem.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if GLEY_PEDESTRIAN_SYSTEM
using PedestrianTypes = Gley.PedestrianSystem.User.PedestrianTypes;
#else
using PedestrianTypes = Gley.PedestrianSystem.PedestrianTypes;
#endif

namespace Gley.PedestrianSystem.Editor
{
    internal class RoadConstructorSetup : SetupWindowBase
    {

        protected override void TopPart()
        {
            base.TopPart();
#if GLEY_ROADCONSTRUCTOR_TRAFFIC
            if (GUILayout.Button("Disable Road Constructor"))
            {
                Gley.Common.Editor.PreprocessorDirective.AddToCurrent(UrbanSystemConstants.GLEY_ROADCONSTRUCTOR_TRAFFIC, true);
            }
#else
            if (GUILayout.Button("Enable Road Constructor Support"))
            {
                Gley.Common.Editor.PreprocessorDirective.AddToCurrent(UrbanSystemConstants.GLEY_ROADCONSTRUCTOR_TRAFFIC, false);
            }
#endif
            EditorGUILayout.Space();
            if (GUILayout.Button("Download Road Constructor"))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/level-design/road-constructor-287445?aid=1011l8QY4");
            }
        }

#if GLEY_ROADCONSTRUCTOR_TRAFFIC
        protected override void ScrollPart(float width, float height)
        {
            base.ScrollPart(width, height);

            //EditorGUILayout.Space();
            //EditorGUILayout.LabelField("Select default intersection type to use:");

            EditorGUILayout.Space();
            if (GUILayout.Button("Extract Waypoints"))
            {
                List<int> vehicleTypes = System.Enum.GetValues(typeof(PedestrianTypes)).Cast<int>().ToList();
                RoadConstructorMethods.ExtractWaypoints(vehicleTypes);
            }
        }
#endif
    }
}
