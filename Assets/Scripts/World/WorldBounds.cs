using UnityEngine;

public class WorldBounds : MonoBehaviour
{
    [Header("Play area (world space)")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(70f, 24f, 70f);

    [Header("Walls")]
    public float wallThickness = 2f;
    public float wallHeight = 24f;
    public bool addCeiling = false;

    [Header("Fall guard")]
    public float fallY = -12f;            // respawn if the player drops below this Y
    public bool preferSpawnPoint = true;  // use a GameObject named "SpawnPoint" if present

    private Vector3 respawn;
    private bool hasRespawn;

    void Start()
    {
        BuildWalls();
        ResolveRespawn();
    }

    private void ResolveRespawn()
    {
        if (preferSpawnPoint)
        {
            GameObject sp = GameObject.Find("SpawnPoint");
            if (sp != null) { respawn = sp.transform.position + Vector3.up * 1.5f; hasRespawn = true; return; }
        }

        Transform player = GameFlowManager.FindPlayer();
        if (player != null) { respawn = player.position + Vector3.up * 1.0f; hasRespawn = true; return; }

        respawn = areaCenter + Vector3.up * 2f;
        hasRespawn = true;
    }

    void Update()
    {
        if (!hasRespawn) return;

        Transform player = GameFlowManager.FindPlayer();
        if (player == null) return;

        if (player.position.y < fallY)
            Teleport(player, respawn);
    }

    private static void Teleport(Transform player, Vector3 to)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = to;
        if (cc != null) cc.enabled = true;
        ScreenPrompt.Toast("You left the area — brought you back.", 2.5f);
    }

    private void BuildWalls()
    {
        Vector3 half = areaSize * 0.5f;
        float h = wallHeight;
        float cy = areaCenter.y + h * 0.5f - 2f;

        MakeWall("Wall_North", new Vector3(areaCenter.x, cy, areaCenter.z + half.z),
            new Vector3(areaSize.x + wallThickness * 2f, h, wallThickness));
        MakeWall("Wall_South", new Vector3(areaCenter.x, cy, areaCenter.z - half.z),
            new Vector3(areaSize.x + wallThickness * 2f, h, wallThickness));
        MakeWall("Wall_East", new Vector3(areaCenter.x + half.x, cy, areaCenter.z),
            new Vector3(wallThickness, h, areaSize.z + wallThickness * 2f));
        MakeWall("Wall_West", new Vector3(areaCenter.x - half.x, cy, areaCenter.z),
            new Vector3(wallThickness, h, areaSize.z + wallThickness * 2f));

        if (addCeiling)
            MakeWall("Ceiling", new Vector3(areaCenter.x, areaCenter.y + h, areaCenter.z),
                new Vector3(areaSize.x, wallThickness, areaSize.z));
    }

    private void MakeWall(string wallName, Vector3 pos, Vector3 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform, false);
        wall.transform.position = pos;
        BoxCollider box = wall.AddComponent<BoxCollider>();
        box.size = size;   // collider only, no renderer
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireCube(areaCenter + Vector3.up * (wallHeight * 0.5f - 2f),
            new Vector3(areaSize.x, wallHeight, areaSize.z));
        Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.6f);
        Gizmos.DrawWireCube(new Vector3(areaCenter.x, fallY, areaCenter.z),
            new Vector3(areaSize.x, 0.1f, areaSize.z));
    }
}
