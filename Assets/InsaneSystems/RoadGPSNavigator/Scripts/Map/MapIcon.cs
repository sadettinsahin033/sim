using UnityEngine;

namespace InsaneSystems.RoadNavigator
{
	public class MapIcon : MonoBehaviour
	{
		[SerializeField] bool isPlayerIcon;
		Transform mainParent;

		float timer;

		void Start()
		{
			mainParent = transform.parent.parent;
			//transform.localEulerAngles = new Vector3(-mainParent.localEulerAngles.z, 0, 0);

			if (isPlayerIcon)
				mainParent = transform.parent;
		}

		void Update()
		{
			if (timer > 0f)
			{
				timer -= Time.deltaTime;
				return;
			}

			transform.localScale = Vector3.one / mainParent.localScale.x;

			if (Map.SceneInstance)
			{
				if (Map.SceneInstance.WorkMode == Map.NavigatorMode.Navigator)
					transform.localScale *= Map.SceneInstance.MapStorage.navigatorIconsScaling;
				else
					transform.localScale *= Map.SceneInstance.MapStorage.mapIconsScaling;
			}

			if (!isPlayerIcon)
			{
				var up = transform.position + Vector3.forward;
				transform.rotation = Quaternion.LookRotation(up - transform.position);
			}

			timer = 0.1f;
		}
	}
}