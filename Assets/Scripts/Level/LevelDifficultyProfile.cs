using UnityEngine;

namespace ITWaves.Level
{
    /// <summary>
    /// Defines difficulty scaling across all 20 levels.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDifficultyProfile", menuName = "ITWaves/Level Difficulty Profile")]
    public class LevelDifficultyProfile : ScriptableObject
    {
        [Header("Snake Difficulty")]
        [SerializeField, Tooltip("Base segment count at level 1.")]
        public int snakeBaseSegments = 10;

        [SerializeField, Tooltip("Additional segments every N levels.")]
        public int snakeSegmentsPerLevels = 3;

        [SerializeField, Tooltip("Snake head speed curve (level 1-20).")]
        public AnimationCurve snakeSpeedCurve = AnimationCurve.Linear(1f, 3f, 20f, 6f);
        
        [Header("Enemy Difficulty")]
        [SerializeField, Tooltip("Enemy spawn rate curve (enemies per second).")]
        public AnimationCurve enemySpawnRateCurve = AnimationCurve.Linear(1f, 0.4f, 20f, 1.2f);
        
        [SerializeField, Tooltip("Maximum active enemies curve.")]
        public AnimationCurve enemyCapCurve = AnimationCurve.Linear(1f, 3f, 20f, 12f);
        
        [SerializeField, Tooltip("Crawler weight curve (0-1).")]
        public AnimationCurve crawlerWeightCurve = AnimationCurve.Linear(1f, 1f, 20f, 0.6f);
        
        [SerializeField, Tooltip("Skitterer weight curve (0-1).")]
        public AnimationCurve skittererWeightCurve = AnimationCurve.Linear(1f, 0f, 20f, 0.4f);
        
        [Header("Props Difficulty")]
        [SerializeField, Tooltip("Box density curve (boxes per 100 sq units).")]
        public AnimationCurve boxDensityCurve = AnimationCurve.Linear(1f, 2f, 20f, 8f);
        
        [SerializeField, Tooltip("Chance to spawn box when snake is hit.")]
        public AnimationCurve boxSpawnOnHitCurve = AnimationCurve.Linear(1f, 0.1f, 20f, 0.3f);
        
        [SerializeField, Tooltip("Box health (hits to destroy).")]
        public int boxHealth = 3;

        /// <summary>
        /// Get snake segment count for a given level.
        /// </summary>
        public int GetSnakeSegments(int level)
        {
            return snakeBaseSegments + (level - 1) / snakeSegmentsPerLevels;
        }

        /// <summary>
        /// Get snake speed for a given level.
        /// </summary>
        public float GetSnakeSpeed(int level)
        {
            return snakeSpeedCurve.Evaluate(level);
        }
        
        /// <summary>
        /// Get enemy spawn rate for a given level.
        /// </summary>
        public float GetEnemySpawnRate(int level)
        {
            return enemySpawnRateCurve.Evaluate(level);
        }
        
        /// <summary>
        /// Get maximum active enemies for a given level.
        /// </summary>
        public int GetEnemyCap(int level)
        {
            return Mathf.RoundToInt(enemyCapCurve.Evaluate(level));
        }
        
        /// <summary>
        /// Get box density for a given level.
        /// </summary>
        public float GetBoxDensity(int level)
        {
            return boxDensityCurve.Evaluate(level);
        }
        
        /// <summary>
        /// Get box spawn chance on snake hit for a given level.
        /// </summary>
        public float GetBoxSpawnChance(int level)
        {
            return boxSpawnOnHitCurve.Evaluate(level);
        }
        
        /// <summary>
        /// Get enemy type weights for a given level.
        /// </summary>
        public void GetEnemyWeights(int level, out float crawlerWeight, out float skittererWeight)
        {
            crawlerWeight = crawlerWeightCurve.Evaluate(level);
            skittererWeight = skittererWeightCurve.Evaluate(level);
        }
    }
}

