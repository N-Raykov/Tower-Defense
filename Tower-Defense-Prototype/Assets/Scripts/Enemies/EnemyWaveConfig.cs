// ScriptableObject describing a single enemy wave configuration.
// Editable in the Inspector without changing code.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Enemies
{
    // One instruction in a wave: spawn a given enemy prefab multiple times
    // with a delay between each spawn.

    [Serializable]
    public class EnemySpawnInstruction
    {
        [Tooltip("Enemy prefab to spawn.")]
        public GameObject enemyPrefab;

        [Tooltip("How many of this enemy to spawn.")]
        public int count = 1;

        [Tooltip("Delay between each spawn of this enemy type.")]
        public float delayBetweenSpawns = 0.5f;

        [Tooltip("Extra delay after finishing this group before the next group starts.")]
        public float delayAfterGroup = 0f;
    }

    // Configuration asset for a single wave of enemies.

    [CreateAssetMenu(
        fileName = "EnemyWaveConfig",
        menuName = "TowerDefense/Enemy Wave Config")]

    public class EnemyWaveConfig : ScriptableObject
    {
        [Tooltip("Optional delay before this wave starts (seconds).")]
        public float initialDelay = 0f;

        [Tooltip("Sequence of spawn instructions for this wave.")]
        public List<EnemySpawnInstruction> spawnInstructions =
            new List<EnemySpawnInstruction>();
    }
}

