//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2016 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;

namespace SonicBloom.Koreo.PlayMaker
{
	public class KoreographyEventInfo
	{
		#region Static Fields

		// Global, should only be valid during a callback phase.
		static KoreographyEventInfo TheInfo;

		#endregion
		#region Fields

		public KoreographyEvent koreoEvent = null;
		public int sampleTime = -1;
		public int sampleDelta = -1;
		public DeltaSlice deltaSlice;

		#endregion
		#region Static Properties

		/// <summary>
		/// Gets the current KoreographyEventInfo object for callbacks.  This is intended for use by PlayMaker
		/// Actions during a callback processing phase.
		/// </summary>
		/// <value>The current callback info.</value>
		public static KoreographyEventInfo CallbackInfo
		{
			get
			{
				return TheInfo;
			}
		}

		#endregion
		#region Static Methods

		/// <summary>
		/// Sets the static KoreographyEventInfo object used by PlayMaker Actions during a callback phase.
		/// </summary>
		/// <param name="info">The KoreographyEventInfo to use during this callback phase.</param>
		public static void SetCallbackInfo(KoreographyEventInfo info)
		{
			if (TheInfo != null)
			{
				Debug.LogError("Setting up a new KoreographyEventInfo for callback processing without clearing the previous one first! ");
			}
			
			TheInfo = info;
		}

		/// <summary>
		/// Clears the KoreographyEventInfo object used by PlayMaker Actions during a callback phase.
		/// </summary>
		public static void ClearCallbackInfo()
		{
			TheInfo = null;
		}

		#endregion
		#region Methods

		public void Reset()
		{
			koreoEvent = null;
			sampleTime = -1;
			sampleDelta = -1;
			deltaSlice = new DeltaSlice();
		}

		#endregion
	}
}
