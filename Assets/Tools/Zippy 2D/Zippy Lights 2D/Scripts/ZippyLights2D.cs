#pragma warning disable 0618
using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]

public class ZippyLights2D : MonoBehaviour {

	[HideInInspector]	public bool idle;                                   // When idle the mesh generation is in pause mode, mesh is refreshed if light ray points move.
	[HideInInspector]	public bool lightEnabled = true;                    // Rays will only be cast if this property is enabled.

	//[Header("Light Settings")]

	[Tooltip("Disables light script on start, used for lights that never changes shape.")]
	[SerializeField]  public bool staticLight = false; 

	[Tooltip("How many rays and points the light emits.")]
		[Range(10, 720)]
		public int resolution = 50;

	[Tooltip("Light area degrees.")]
		[Range(10, 360)]
		public float degrees = 360f;
		[HideInInspector]
		public float oldDegrees = 360f;

	[Tooltip("Extend the light mesh beyond colliders.")]
		public float offset = 0f;
	[Tooltip("Extend closer colliders more.")]
		public float offsetSpherify = 0f;

[Tooltip("Only update the light if it is moving.")]
		public bool moveToUpdate;

	[Tooltip("What physics layers light interacts with.")]
		public LayerMask layers = -1;

	[Tooltip("Unity light to apply colors to.")]
		public Light unityLight;

	[Tooltip("Object to follow.")]
		public Transform follow;


	//[Header("Range Settings")]

	[Tooltip("How far the light travels.")]
		//[Range(1f, 500f)]
		public float range = 15f;

	[Tooltip("Enable range animation.")]
		public bool animateRange;

	[Tooltip("How to scale range over time.")]
		public AnimationCurve rangeAnimation;

	[Tooltip("Animated light range speed.")]
		[Range(0f, 5f)]
		public float animateRangeSpeed = 1f;

	[Tooltip("Animated light distance.")]
		public float animateRangeScale = 1f;

	
	//[Header("Color Settings")]
	[Tooltip("Enable vertex colors.")]
		public bool enableVertexColors = true;

	[Tooltip("Fade edge transparancy.")]
		public bool vertexFade;

	[Tooltip("Main color of the light.")]
		public Color vertexColor = Color.white;

	[Tooltip("Enables different color on the outside rim of the light.")]
		public bool enableOuterColor = false;

	[Tooltip("Secondary outside color of the light.")]
		public Color vertexColorOuter = Color.white;

	[Tooltip("Enable inner light color animation.")]
		public bool ColorCycleEnabled;

	[Tooltip("Colors to apply over time.")]
		public Gradient ColorCycle;

	[Tooltip("How fast to cycle colors over time.")]
		public float ColorCycleSpeed = 1f;

	[Tooltip("Enable mesh color animation for edge of mesh.")]
		public bool ColorCycleOuterEnabled;

	[Tooltip("Colors to apply over time.")]
		public Gradient ColorCycleOuter;

	[Tooltip("How fast to cycle outer colors over time.")]
		public float ColorCycleSpeedOuter = 1f;

	//[Header("UV Settings")]
	[Tooltip("Enable UV generation in mesh.")]
		public bool CreateUV = true;

	[Tooltip("Size adjustment of mesh UV.")]
		public float UVScale = 1f;

	//[Header("Noise Settings")]
	[Tooltip("Randomize positions of mesh verts.")]
		[Range(0f, 2f)]
		public float noise = 0;

	[Tooltip("Delay between each randomization.")]
		[Range(0.01f, .5f)]
		public float noiseDelay = .05f;
		float noiseVal;

	//[Header("Sort Settings")]
	[Tooltip("Sprite sorting order.")]
		public int sortingOrder = 1;

	[Tooltip("Sprite sorting layer.")]
	[UnluckSoftware.SortingLayer]
	public int sortingLayer;

	//[Header("Particle Settings")]
	[Tooltip("Particles emitted when a light ray hits something.")]
		public ParticleSystem particles;
	
	[Tooltip("Delay between each particle emit.")]
		[Range(0.02f, .1f)]
		public float particleEmitDelay = .1f;
		
	[Tooltip("How many rays emit particles.")]
		[Range(0.02f, .1f)]
		public float particleRayAmount = .1f;

	[Tooltip("How many particles to emit.")]
		[Range(1f, 10)]
		public int particleEmitAmount = 2;

	[Tooltip("Minimum distance light have to travel to emit particle.")]
		public float particleRangeLimitMin = .05f;

	[Tooltip("Maximum distance light can travel to emit particle.")]
		public float particleRangeLimitMax = 5f;

#if UNITY_EDITOR
	//[Header("Editor Settings")]
	public bool updateInEditor = true;

	public bool showLightSettings = true;
	public bool showRange = true;
	public bool showColor = false;
	public bool showUV = false;
	public bool showNoise = false;
	public bool showSort =true;
	public bool showParticle;
	public bool showFunctions;


#endif

	float lightTime;

	Vector3[] points;									// Holds list of all point raycast hits and default point positions.
	[HideInInspector]	Vector3[] pointsX;				// Previous version of points, used to compare positions from newly genereated points with the previous. 
	[HideInInspector]	public Vector3[] pointsP;		// Holds list of only point positions where the raycast hit something, no default point positions.
	[HideInInspector]	public float[] str;			    // List of how far away a "points" point is from center.
	[HideInInspector]	public Vector3 savePos;			// Used to detect if light has moved.
	[HideInInspector]	public Quaternion saveRot;		// Used to dect if light has rotated.
	Mesh lightMesh;										// Mesh used for generated light.

	// References to components
	[HideInInspector]	public Transform cacheTransform;
	[HideInInspector]	public Transform cacheParticleTransform;	                  
	[HideInInspector]	MeshRenderer cacheMeshRenderer;
	MeshFilter cacheMeshFilter;
	int pointsPlenght = 0;
	
	float degreeResolution;

	[HideInInspector]	public Vector3[] verts;
	[HideInInspector]	public Color[] colors;
	[HideInInspector]	public Vector2[] u;
	[HideInInspector]	public int[] tris;
	[HideInInspector]	public int currentResolution = -1;

	//WIP future update.
	//public float width = 1;
	//public float test = 5;
	//public float what = .5f;


	Vector2[] pointsC;

	void Awake() {
		Init();
		Noise();
		EmitParticles();
		cacheTransform = transform;
#if UNITY_EDITOR
		if (Application.isPlaying && staticLight) this.enabled = false;
#else
		if (staticLight) this.enabled = false;
#endif

	}

	public void ForceNewMesh() {
		DestroyImmediate(lightMesh);
		lightMesh = null;
		cacheTransform = transform;
		ForceUpdate();
	}

	void OnEnable() {
		if(particles) particles.Clear();
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			Init();
			Brighten();
			EditorUtility.SetSelectedWireframeHidden(GetComponent<MeshRenderer>(), true);
		}
#endif
	}

	void Noise() {
		Invoke("Noise", noiseDelay);
		if (!lightEnabled || noise <= 0) return;
		noiseVal = 0;
		noiseVal = Random.Range(-noise, noise);
	}

	void EmitParticles() {
		Invoke("EmitParticles", particleEmitDelay);
		if (!lightEnabled || !particles || pointsP == null) return;
		for (int i = 0; i < pointsPlenght; i++) {
			cacheParticleTransform.position = pointsP[i];
			particles.Emit(particleEmitAmount);
		}
	}

	public void Init() {	
		if (lightMesh == null) lightMesh = new Mesh();
		cacheMeshFilter = gameObject.GetComponent<MeshFilter>();
		cacheMeshRenderer = gameObject.GetComponent<MeshRenderer>();
		cacheMeshRenderer.sortingOrder = sortingOrder;
		cacheMeshRenderer.sortingLayerID = sortingLayer;
		if (!cacheTransform) cacheTransform = transform;
		if (particles && !cacheParticleTransform) cacheParticleTransform = particles.transform;
		if (points == null) points = new Vector3[resolution];
		degreeResolution = degrees / resolution;
		Brighten(true);
	}

	void LateUpdate() {
#if UNITY_EDITOR
		if (!updateInEditor) return;
		if (!Application.isPlaying) {
			Init();
			//Debug.Log(lightTime);
		}

#endif
		if (oldDegrees == degrees) {
#if UNITY_EDITOR
			if (Application.isPlaying)
#endif
			Brighten();
			return;
		}
		ForceUpdate();
		oldDegrees = degrees;
	}

#if UNITY_EDITOR
	void OnValidate() {
		if (offsetSpherify < 0) offsetSpherify = 0;
		if (offset < 0) offset = 0;
	}
#endif

	public void Brighten(bool forceBrighten = false) {
		if (follow) {
			cacheTransform.position = new Vector3(follow.position.x, follow.position.y, 0);
			if(degrees < 360) cacheTransform.eulerAngles = new Vector3(0, 0, follow.eulerAngles.z -degrees*.5f);
		}
		if (!forceBrighten && !cacheMeshRenderer.isVisible) {
			lightEnabled = false;
			return;
		} else if (lightEnabled == false) {
			lightEnabled = true;
		}
		lightTime = Time.time;
#if UNITY_EDITOR
		if (!Application.isPlaying) lightTime = (float)EditorApplication.timeSinceStartup;
#endif
		if (Application.isPlaying && moveToUpdate && savePos == cacheTransform.position && saveRot == cacheTransform.rotation) return;
		ScanPoints();
		MeshGen();
		savePos = cacheTransform.position;
		saveRot = cacheTransform.rotation;
		if (!lightEnabled) return;
		if (ColorCycleEnabled) {
			Color c = ColorCycle.Evaluate(lightTime * ColorCycleSpeed % 1);
			vertexColor = c;
			if (unityLight) unityLight.color = c;
		}
		if (ColorCycleOuterEnabled) {
			Color c = ColorCycleOuter.Evaluate(lightTime * ColorCycleSpeedOuter % 1);
			vertexColorOuter = c;
			if (unityLight && !ColorCycleEnabled) unityLight.color = c;
		}
		if (animateRange) range = rangeAnimation.Evaluate(lightTime * animateRangeSpeed % 1) * animateRangeScale;
	}

	void ScanPoints() {
		//int r = resolution + Mathf.CeilToInt(x);
		degreeResolution = degrees / resolution;

		if (points == null || points.Length != resolution) points = new Vector3[resolution];
		if (pointsP == null || pointsP.Length != resolution) pointsP = new Vector3[resolution];
		if (str == null || str.Length != resolution) str = new float[resolution];


		pointsPlenght = 0;
		Vector3 d = cacheTransform.up;
		int pr = (int)(resolution * particleRayAmount);
		Vector3 f = Vector3.forward;
		for (int i = 0; i < resolution; i++) {
			Quaternion q = Quaternion.AngleAxis((float)(i * degreeResolution) + noiseVal, f);
			Vector3 qd = q*d;
			RaycastHit2D hit = Physics2D.Raycast(cacheTransform.position, qd, range, layers);
			float dist = hit.distance;
			if (hit) {
				if (particles && dist < particleRangeLimitMax && dist > particleRangeLimitMin && i % pr == Random.Range(0, pr+1) ) {

					pointsP[pointsPlenght] = hit.point;
					

					pointsPlenght++;
				}
				//if (i == (int)Random.Range(0, resolution)) Debug.Log(hit.collider.gameObject);
				points[i] = hit.point;

				str[i] = 1 - dist / range;
				if (offset != 0) {
					Vector2 np;
					if (offsetSpherify == 0) {
						np = Vector2.MoveTowards(points[i], transform.position, -offset);
						if (enableVertexColors) {
							float nd = Vector2.Distance(points[i], np);
							str[i] = 1 - (dist + nd) / range;
						}	
					} else {
						np = Vector2.MoveTowards(points[i], transform.position, -offset * (1 + str[i] * offsetSpherify));
						float nd = Vector2.Distance(points[i], np);
						str[i] = 1 - (dist + nd) / range;
					}

					

					points[i] = np;


				}
			} else {
				points[i] = cacheTransform.position + qd * range;
				str[i] = 0;
			}
			
		}
		//ResizeArray(resolution, ref points);
	}

	public void ResizeArray(int size, ref Vector3[] arr) {
		Vector3[] tmp = new Vector3[size];
		for (int c = 0; c < size; c++) {
			tmp[c] = arr[c];
		}
		arr = tmp;
	}

	bool ComparePoints() {
		if (points.Length != pointsX.Length) return true;
		if (pointsX.Length == 0) return true;
		for (int i = 0; i < points.Length; i++) {
			if (points[i] != pointsX[i]) return true;
		}
		return false;
	}

	void MeshGen() {

		if(currentResolution != resolution) {
			currentResolution = resolution;
			verts = null;
			tris = null;
			colors = null;
			u = null;
		}

		if (Application.isPlaying && pointsX != null && ComparePoints() == false) {
			idle = true;
			return;
		} else {
			idle = false;
		}
		int l = points.Length+2;
		if (verts == null) verts = new Vector3[l];
		if (tris == null) tris = new int[l*3+3];
		if (colors == null) colors = new Color[l];
		if (verts.Length > 0) verts[0] = cacheTransform.InverseTransformPoint(cacheTransform.position);
		if (colors.Length > 0) colors[0] = vertexColor;
		
		for (int i = 0; i < points.Length +1; i++) {
			int i2 = i+1;
			verts[i2] = cacheTransform.InverseTransformPoint(points[i % points.Length]);
			if (enableVertexColors) {
				float s = 1f;
				float alpha = 1f;
				s = str[i % points.Length];
				if (vertexFade) {
					alpha = s;
				}
				if (enableOuterColor || ColorCycleOuterEnabled) {
						colors[i2] = new Color(
									((vertexColor.r * (s )) + (vertexColorOuter.r * (1 - s))/2),
									((vertexColor.g * (s )) + (vertexColorOuter.g * (1 - s))/2),
									((vertexColor.b * (s )) + (vertexColorOuter.b * (1 - s))/2),
									//((vertexColor.a * (s + 1)) + (vertexColorOuter.a * (1 - s))) * alpha
									vertexColor.a * alpha
						);
				} else {
					colors[i2] = new Color(vertexColor.r, vertexColor.g, vertexColor.b, vertexColor.a * alpha);
				}
			}
		}

		if (resolution != verts.Length) {
			lightMesh.Clear();
			int n = 0;
			if (degrees < 360)
				n = -2;
			int x = 0;
			for (var i = 0; i < verts.Length +n; i++) {
				tris[x] = (i + 1) % verts.Length;
				x++;
				tris[x] = i % verts.Length;
				x++;
				tris[x] = 0;
				x++;
			}
			lightMesh.vertices = verts;
			lightMesh.triangles = tris;
		} else {
			lightMesh.vertices = verts;
		}

		if (enableVertexColors) lightMesh.colors = colors;
		else lightMesh.colors = null;
		if (CreateUV) {
			if(u == null) u = new Vector2[verts.Length];
			float r = range * UVScale;
			for (int i = 0; i < verts.Length; i++) {
				float r2 = r * .5f;
				u[i] = new Vector2(verts[i].x / r2 + .5f, verts[i].y / r2 + .5f);
			}
			lightMesh.uv = u;
		} else {
			lightMesh.uv = null;
		}
		if (pointsX != null && pointsX.Length != resolution) return;
		cacheMeshFilter.mesh = lightMesh;
		int len = points.Length;
		if(pointsX == null) pointsX = new Vector3[len];
		System.Array.Copy(points, pointsX, len);

		PolygonCollider2D cachePolygonCollider = GetComponent<PolygonCollider2D>();
		if (cachePolygonCollider && cachePolygonCollider.enabled) {
			cachePolygonCollider.points = AsV2(points);
			//cachePolygonCollider.offset = -transform.position;
		}

	}

	public Vector2[] AsV2(Vector3[] src) {
		Vector2[] ret = new Vector2[ src.Length ];
		for (int i = 0; i < src.Length; i++) ret[i] = new Vector2(src[i].x-transform.position.x, src[i].y - transform.position.y);
		return ret;
	}

	public Color CombineColors(Color c1, Color c2) {
		Color result = new Color(0,0,0,0);
		result += c1;
		result += c2;
		result /= 2;
		return result;
	}

	public void ForceUpdate() {
		resolution++;
		Init();
		Brighten();	
		resolution--;
	}

	//Work in progress

	//void ScanPoints3() {
	//	float x = width / resolution;
	//	pointsP = new List<Vector3>();
	//	points = new List<Vector3>();
	//	str = new List<float>();
	//	bool hitSomething = false;
	//	int cap = (int)(resolution *.1f);
	//	Vector3 prevPos = cacheTransform.position;

	//	Vector3 inc = cacheTransform.right * x;
	//	Vector3 offset = inc *resolution *.5f;

	//	Vector3 up = cacheTransform.up;

	//	for (int i = 0; i < resolution; i++) {
	//		Vector3 pos = cacheTransform.position;
	//		pos += inc * i -offset;
	//		points.Add(pos);
	//		RaycastHit2D hit = Physics2D.Raycast(pos , up, range, layers);
	//		//points.Add(prevPos);

	//		if (hit) {
	//			//if (particles) pointsP.Add(hit.point);
	//			points.Add(hit.point);
	//			str.Add(1 - (hit.distance / range));
	//			hitSomething = true;
	//			prevPos = pos;
	//		} else {
	//			prevPos = pos;
	//			points.Add(pos + up * range);
	//			str.Add(0);
	//			hitSomething = false;
	//		}
	//		//if(i % 1 == 0) Debug.DrawLine(points[i], points[(i+1) %points.Count]);

	//		if (points.Count == 2) {
	//			//	cacheMeshFilter.mesh = lightMesh;
	//			if (points[0] == points[1]) {
	//				cacheMeshFilter.mesh = null;
	//				return;
	//			}
	//		}
	//	}
	//}


}
