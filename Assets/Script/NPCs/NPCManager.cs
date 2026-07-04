using UnityEngine;
using UnityEngine.AI;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

using UnityEngine.InputSystem;

#endif

public class NPCManager : MonoBehaviour
{
    [Header("Prefabs Settings")]
    [Tooltip("A civilian prefab with the CitizenNPC script attached.")]
    public GameObject citizenPrefab;

    [Tooltip("Assassin prefab with the AssassinNPC script attached")]
    public GameObject assassinPrefab;

    [Header("Game Settings")]
    public Transform vipTransform;

    public int npcCount = 30;
    public float spawnRadius = 8f;

    public bool enableClickToSpawnSuspect = true;
    public bool enableClickToInteractSuspect = true;
    public LayerMask groundLayers = ~0;
    public float maxSampleDistance = 1.5f;

    private NPCBase cachedSuspect;

    private void Start()
    {
        if (vipTransform == null || citizenPrefab == null || assassinPrefab == null) return;
        SpawnCrowd();
    }

    private void Update()
    {
        if (!Application.isPlaying) return;

        bool click = false;
        Vector2 screenPos = default;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            click = true;
            screenPos = Mouse.current.position.ReadValue();
        }
#else
        if ((enableClickToSpawnSuspect || enableClickToInteractSuspect) && Input.GetMouseButtonDown(0))
        {
            click = true;
            screenPos = Input.mousePosition;
        }
#endif

        if (click)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPos);

            if (enableClickToInteractSuspect)
            {
                if (Physics.Raycast(ray, out RaycastHit npcHit, 100f))
                {
                    NPCBase hitNpc = npcHit.collider.GetComponentInParent<NPCBase>();
                    if (hitNpc != null)
                    {
                        hitNpc.OnInteracted();
                        Debug.Log($"[Interaction] Clicked on an NPC; is this person a suspect?: {hitNpc.NPCType.Equals(NPCType.Assassin)}");
                        return;
                    }
                }
            }

            if (enableClickToSpawnSuspect)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayers))
                {
                    Vector3 clickPoint = hit.point;
                    if (NavMesh.SamplePosition(clickPoint, out NavMeshHit navHit, maxSampleDistance, NavMesh.AllAreas))
                    {
                        SpawnSuspectAt(navHit.position);
                    }
                }
            }
        }
    }

    private void SpawnCrowd()
    {
        int suspectIndex = Random.Range(0, npcCount);

        for (int i = 0; i < npcCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPos = new Vector3(
                vipTransform.position.x + randomCircle.x + Random.Range(-1f, 1f),
                vipTransform.position.y,
                vipTransform.position.z + randomCircle.y + Random.Range(-1f, 1f)
            );

            bool isSuspect = (i == suspectIndex);

            GameObject targetPrefab = isSuspect ? assassinPrefab : citizenPrefab;
            GameObject npcGo = Instantiate(targetPrefab, spawnPos, Quaternion.identity);

            NPCBase npcScript = npcGo.GetComponent<NPCBase>();
            if (npcScript != null)
            {
                npcScript.Initialize();
            }

            if (isSuspect)
            {
                cachedSuspect = npcScript;
                var rend = npcGo.GetComponentInChildren<Renderer>();
                if (rend != null) rend.material.color = Color.yellow;
                Debug.Log($"[System Generated] The suspect (using the Assassin Prefab) has blended into the crowd; Index:{i}");
            }
        }
    }

    public void SpawnSuspectAt(Vector3 position)
    {
        if (assassinPrefab == null || vipTransform == null) return;

        if (cachedSuspect != null)
        {
            Debug.Log($"[Cleanup] Removed the previously manually generated suspect: {cachedSuspect.name}");
            Destroy(cachedSuspect.gameObject);
        }

        GameObject npcGo = Instantiate(assassinPrefab, position, Quaternion.identity);
        NPCBase npcScript = npcGo.GetComponent<NPCBase>();
        if (npcScript == null) return;

        npcScript.Initialize();
        npcScript.SetNavMeshTarget(vipTransform);
        cachedSuspect = npcScript;

        Debug.Log($"[Manually Generated] New Assassin successfully created, location: {position}");
    }

    public void CatchSuspectInEditor()
    {
        if (cachedSuspect != null)
        {
            cachedSuspect.OnInteracted();
        }
        else
        {
            Debug.LogWarning("The suspect has not yet been generated in the scene, or the game is not running!");
        }
    }
}