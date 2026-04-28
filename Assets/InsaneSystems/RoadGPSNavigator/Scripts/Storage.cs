using UnityEngine;

namespace InsaneSystems.RoadNavigator
{
	[CreateAssetMenu(fileName = "Storage", menuName = "Insane Systems/Road GPS Navigator/Storage")]
	public class Storage : ScriptableObject
	{
		[Header("Map Settings")]
		[Range(0.25f, 4f)] public float mapIconsScaling = 1f;
		[Range(0.25f, 4f)] public float navigatorIconsScaling = 1f;
		public Color linesColor = new Color(1f, 0.75f, 0f, 1f);
		[Range(1, 48)] public int lineWidthInPx = 16;
		[Tooltip("If your icon sprite rotated wrong, you can edit this value. Try to set 90 or 180 to fix problem.")]
		public float playerIconAngleOffset = 0;
		public bool allowDragMapByMouse = true;
		[Range(0.25f, 4f)] public float mapZoomSpeed = 1f;
		public Sprite playerIcon;
		public Sprite targetIcon;
		[Range(0, 60)] public int mapIconCornersPx = 15;

		[Header("Navigator settings")]
		public bool showNavigatorAtStartup = true;
		[Tooltip("This value means how will be navigator scaled regarding to map scale.")]
		[Range(0.5f, 6f)] public float navigatorZoom = 1.5f;
		[Tooltip("How oftenly should be redrawn navigation line? Lower values will give smooth experience, but can affect game performance.")]
		[Range(0.25f, 3f)] public float rebuildLineEverySeconds = 0.5f;
		public bool useThreadingForOptimization = true;
		[Tooltip("PREVIEW VERSION! Use it on your own risk. Enables wave search algorithm. It can work faster, but in preview version it can work not fully correct.")]
		public bool useNewAlgorithm;

		[Header("Input settings")]
		public KeyCode showMapKey = KeyCode.M;
		public KeyCode showNavigatorKey = KeyCode.N;

		[Header("UI Templates")]
		public GameObject iconTemplate;
		public GameObject lineTemplate;
		public GameObject connectorCircleTemplate;

		[Header("Advanced Settings")]
		[Tooltip("Count of pathes to check to reach the destination. Increasing this value can solve some problems on big maps, but decreases performance. Do not change it if you don't know what you're doing.")]
		[Range(15, 200)] public int checkedPathsLimit = 75;
		[Tooltip("Count of points in one path to check to reach the destination. Increasing this value can solve some problems on big maps, but decreases performance. Do not change it if you don't know what you're doing.")]
		[Range(25, 600)] public int checkedPointsLimit = 120;
		[Tooltip("Enable alpha-preview features. Stable and correct work not guaranted until these features will be finally released.")]
		public bool enableAlphaFeatures;
	}
}