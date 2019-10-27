using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public class CreateGameObjectOptionTree : FuzzyOptionTree
	{
		public CreateGameObjectOptionTree(string title = "Create") : base(new GUIContent(title)) { }

		private Dictionary<GameObject, string> prefabs;

		private Dictionary<Sprite, string> sprites;

		private List<string> primitives;

		public override void Prewarm()
		{
			base.Prewarm();

			UnityAPI.Await
			(
				() =>
				{
					// Find prefabs and optionally models
					// TODO: Late load

					var searchString = "t:prefab";

					if (PeekPlugin.Configuration.createModels)
					{
						searchString += " t:model";
					}

					prefabs = new Dictionary<GameObject, string>(ReferenceEqualityComparer<GameObject>.Instance);
					
					foreach (var prefab in AssetDatabase.FindAssets(searchString)
						.Select(AssetDatabase.GUIDToAssetPath)
						.Select(AssetDatabase.LoadMainAssetAtPath)
						.OfType<GameObject>())
					{
						if (!prefabs.ContainsKey(prefab))
						{
							prefabs.Add(prefab, prefab.name);
						}
					}
					
					// Find sprites
					if (PeekPlugin.Configuration.createSprites)
					{
						searchString = "t:sprite";
						sprites = new Dictionary<Sprite, string>(ReferenceEqualityComparer<Sprite>.Instance);

						foreach (var sprite in AssetDatabase.FindAssets(searchString)
							.Select(AssetDatabase.GUIDToAssetPath)
							.SelectMany(AssetDatabase.LoadAllAssetRepresentationsAtPath)
							.OfType<Sprite>())
						{
							if (!sprites.ContainsKey(sprite))
							{
								sprites.Add(sprite, sprite.name);
							}
						}
					}

					// Find primitives
					primitives = EditorMainMenu.Parse().Where
					(
						item => item.StartsWith("GameObject/") &&
						        !excludedMenuItems.Contains(item) &&
						        !PeekPlugin.Configuration.createMenuBlacklist.Contains(item)
					).ToList();
				}
			);
		}

		private static HashSet<string> excludedMenuItems = new HashSet<string>()
		{
			"GameObject/Create Prefab",
			"GameObject/Create Parent",
			"GameObject/Create Sibling",
			"GameObject/Create Child",
			"GameObject/Replace",

			"GameObject/Create Empty Child",
			"GameObject/Center On Children",
			"GameObject/Make Parent",
			"GameObject/Clear Parent",
			"GameObject/Set as first sibling",
			"GameObject/Set as last sibling",
			"GameObject/Move To View",
			"GameObject/Align With View",
			"GameObject/Align View to Selected",
			"GameObject/Toggle Active State",
		};

		private CreateGameObjectOption PrefabOption(GameObject prefab)
		{
			if (!prefabs.TryGetValue(prefab, out var label))
			{
				label = UnityAPI.Await(() => prefab.name);
			}

			return new CreateGameObjectOption(prefab, label);
		}

		private CreateGameObjectOption SpriteOption(Sprite sprite)
		{
			if (!sprites.TryGetValue(sprite, out var label))
			{
				label = UnityAPI.Await(() => sprite.name);
			}

			return new CreateGameObjectOption(sprite, label);
		}

		private CreateGameObjectOption MenuItemOption(string menuItem)
		{
			return new CreateGameObjectOption(menuItem);
		}

		public override IEnumerable<IFuzzyOption> Root()
		{
			if (primitives.Count > 0)
			{
				yield return Separator("Primitives");

				foreach (var option in PrimitiveChildren("GameObject"))
				{
					yield return option;
				}
			}

			if (prefabs.Count > 0)
			{
				yield return Separator("Prefabs");

				foreach (var option in AssetChildren("Assets"))
				{
					yield return option;
				}
			}
		}

		private IEnumerable<IFuzzyOption> AssetChildren(string pathFromProject)
		{
			var fullPath = Path.Combine(Paths.project, pathFromProject);

			foreach (var directory in Directory.EnumerateDirectories(fullPath))
			{
				yield return new AssetFolderOption(PathUtility.FromProject(directory), FuzzyOptionMode.Branch);
			}

			foreach (var prefabChild in UnityAPI.Await(() => LoadPrefabChildren(fullPath).ToArray()))
			{
				yield return prefabChild;
			}

			foreach (var spriteChild in UnityAPI.Await(() => LoadSpriteChildren(fullPath).ToArray()))
			{
				yield return spriteChild;
			}
		}

		private bool MenuItemHasChildren(string menuItem)
		{
			return primitives.Any(m => m.StartsWith(menuItem + "/"));
		}

		private IEnumerable<IFuzzyOption> PrimitiveChildren(string path)
		{
			var children = primitives.Where(m => m != path && m.PartBeforeLast('/') == path).ToArray();

			foreach (var menuItem in children)
			{
				if (!MenuItemHasChildren(menuItem))
				{
					yield return MenuItemOption(menuItem);
				}
			}

			foreach (var menuItem in children)
			{
				if (MenuItemHasChildren(menuItem))
				{
					yield return new MenuFolderOption(menuItem, FuzzyOptionMode.Branch);
				}
			}
		}

		private IEnumerable<IFuzzyOption> LoadPrefabChildren(string fullPath)
		{
			foreach (var file in Directory.EnumerateFiles(fullPath))
			{
				var assetPath = PathUtility.FromProject(file);
				var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

				if (asset != null && !asset.IsHidden())
				{
					yield return PrefabOption(asset);
				}
			}
		}
		
		private IEnumerable<IFuzzyOption> LoadSpriteChildren(string fullPath)
		{
			foreach (var file in Directory.EnumerateFiles(fullPath))
			{
				var assetPath = PathUtility.FromProject(file);
				var sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath).OfType<Sprite>();

				foreach (var sprite in sprites)
				{
					if (sprite != null && !sprite.IsHidden())
					{
						yield return SpriteOption(sprite);
					}
				}
			}
		}

		public override IEnumerable<IFuzzyOption> Children(IFuzzyOption parent, bool ordered)
		{
			if (parent is AssetFolderOption assetFolderOption)
			{
				return AssetChildren(assetFolderOption.value);
			}
			else if (parent is MenuFolderOption folderOption)
			{
				return PrimitiveChildren(folderOption.value);
			}
			else
			{
				return Enumerable.Empty<IFuzzyOption>();
			}
		}



		#region Search

		public override bool searchable { get; } = true;

		public override IEnumerable<IFuzzyOption> SearchableRoot()
		{
			foreach (var menuItem in primitives)
			{
				if (!MenuItemHasChildren(menuItem))
				{
					yield return MenuItemOption(menuItem);
				}
			}

			foreach (var prefab in prefabs)
			{
				yield return PrefabOption(prefab.Key);
			}

			foreach (var sprite in sprites)
			{
				yield return SpriteOption(sprite.Key);
			}
		}

		#endregion
	}
}