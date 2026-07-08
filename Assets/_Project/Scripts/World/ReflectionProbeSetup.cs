using UnityEngine;
using UnityEngine.Rendering;

namespace DreamCore.World
{
    /// <summary>
    /// Configures the corridor's reflection probe: one realtime probe rendered
    /// once on awake, box-projected so the neon and clouds land believably on
    /// the water and chrome.
    /// </summary>
    [RequireComponent(typeof(ReflectionProbe))]
    public class ReflectionProbeSetup : MonoBehaviour
    {
        [SerializeField, Tooltip("Extra probe refreshes during the first seconds (lets emissives settle).")]
        private int warmupRenders = 2;

        private ReflectionProbe probe;

        public static ReflectionProbeSetup Create(Transform parent, Vector3 center, Vector3 size)
        {
            var go = new GameObject("ReflectionProbe_Water");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;

            var probe = go.AddComponent<ReflectionProbe>();
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
            probe.boxProjection = true;
            probe.size = size;
            probe.resolution = 128;
            probe.hdr = true;
            probe.importance = 1;
            probe.intensity = 1f;

            return go.AddComponent<ReflectionProbeSetup>();
        }

        private void Awake()
        {
            probe = GetComponent<ReflectionProbe>();
        }

        private void Start()
        {
            if (Application.isPlaying && probe != null && warmupRenders > 0)
            {
                StartCoroutine(Warmup());
            }
        }

        private System.Collections.IEnumerator Warmup()
        {
            for (int i = 0; i < warmupRenders; i++)
            {
                yield return new WaitForSeconds(0.5f * (i + 1));
                probe.RenderProbe();
            }
        }
    }
}
