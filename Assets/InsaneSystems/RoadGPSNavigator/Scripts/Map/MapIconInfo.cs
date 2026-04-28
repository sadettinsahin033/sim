using UnityEngine;

namespace InsaneSystems.RoadNavigator
{
	public class MapIconInfo
	{
		public GameObject MapObject { get; set; }
		public RectTransform RectTransform { get; set; }
		public ObjectToDrawOnMap DrawInfo { get; set; }
		public Vector2 InitialUiPosition { get; set; }
	}
}