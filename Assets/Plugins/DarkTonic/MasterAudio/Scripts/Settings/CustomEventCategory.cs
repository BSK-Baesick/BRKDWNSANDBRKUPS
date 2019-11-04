/*! \cond PRIVATE */
using System;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
	[Serializable]
	// ReSharper disable once CheckNamespace
	public class CustomEventCategory {
		public string CatName = MasterAudio.NoCategory;
		public bool IsExpanded = true;
		public bool IsEditing = false;
		public bool IsTemporary = false;
		public string ProspectiveName = MasterAudio.NoCategory;
	}
}
/*! \endcond */