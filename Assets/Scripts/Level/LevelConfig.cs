using UnityEngine;

namespace ITWaves.Level
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "ITWaves/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Grid Settings")]
        [SerializeField, Tooltip("Grid cell size (1x1 units for all entities).")]
        public float gridCellSize = 1f;

        [SerializeField, Tooltip("Margin around the grid (in world units).")]
        public float gridMargin = 0.5f;

        [Header("Arena Settings (Deprecated - use GridManager instead)")]
        [SerializeField, Tooltip("Arena width in world units.")]
        public float arenaWidth = 32f;

        [SerializeField, Tooltip("Arena height in world units.")]
        public float arenaHeight = 18f;

        [SerializeField, Tooltip("Safe radius around player start where nothing spawns (in grid cells).")]
        public float playerSafeRadius = 4f;

        [Header("Procedural Generation")]
        [SerializeField, Tooltip("Random seed for this level (0 = use level index).")]
        public int seed = 0;

        [SerializeField, Tooltip("Enable procedural generation.")]
        public bool useProcedural = true;
    }
}

