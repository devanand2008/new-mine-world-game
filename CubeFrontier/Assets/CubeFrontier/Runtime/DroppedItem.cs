using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    /// <summary>Dropped item entity in the world. Floats, bobs, and can be picked up.</summary>
    public sealed class DroppedItem : MonoBehaviour
    {
        public int ItemId;
        public int Count = 1;
        public int Durability = -1;
        public GameSession Session;
        private float spawnTime;
        private float bobPhase;
        private Transform visual;
        private static Material glowMaterial;

        public static DroppedItem Spawn(GameSession session, Vector3 position, int itemId, int count, int durability = -1)
        {
            var obj = new GameObject("Dropped " + Items.Name(itemId));
            obj.transform.SetParent(session.World.transform,false);
            obj.transform.position = position + Vector3.up * .5f;
            var dropped = obj.AddComponent<DroppedItem>();
            dropped.ItemId = itemId;
            dropped.Count = count;
            dropped.Durability = durability;
            dropped.Session = session;
            return dropped;
        }

        private void Start()
        {
            spawnTime = Time.time;
            bobPhase = Random.value * Mathf.PI * 2;
            gameObject.layer = 2; // Ignore Raycast

            // Build visual
            if (Blocks.Placeable(ItemId))
            {
                var mesh = HeldItem.BlockMesh((Block)ItemId, .18f);
                var child = new GameObject("Block visual");
                child.transform.SetParent(transform, false);
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                child.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>(
                    Blocks.Transparent((Block)ItemId) ? "FrontierGlass" : "FrontierVoxel");
                child.AddComponent<OwnedArtMesh>().Mesh = mesh;
                visual = child.transform;
            }
            else
            {
                var art = new ArtMesh();
                Color color = Items.Tool(ItemId) ? ReferenceCharacters.Hex("75DBE4")
                    : Items.Armor(ItemId) ? ReferenceCharacters.Hex("AAB9BD")
                    : ReferenceCharacters.Hex("D8A24F");
                art.Box(Vector3.zero, Vector3.one * .15f, color, default(Vector3), Items.Weapon(ItemId));
                visual = art.Build(transform, "Item visual");
            }

            // Add a trigger collider for pickup
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = .6f;
            sphere.isTrigger = true;
        }

        private void Update()
        {
            if (!Session || Session.Paused) return;

            // Despawn after 5 minutes
            if (Time.time - spawnTime > 300) { Destroy(gameObject); return; }

            // Bobbing animation
            if (visual)
            {
                float bob = Mathf.Sin((Time.time + bobPhase) * 2f) * .08f;
                visual.localPosition = Vector3.up * bob;
                visual.Rotate(0, Time.deltaTime * 60, 0);
            }

            // Gravity
            if (Session.World != null)
            {
                Vector3 pos = transform.position;
                Block below = Session.World.Data.Get(
                    Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y - .3f), Mathf.FloorToInt(pos.z));
                if (!Blocks.Solid(below) && pos.y > 0)
                    transform.position += Vector3.down * 4 * Time.deltaTime;
            }

            // Auto-pickup when player is near
            if (Session.Player != null)
            {
                float dist = Vector3.Distance(transform.position, Session.Player.transform.position);
                if (dist < 1.5f)
                {
                    if (Session.Player.Inventory.Add(ItemId, Count, Durability))
                    {
                        Session.Tone(420);
                        Session.Notify("Picked up " + Count + " " + Items.Name(ItemId));
                        Destroy(gameObject);
                    }
                }
                else if (dist < 3f)
                {
                    // Magnetic attraction
                    Vector3 dir = (Session.Player.transform.position - transform.position).normalized;
                    transform.position += dir * 3 * Time.deltaTime;
                }
            }
        }
    }
}
