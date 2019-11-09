using System;
using System.IO;

namespace AlmostEngine
{
	public class PathUtils
	{
		public static bool IsValidPath (string path)
		{
			#if UNITY_EDITOR_WIN
			// Check thar first 3 chars are a drive letter
			if (path.Length < 3)
				return false;
			if (!Char.IsLetter (path [0]))
				return false;
			if (path [1] != ':')
				return false;
			if (path [2] != '\\' && path [2] != '/')
				return false;
			#endif

			if (String.IsNullOrEmpty (path)) {
				return false;
			}

			char[] invalids = Path.GetInvalidPathChars ();
			foreach (char c in invalids) {
				if (path.Contains (c.ToString ()))
					return false;
			}

			try {
				Path.GetFullPath (path);
			} catch {
				return false;
			}


			return true;
		}
	}
}

