#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class SetRenderQueue : MonoBehaviour {
	public int[] m_queues = new int[]{3000};
}
[CanEditMultipleObjects]

[CustomEditor(typeof(SetRenderQueue))]
public class SetRenderQueueEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		DrawCustomInspector();
	}
	public void DrawCustomInspector() {
		SetRenderQueue t = (SetRenderQueue)target;
		if (GUILayout.Button("Get Render Queue")) {
			Material[] materials = t.GetComponent<Renderer>().sharedMaterials;
			for (int i = 0; i < materials.Length && i < t.m_queues.Length; ++i) {
				t.m_queues[i] = materials[i].renderQueue;
			}
		}

		if (GUILayout.Button("Set Render Queue")) {
			Material[] materials = t.GetComponent<Renderer>().sharedMaterials;
			for (int i = 0; i < materials.Length && i < t.m_queues.Length; ++i) {
				materials[i].renderQueue = t.m_queues[i];
			}
		}
	}
}
#endif