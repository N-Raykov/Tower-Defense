// Moves an enemy along a sequence of waypoints provided by PathFromTilemap

using System;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Level;

namespace TowerDefense.Enemies
{
    // Component that moves its GameObject along a list of waypoints.
    [RequireComponent(typeof(Collider2D))]
    public class EnemyMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float waypointReachThreshold = 0.05f;

        [Tooltip("Rotation offset in degrees so the sprite visually faces the movement direction.")]
        [SerializeField] private float rotationOffset = 0f;

        [Header("Path Source")]
        [SerializeField] private PathFromTilemap pathProvider;

        private IReadOnlyList<Vector3> waypoints;
        private int currentWaypointIndex;
        private bool hasPath;

        public event Action<EnemyMover> OnPathCompleted;

        private void Start()
        {
            if (pathProvider == null)
            {
                pathProvider = FindObjectOfType<PathFromTilemap>();
            }

            if (pathProvider == null)
            {
                Debug.LogError("EnemyMover: No PathFromTilemap found in the scene.");
                return;
            }

            waypoints = pathProvider.Waypoints;
            if (waypoints == null || waypoints.Count == 0)
            {
                Debug.LogError("EnemyMover: Path provider has no waypoints.");
                return;
            }

            hasPath = true;
            currentWaypointIndex = 0;

            transform.position = waypoints[0];
        }

        private void Update()
        {
            if (!hasPath || currentWaypointIndex >= waypoints.Count)
            {
                return;
            }

            Vector3 target = waypoints[currentWaypointIndex];
            Vector3 position = transform.position;

            Vector3 toTarget = target - position;
            Vector3 direction = toTarget.normalized;
            float distanceThisFrame = moveSpeed * Time.deltaTime;

            // Rotate to face the movement direction (only if we have a non-zero direction)
            if (direction.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
            }

            // If we are very close, snap and move to the next waypoint
            if (toTarget.magnitude <= waypointReachThreshold)
            {
                transform.position = target;
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Count)
                {
                    hasPath = false;
                    OnPathCompleted?.Invoke(this);
                }

                return;
            }

            transform.position = position + direction * distanceThisFrame;

        }
    }
}

