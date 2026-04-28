using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InsaneSystems.RoadNavigator
{
	public sealed class Map : MonoBehaviour, IDragHandler
	{
		public enum NavigatorMode
		{
			Hidden,
			Navigator,
			FullsizeMap
		}
		
		public static Map SceneInstance { get; private set; }
		const float maxZoomValue = 3f;

		public NavigatorMode WorkMode { get; private set; } = NavigatorMode.Hidden;
		public Storage MapStorage => storage;
		public LevelMapSettings GetLevelMapSettings => levelMapSettings;
		public Vector2 RealMapScale => levelMapSettings.realMapScale;
		public Mask MapMask => maskTransform.GetComponent<Mask>();
		public Vector2 CarUIPosition { get; private set; }
		public Vector3 CarUIRealPosition { get; private set; }
		public Vector2 MinimapOffset => mapTransform.anchoredPosition;
		public Vector3 TargetUIRealPosition => targetIconOnMap.position;

		[SerializeField] LevelMapSettings levelMapSettings;
		[SerializeField] Storage storage;

		[Header("Scene objects")]
		[SerializeField] RectTransform playerIconOnMap;
		[SerializeField] RectTransform targetIconOnMap;
		[SerializeField] GameObject selfObject;
		[SerializeField] RectTransform maskTransform;
		[SerializeField] RectTransform mapTransform;
		[SerializeField] RectTransform mapContainerTransform;
		[SerializeField] Image mapImage;
		[SerializeField] RectTransform navigatorPlaceholder;

		[Header("Additional parameters")]
		[Tooltip("If you have an Image in Map Canvas, which should be background of navigator (and active only in Navigator Mode), put it to this field.")]
		[SerializeField] Image customNavigatorBG;
		[Tooltip("If you have an Image in Map Canvas, which should be foreground (for example, outline) of navigator (and active only in Navigator Mode), put it to this field.")]
		[SerializeField] Image customNavigatorFG;

		Transform navigatorPlayerTransform;
		Navigator navigator;

		float zoomValue = 1f;
		float lastPlayerZoom = 1f;
		
		Vector2 mapImageSize;
		readonly List<MapIconInfo> iconsAddedToMap = new List<MapIconInfo>();

		Vector2 iconsCorners;
		
		void Awake()
		{
			SceneInstance = this;

			navigator = GetComponent<Navigator>();

			MapMask.enabled = true;

			playerIconOnMap.GetComponent<Image>().sprite = storage.playerIcon;
			targetIconOnMap.GetComponent<Image>().sprite = storage.targetIcon;

			iconsCorners = storage.mapIconCornersPx * Vector2.one;
		}

		void Start()
		{
			SetupMapImage();
			
			if (navigatorPlaceholder)
				navigatorPlaceholder.gameObject.SetActive(false);
			else 
				Debug.LogWarning("[Road GPS Navigator] Please, setup Navigator Placeholder in Map component parameters. Without it you're using legacy method of Navigator UI position algorithm calculation, which will be removed in future.");
			
			selfObject.SetActive(false);
			
			if (storage.showNavigatorAtStartup)
				ShowAsNavigator();
		}

		public void SetupMapImage()
		{
			mapImage.sprite = levelMapSettings.mapSprite;
			mapImage.rectTransform.sizeDelta = new Vector2(mapImage.sprite.texture.width, mapImage.sprite.texture.height);
			mapImageSize = mapImage.GetComponent<RectTransform>().sizeDelta;
			mapContainerTransform.sizeDelta = mapImageSize;
		}

		void Update()
		{
			HandleInput();

			if (WorkMode == NavigatorMode.Hidden)
				return;

			if (!navigatorPlayerTransform || !navigatorPlayerTransform.gameObject.activeSelf)
			{
				var foundPlayer = FindObjectOfType<NavigatorPlayer>();
	
				if (foundPlayer)
					navigatorPlayerTransform = foundPlayer.transform;
				else
					return;
			}

			CarUIPosition = WorldToMinimapPosition(navigatorPlayerTransform.position);

			playerIconOnMap.anchoredPosition = CarUIPosition;
			playerIconOnMap.localEulerAngles = new Vector3(0, 0, -navigatorPlayerTransform.localEulerAngles.y + storage.playerIconAngleOffset);

			targetIconOnMap.anchoredPosition = navigator.targetUIPoint;

			if (WorkMode == NavigatorMode.Navigator)
			{
				var mapPosition = mapImageSize / 2;

				mapTransform.anchoredPosition = mapPosition - CarUIPosition;
				mapTransform.pivot =  CarUIPosition / mapImageSize;
				mapTransform.localEulerAngles = new Vector3(0, 0, storage.playerIconAngleOffset * 2 - playerIconOnMap.localEulerAngles.z - storage.playerIconAngleOffset);

				playerIconOnMap.anchoredPosition = mapPosition;
				playerIconOnMap.localEulerAngles = new Vector3(0, 0, storage.playerIconAngleOffset);

				OnPointsPositionChanged();
			}
			
			var mapTransformPos = (Vector2) mapTransform.position;
				
			foreach (var icon in iconsAddedToMap)
				if (icon.MapObject && icon.DrawInfo.AlwaysKeepOnMap)
					DrawIconBounded(icon, mapTransformPos);

			CarUIRealPosition = playerIconOnMap.position;
		}

		void DrawIconBounded(MapIconInfo mapIconInfo, Vector2 mapTransformPos)
		{
			mapIconInfo.RectTransform.anchoredPosition = mapIconInfo.InitialUiPosition;
			
			if (WorkMode != NavigatorMode.Navigator)
				return;
			
			var finalPosition = mapIconInfo.RectTransform.position;
			var realScale = transform.localScale.x;

			var halfSize = maskTransform.sizeDelta / 2 - iconsCorners;
			var minPoint = mapTransformPos - halfSize * realScale;
			var maxPoint = mapTransformPos + halfSize * realScale;
			
			if (!IsPositionInBounds(finalPosition, minPoint, maxPoint))
			{
				finalPosition.x = Mathf.Clamp(finalPosition.x, minPoint.x, maxPoint.x);
				finalPosition.y = Mathf.Clamp(finalPosition.y, minPoint.y, maxPoint.y);
							
				mapIconInfo.RectTransform.position = finalPosition;
			}
		}
		
		// todo replace to Rect?
		bool IsPositionInBounds(Vector2 position, Vector2 minPoint, Vector2 maxPoint)
		{
			return position.x >= minPoint.x && position.x <= maxPoint.x &&
			       position.y >= minPoint.y && position.y <= maxPoint.y;
		}

		public void OnDrag(PointerEventData pointerEventData)
		{
			if (WorkMode == NavigatorMode.FullsizeMap && storage.allowDragMapByMouse)
				MapPositionChange(pointerEventData.delta);
		}

		void MapPositionChange(Vector2 mousePositionDelta)
		{
			var newPosition = mapContainerTransform.anchoredPosition + mousePositionDelta;
			var halfMapSize = mapImageSize / 2f * mapContainerTransform.localScale.x;

			newPosition.x = Mathf.Clamp(newPosition.x, -halfMapSize.x, halfMapSize.x);
			newPosition.y = Mathf.Clamp(newPosition.y, -halfMapSize.y, halfMapSize.y);

			mapContainerTransform.anchoredPosition = newPosition;

			OnPointsPositionChanged();
		}

		void HandleInput()
		{
			if (Input.GetKeyDown(storage.showMapKey))
				ShowAsMap();
			if (Input.GetKeyDown(storage.showNavigatorKey))
				ShowAsNavigator();

			if (WorkMode == NavigatorMode.FullsizeMap)
				Zoom();
		}

		void Zoom()
		{
			var zoomAxisValue = Input.GetAxis("Mouse ScrollWheel");

			zoomValue = Mathf.Clamp(zoomValue + zoomAxisValue * Time.deltaTime * 60f * storage.mapZoomSpeed, 1f, maxZoomValue);
			lastPlayerZoom = zoomValue;

			ApplyZoom();

			MapPositionChange(Vector2.zero);
		}

		public Vector2 WorldToMinimapPosition(Vector3 worldPosition)
		{
			if (mapImageSize == Vector2.zero)
				SetupMapImage();

			var uiPosition = new Vector2(worldPosition.x, worldPosition.z);
			uiPosition -= levelMapSettings.baseOffset;

			uiPosition.x = uiPosition.x / levelMapSettings.realMapScale.x * mapImageSize.x;
			uiPosition.y = uiPosition.y / levelMapSettings.realMapScale.y * mapImageSize.y;

			return uiPosition;
		}

		public void ShowAsMap()
		{
			if (WorkMode == NavigatorMode.FullsizeMap)
			{
				selfObject.SetActive(false);
				WorkMode = NavigatorMode.Hidden;
			}
			else 
			{
				selfObject.SetActive(true);
				WorkMode = NavigatorMode.FullsizeMap;
			}

			mapContainerTransform.anchoredPosition = Vector2.zero;

			mapTransform.anchoredPosition = Vector2.zero;
			maskTransform.anchoredPosition = Vector2.zero;
			maskTransform.sizeDelta = new Vector2(0, 0);
			maskTransform.pivot = Vector2.one * 0.5f;

			maskTransform.anchorMin = new Vector2(0f, 0f);
			maskTransform.anchorMax = new Vector2(1f, 1f);
			mapTransform.pivot = Vector2.one * 0.5f;

			mapTransform.localEulerAngles = Vector3.zero;
			
			zoomValue = lastPlayerZoom;
			ApplyZoom();

			if (customNavigatorBG)
				customNavigatorBG.enabled = false;

			if (customNavigatorFG)
				customNavigatorFG.enabled = false;
		}

		public void ShowAsNavigator()
		{
			if (WorkMode == NavigatorMode.Navigator)
			{
				selfObject.SetActive(false);
				WorkMode = NavigatorMode.Hidden;
			}
			else 
			{
				selfObject.SetActive(true);
				WorkMode = NavigatorMode.Navigator;
			}

			mapContainerTransform.anchoredPosition = Vector2.zero;

			if (navigatorPlaceholder)
			{
				maskTransform.pivot = navigatorPlaceholder.pivot;
				maskTransform.anchorMin = navigatorPlaceholder.anchorMin;
				maskTransform.anchorMax = navigatorPlaceholder.anchorMax;

				maskTransform.anchoredPosition = navigatorPlaceholder.anchoredPosition; 
				maskTransform.sizeDelta = navigatorPlaceholder.sizeDelta;
			}
			else // todo legacy, will be removed in future versions
			{
				maskTransform.anchoredPosition = new Vector2(Screen.width / 2 - 10, -Screen.height / 2 + 10);
				maskTransform.sizeDelta = new Vector2(300, 200);
				maskTransform.pivot = new Vector2(1f, 0f);
					
				maskTransform.anchorMin = new Vector2(0.5f, 0.5f);
				maskTransform.anchorMax = new Vector2(0.5f, 0.5f);
			}

			zoomValue = storage.navigatorZoom;
			ApplyZoom();

			if (customNavigatorBG)
				customNavigatorBG.enabled = true;

			if (customNavigatorFG)
				customNavigatorFG.enabled = true;
		}

		void ApplyZoom()
		{
			mapContainerTransform.localScale = Vector3.one * zoomValue;

			OnPointsPositionChanged();
		}

		void OnPointsPositionChanged()
		{
			if (!Navigator.isUsingThreading)
				return;

			for (int i = 0; i < Navigator.navigatorPoints.Length; i++)
				Navigator.navigatorPoints[i].PointPositionChanged();
		}

		/// <summary>Allows add custom icon to the map, for example if your game have Gas Station, some mission on map, or something else. </summary>
		/// <param name="icon">Icon image. Use sprite image here.</param>
		/// <param name="worldPosition">Position of target object in real world (not in map coords). You can take it by transform.position of your object.</param>
		/// <param name="iconName">Name of your icon. It needed only if you want to remove your icon from map in future, for example on mission end etc.</param>
		public void AddObjectToMap(Sprite icon, Vector3 worldPosition, string iconName = "Icon", ObjectToDrawOnMap component = null)
		{
			var spawnedObject = Instantiate(storage.iconTemplate, mapTransform);
			var mapObject = new MapIconInfo()
			{
				MapObject = spawnedObject,
				RectTransform = spawnedObject.GetComponent<RectTransform>(),
				DrawInfo = component,
				InitialUiPosition = WorldToMinimapPosition(worldPosition)
			};
				
			spawnedObject.name = iconName;
			mapObject.RectTransform.anchoredPosition = mapObject.InitialUiPosition;
			spawnedObject.GetComponent<Image>().sprite = icon;
			iconsAddedToMap.Add(mapObject);
		}

		/// <summary> Allows to remove previously added to map icon by its name. </summary>
		public void RemoveObjectFromMap(string iconName)
		{
			foreach (var customMapIcon in iconsAddedToMap)
			{
				if (customMapIcon.MapObject && customMapIcon.MapObject .name == iconName)
				{
					Destroy(customMapIcon.MapObject);
					iconsAddedToMap.Remove(customMapIcon);
					break;
				}
			}
		}

		/// <summary> Change object which should be used as a player to follow. </summary>
		public void SetNavigatorPlayer(NavigatorPlayer navigatorPlayer)
		{
			navigatorPlayerTransform = navigatorPlayer ? navigatorPlayer.transform : null;
		}

		void OnDrawGizmos()
		{
			if (!levelMapSettings)
				return;

			Gizmos.color = Color.red;

			var center2 = Vector2.Lerp(levelMapSettings.baseOffset, levelMapSettings.realMapScale + levelMapSettings.baseOffset, 0.5f);
			var realCenter2 = new Vector3(center2.x, 0, center2.y);
			var size2 = new Vector3(levelMapSettings.realMapScale.x, 0f, levelMapSettings.realMapScale.y);
			Gizmos.DrawWireCube(realCenter2, size2);
		}
	}
}