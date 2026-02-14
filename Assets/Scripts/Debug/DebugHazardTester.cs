#if DEVELOPMENT_BUILD || UNITY_EDITOR
using MetalPod.Hazards;
using MetalPod.Shared;
using UnityEngine;

namespace MetalPod.Debugging
{
    /// <summary>
    /// Development panel for spawning and cleaning hazards while playtesting.
    /// </summary>
    public class DebugHazardTester : MonoBehaviour
    {
        [Header("Hazard Prefabs")]
        [SerializeField] private GameObject[] hazardPrefabs;

        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistance = 15f;
        [SerializeField] private float spawnHeight = 2f;
        [SerializeField] private bool replaceActiveHazard = true;

        private bool _isVisible;
        private Vector2 _scrollPosition;
        private Rect _windowRect = new Rect(12f, 460f, 300f, 320f);
        private GameObject _currentHazard;

        public bool IsVisible => _isVisible;

        public void Toggle() => _isVisible = !_isVisible;
        public void Show() => _isVisible = true;
        public void Hide() => _isVisible = false;

        private void OnGUI()
        {
            if (!_isVisible)
            {
                return;
            }

            _windowRect = GUI.Window(8122, _windowRect, DrawWindow, "Hazard Tester");
        }

        private void DrawWindow(int _)
        {
            GUILayout.BeginVertical();
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            if (hazardPrefabs == null || hazardPrefabs.Length == 0)
            {
                GUILayout.Label("No hazard prefabs assigned.");
                GUILayout.Label("Assign prefabs in the inspector.");
            }
            else
            {
                for (int i = 0; i < hazardPrefabs.Length; i++)
                {
                    GameObject prefab = hazardPrefabs[i];
                    if (prefab == null)
                    {
                        continue;
                    }

                    if (GUILayout.Button($"Spawn: {prefab.name}"))
                    {
                        SpawnHazard(prefab);
                    }
                }
            }

            GUILayout.Space(10f);

            if (_currentHazard != null)
            {
                GUILayout.Label($"Active: {_currentHazard.name}");
                if (GUILayout.Button("Destroy Current"))
                {
                    Destroy(_currentHazard);
                    _currentHazard = null;
                }
            }

            if (GUILayout.Button("Destroy All Hazards"))
            {
                int removed = DestroyAllHazards();
                DebugConsole.Instance?.Log($"Destroyed {removed} hazards.");
            }

            GUILayout.EndScrollView();

            if (GUILayout.Button("Close"))
            {
                Hide();
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 22f));
        }

        private void SpawnHazard(GameObject prefab)
        {
            if (prefab == null)
            {
                return;
            }

            if (replaceActiveHazard && _currentHazard != null)
            {
                Destroy(_currentHazard);
                _currentHazard = null;
            }

            Vector3 spawnPosition = ResolveSpawnPosition();
            Quaternion spawnRotation = Quaternion.identity;

            GameObject player = GameObject.FindGameObjectWithTag(GameConstants.TAG_PLAYER);
            if (player != null)
            {
                spawnRotation = Quaternion.LookRotation(player.transform.forward, Vector3.up);
            }

            _currentHazard = Instantiate(prefab, spawnPosition, spawnRotation);
            DebugConsole.Instance?.Log($"Spawned hazard '{prefab.name}' at {spawnPosition}.");
        }

        private Vector3 ResolveSpawnPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag(GameConstants.TAG_PLAYER);
            if (player != null)
            {
                return player.transform.position +
                       (player.transform.forward * spawnDistance) +
                       (Vector3.up * spawnHeight);
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                return camera.transform.position +
                       (camera.transform.forward * spawnDistance) +
                       (Vector3.up * spawnHeight * 0.5f);
            }

            return Vector3.up * spawnHeight;
        }

        private int DestroyAllHazards()
        {
            HazardBase[] hazards = FindObjectsOfType<HazardBase>();
            for (int i = 0; i < hazards.Length; i++)
            {
                if (hazards[i] != null)
                {
                    Destroy(hazards[i].gameObject);
                }
            }

            _currentHazard = null;
            return hazards.Length;
        }
    }
}
#endif
