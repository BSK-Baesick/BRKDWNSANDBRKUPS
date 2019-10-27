using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using UEditor = UnityEditor.Editor;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public class CreateGameObjectOption : FuzzyOption<LudiqGUI.PopupFunc>
	{
		private static UnityObject editorTarget;

		private static UEditor editor;

		private GameObject prefab;

		private Sprite sprite;

		private string menuItem;

		public CreateGameObjectOption(GameObject prefab, string label) : base(FuzzyOptionMode.Leaf)
		{
			Ensure.That(nameof(prefab)).IsNotNull(prefab);

			if (label == null)
			{
				label = UnityAPI.Await(() => prefab.name);
			}

			this.label = label;
			this.prefab = prefab;

			value = InstantiatePrefab;
			getIcon = () =>
			{
				if (PeekPlugin.Configuration.enablePreviewIcons && PreviewUtility.TryGetPreview(prefab, out var preview) && !AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()))
				{
					return EditorTexture.Single(preview);
				}
				else
				{
					return prefab.Icon();
				}
			};
		}

		public CreateGameObjectOption(Sprite sprite, string label) : base(FuzzyOptionMode.Leaf)
		{
			Ensure.That(nameof(sprite)).IsNotNull(sprite);

			if (label == null)
			{
				label = UnityAPI.Await(() => sprite.name);
			}

			this.label = label;
			this.sprite = sprite;

			value = InstantiateSprite;

			getIcon = () =>
			{
				if (PeekPlugin.Configuration.enablePreviewIcons && PreviewUtility.TryGetPreview(sprite, out var preview) && !AssetPreview.IsLoadingAssetPreview(sprite.GetInstanceID()))
				{
					return EditorTexture.Single(preview);
				}
				else
				{
					return sprite.Icon();
				}
			};
		}

		public CreateGameObjectOption(string menuItem) : base(FuzzyOptionMode.Leaf)
		{
			Ensure.That(nameof(menuItem)).IsNotNull(menuItem);

			this.menuItem = menuItem;
			label = menuItem.PartAfterLast('/').TrimStart("Create ");
			value = ExecuteMenuItem;
			getIcon = () => PeekPlugin.Icons.createGameObject;
		}

		private bool InstantiatePrefab(out object value)
		{
			var instance = PrefabUtility.InstantiatePrefab(prefab);
			Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
			value = instance;
			return true;
		}

		private bool InstantiateSprite(out object value)
		{
			var instance = new GameObject(sprite.name);
			var renderer = instance.AddComponent<SpriteRenderer>();
			renderer.sprite = sprite;
			Undo.RegisterCreatedObjectUndo(instance, "Create " + instance.name);
			value = instance;
			return true;
		}

		private bool ExecuteMenuItem(out object value)
		{
			var objectsBefore = UnityObject.FindObjectsOfType(typeof(GameObject));
			EditorApplication.ExecuteMenuItem(menuItem);
			var objectsAfter = UnityObject.FindObjectsOfType(typeof(GameObject));
			var createdObjects = objectsAfter.Except(objectsBefore).Cast<GameObject>().ToArray();

			GameObject topmostObject = null;

			foreach (var createdObject in createdObjects)
			{
				var isTopmost = true;

				foreach (var otherCreatedObject in createdObjects)
				{
					if (createdObject == otherCreatedObject)
					{
						continue;
					}

					if (createdObject.transform.IsChildOf(otherCreatedObject.transform))
					{
						isTopmost = false;
						break;
					}
				}

				if (isTopmost)
				{
					topmostObject = createdObject;
				}
			}

			value = topmostObject;

			return topmostObject != null;
		}

		public override bool hasFooter => prefab != null && prefab.GetComponentInChildren<Renderer>() != null;

		public override float GetFooterHeight(FuzzyOptionNode node, float width)
		{
			return 128;
		}

		public override void OnFooterGUI(FuzzyOptionNode node, Rect position)
		{
			if (editorTarget != prefab)
			{
				editorTarget = prefab;
				UEditor.CreateCachedEditor(editorTarget, null, ref editor);
			}

			if (editor != null)
			{
				editor.DrawPreview(position);
			}
		}

		public override string SearchResultLabel(string query)
		{
			var label = base.SearchResultLabel(query);

			if (menuItem != null)
			{
				label += LudiqGUIUtility.DimString($" (in {menuItem.PartBeforeLast('/')})");
			}

			return label;
		}
	}
}