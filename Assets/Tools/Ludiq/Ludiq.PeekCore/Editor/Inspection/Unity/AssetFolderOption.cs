using System.IO;
using UnityEditor;

namespace Ludiq.PeekCore
{
	public class AssetFolderOption : FuzzyOption<string>
	{
		public AssetFolderOption(string pathFromProject, FuzzyOptionMode mode) : base(mode)
		{
			this.value = pathFromProject;
			this.label = Path.GetFileName(pathFromProject);
			getIcon = () => LudiqCore.Icons.folder;
		}
	}
}