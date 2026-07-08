using UnityEngine;

namespace DreamCore.World
{
    /// <summary>
    /// Small ambient motion for lightweight props that should feel suspended in
    /// the humid dream corridor. It is optional and safe on any decorative object.
    /// </summary>
    public class FloatingObject : MonoBehaviour
    {
        [SerializeField, Tooltip("Vertical bob amplitude in meters.")]
        private float bobAmplitude = 0.08f;
        [SerializeField, Tooltip("Seconds per full bob cycle.")]
        private float bobPeriod = 3.5f;
        [SerializeField, Tooltip("Degrees per second around local up.")]
        private float yawSpeed = 4f;
        [SerializeField, Tooltip("Randomize the phase slightly on start.")]
        private bool randomizePhase = true;

        private Vector3 origin;
        private float phase;

        private void Awake()
        {
            origin = transform.localPosition;
            if (randomizePhase)
            {
                phase = Random.value * Mathf.PI * 2f;
            }
        }

        private void OnEnable()
        {
            origin = transform.localPosition;
        }

        private void Update()
        {
            float safePeriod = Mathf.Max(0.01f, bobPeriod);
            float t = phase + Time.time * Mathf.PI * 2f / safePeriod;

            var p = origin;
            p.y += Mathf.Sin(t) * bobAmplitude;
            transform.localPosition = p;

            if (!Mathf.Approximately(yawSpeed, 0f))
            {
                transform.Rotate(Vector3.up, yawSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}
