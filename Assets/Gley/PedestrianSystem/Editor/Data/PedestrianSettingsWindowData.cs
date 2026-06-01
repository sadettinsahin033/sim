using Gley.UrbanSystem.Editor;
using System.Collections.Generic;

#if GLEY_PEDESTRIAN_SYSTEM
using PedestrianTypes = Gley.PedestrianSystem.User.PedestrianTypes;
#else
using PedestrianTypes = Gley.PedestrianSystem.PedestrianTypes;
#endif

namespace Gley.PedestrianSystem.Editor
{
    /// <summary>
    /// Add additional specific pedestrian settings for the settings window data. 
    /// </summary>
    public class PedestrianSettingsWindowData : SettingsWindowData
    {
        public List<PedestrianTypes> GlobalPedestrianList = new List<PedestrianTypes>();

        public override SettingsWindowData Initialize()
        {
            if (LaneWidth == default)
            {
                LaneWidth = 4;
            }
            if (WaypointDistance == default)
            {
                WaypointDistance = 4;
            }
            return this;
        }
    }
}