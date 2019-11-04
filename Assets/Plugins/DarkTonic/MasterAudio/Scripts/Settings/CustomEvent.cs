/*! \cond PRIVATE */
using System;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    [Serializable]
    // ReSharper disable once CheckNamespace
    public class CustomEvent {
        public string EventName;
        public string ProspectiveName;
        public bool IsEditing;
        // ReSharper disable InconsistentNaming
        public bool eventExpanded = true;
        public MasterAudio.CustomEventReceiveMode eventReceiveMode = MasterAudio.CustomEventReceiveMode.Always;
        public float distanceThreshold = 1f;
        public MasterAudio.EventReceiveFilter eventRcvFilterMode = MasterAudio.EventReceiveFilter.All;
        public int filterModeQty = 1;
		public bool isTemporary = false;
		public int frameLastFired = -1;
        public string categoryName = MasterAudio.NoCategory;
        // ReSharper restore InconsistentNaming

        public CustomEvent(string eventName) {
            EventName = eventName;
            ProspectiveName = eventName;
        }
    }
}
/*! \endcond */