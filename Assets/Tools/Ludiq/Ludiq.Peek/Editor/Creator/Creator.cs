using System;
using Ludiq.Peek;
using Ludiq.PeekCore;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityObject = UnityEngine.Object;

[assembly: InitializeAfterPlugins(typeof(Tabs))]

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public static class Creator
	{
		private static Event e => Event.current;

		internal static void OnSceneGUI(SceneView sceneView)
		{
			if (!PeekPlugin.Configuration.enableCreator.Display(sceneView.maximized))
			{
				return;
			}

			if (SceneViewIntegration.used)
			{
				return;
			}

			Profiler.BeginSample("Peek." + nameof(Creator));

			try
			{
				var position = sceneView.GetInnerGuiPosition(); 

				Handles.BeginGUI();
				
				if (position.Contains(e.mousePosition) && e.CtrlOrCmd() && !e.alt && e.shift)
				{
					var hitType = HandleRaycast.Raycast(sceneView, e.mousePosition, PeekPlugin.Configuration.creatorLayerMask, out var hit);
					var hitSuccess = hitType != HandleRaycastHitType.HitNone;

					var createIndicatorStyle = LudiqStyles.CommandButton(true, true);
					var createIndicatorContent = LudiqGUIUtility.TempContent(PeekPlugin.Icons.createGameObjectOptions?[IconSize.Small]);
					var createIndicatorSize = createIndicatorStyle.CalcSize(createIndicatorContent);

					var createIndicatorPosition = new Rect
					(
						e.mousePosition.x - (createIndicatorSize.x / 2),
						e.mousePosition.y + Styles.indicatorMargin,
						createIndicatorSize.x,
						createIndicatorSize.y
					);
					
					GUI.Label
					(
						createIndicatorPosition,
						createIndicatorContent,
						createIndicatorStyle
					);

					if (e.type == EventType.MouseDown && e.button == (int)MouseButton.Left)
					{
						var activatorPosition = new Rect(e.mousePosition, Vector2.zero);
						activatorPosition.width = 220;
						activatorPosition = LudiqGUIUtility.GUIToScreenRect(activatorPosition);

						// Delay closure allocations
						var _hit = hit;
						var _hitSuccess = hitSuccess;
						var _sceneView = sceneView;

						LudiqGUI.FuzzyDropdown
						(
							activatorPosition,
							new CreateGameObjectOptionTree(),
							null,
							(_instance) =>
							{
								var instance = (GameObject)_instance;

								var is2D = instance.GetComponent<RectTransform>() != null ||
								           instance.GetComponent<SpriteRenderer>() != null;

								var isGUI = instance.GetComponent<RectTransform>() != null;

								if (_hitSuccess)
								{
									instance.transform.SetParent(_hit.transform.parent, true);
								}

								instance.transform.position = _hit.worldPosition;

								if (!is2D && PeekPlugin.Configuration.createOnBounds && instance.CalculateBounds(out var bounds, Space.World, true, false, false, false, false))
								{
									var difference = _hit.worldPosition.y - bounds.min.y;

									instance.transform.position += difference * Vector3.up;
								}

								Selection.activeGameObject = instance;

								if (!_hitSuccess && !_sceneView.in2DMode)
								{
									_sceneView.FrameSelected();
								}
							}
						);

						FuzzyWindow.instance.Focus();

						e.Use();
					}

					Handles.EndGUI();

					// Scale handles take depth into account for handle size, so they're more expressive than position handles

					if (sceneView.in2DMode)
					{
						Handles.PositionHandle(hit.worldPosition, Quaternion.identity);
					}
					else
					{
						Handles.ScaleHandle(Vector3.one, hit.worldPosition, Quaternion.identity, PeekPlugin.Configuration.creatorUnitSize);
					}

					Handles.BeginGUI();

					sceneView.Repaint();
				}

				if (position.Contains(e.mousePosition))
				{
					sceneView.Repaint();
				}

				Handles.EndGUI();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			Profiler.EndSample();
		}

		private static class Styles
		{
			static Styles() { }
			
			public static readonly int indicatorMargin = 20;
		}
	}
}