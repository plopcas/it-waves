using UnityEngine;

namespace ITWaves.Snake
{
    /// <summary>
    /// Configuration for snake behaviour and appearance.
    /// </summary>
    [CreateAssetMenu(fileName = "SnakeConfig", menuName = "ITWaves/Snake Config")]
    public class SnakeConfig : ScriptableObject
    {
        [Header("Segments")]
        [SerializeField, Tooltip("Segment prefab.")]
        public GameObject segmentPrefab;
        
        [SerializeField, Tooltip("Base segment count.")]
        public int baseSegmentCount = 10;
        
        [SerializeField, Tooltip("Spacing between segments.")]
        public float segmentSpacing = 0.5f;
        
        [Header("Wave Motion")]
        [SerializeField, Tooltip("Base wave amplitude.")]
        public float baseAmplitude = 0.8f;
        
        [SerializeField, Tooltip("Base wave frequency.")]
        public float baseFrequency = 1f;
        
        [SerializeField, Tooltip("Base head speed.")]
        public float baseSpeed = 3f;
        
        [SerializeField, Tooltip("Turn rate (degrees per second).")]
        public float turnRate = 90f;

        [Header("Grid Movement")]
        [SerializeField, Tooltip("Minimum steps before randomly changing direction (zig-zag).")]
        public int minStepsBeforeRandomZag = 3;

        [SerializeField, Tooltip("Maximum steps before randomly changing direction (zig-zag).")]
        public int maxStepsBeforeRandomZag = 8;

        [SerializeField, Tooltip("Steps to wait before spawning head after level start.")]
        public int minStepsBeforeSpawn = 1;

        [SerializeField, Tooltip("Maximum steps to wait before spawning head.")]
        public int maxStepsBeforeSpawn = 5;

        [SerializeField, Tooltip("Speed multiplier when head retreats after all segments destroyed.")]
        public float retreatSpeedMultiplier = 3f;
    }
}

