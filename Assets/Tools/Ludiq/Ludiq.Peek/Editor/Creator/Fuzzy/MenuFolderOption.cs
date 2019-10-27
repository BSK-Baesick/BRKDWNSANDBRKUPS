namespace Ludiq.Peek
{
	// ReSharper disable once RedundantUsingDirective
	using PeekCore;

	public class MenuFolderOption : FuzzyOption<string>
	{
		public MenuFolderOption(string path, FuzzyOptionMode mode) : base(mode)
		{
			value = path;
			label = path.PartAfterLast('/');
			getIcon = () => LudiqCore.Icons.folder;
		}
	}
}