using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public static class HandleRaycast
	{
		public static HandleRaycastHitType Raycast(SceneView sceneView, Vector2 mousePosition, int layerMask, out HandleRaycastHit hit)
		{
			hit = new HandleRaycastHit();

			// Attempt a 3D raycast

			var ray3D = HandleUtility.GUIPointToWorldRay(mousePosition);
			
			if (Physics.Raycast(ray3D, out var hit3D, Mathf.Infinity, layerMask))
			{
				hit.worldPosition = hit3D.point;
				hit.transform = hit3D.transform;
				return HandleRaycastHitType.Hit3D;
			}

			// Attempt a 2D raycast

			Vector3 screenPosition = HandleUtility.GUIPointToScreenPixelCoordinate(mousePosition);
			var point2D = sceneView.camera.ScreenToWorldPoint(screenPosition);
			point2D.z = 0;

			var collider2D = Physics2D.OverlapPoint(point2D);

			if (collider2D != null)
			{
				hit.worldPosition = point2D;
				hit.worldPosition.z = collider2D.transform.position.z;
				hit.transform = collider2D.transform;
				return HandleRaycastHitType.Hit2D;
			}

			// Attempt a GUI raycast (Not working in editor apparently)

			/*
			var eventSystem = UnityObject.FindObjectOfType<EventSystem>(); // EventSystem.current not available in edit mode

			if (eventSystem != null)
			{
				var hitsGUI = ListPool<RaycastResult>.New();

				var pointerEventData = new PointerEventData(eventSystem);
				pointerEventData.position = mousePosition;

				eventSystem.RaycastAll(pointerEventData, hitsGUI);

				if (hitsGUI.Count > 0)
				{
					var hitGUI = hitsGUI[0];

					hit.worldPosition = hitGUI.worldPosition;
					hit.transform = hitGUI.gameObject.transform;
					Debug.Log(hit.transform + "\n" + hit.worldPosition);
					return HandleRaycastHitType.HitGUI;
				}

				hitsGUI.Free();
			}
			*/

			// Failed, just pick a point in front

			screenPosition.z = sceneView.cameraDistance;
			hit.worldPosition = sceneView.camera.ScreenToWorldPoint(screenPosition);
			hit.transform = null;
			return HandleRaycastHitType.HitNone;
		}
	}
}