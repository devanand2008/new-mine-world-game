using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    public enum CreatureKind { Stalker, Brute, Bat, Warden, MireStalker, AshHound, ShadowArcher }
    public enum CreatureState { Patrol, Investigate, Pursue, Attack, Retreat, Return, Defeated }
    [RequireComponent(typeof(CharacterController))]
    public sealed class Creature : MonoBehaviour
    {
        public CreatureKind Kind;
        public CreatureState State;
        public GameSession Session;
        public string DisplayName { get { return (int)Kind<=4 ? ReferenceCreatures.Names[(int)Kind] : Kind==CreatureKind.AshHound?"Ash Hound":"Shadow Archer"; } }
        private CharacterController controller;
        private Transform visual;
        private Vector3 home, destination, knockback;
        private float health, maxHealth, attackAt, thinkAt, lastSeen, gravity, deathAt;
        private float rangedCooldown;
        private float strikeAt;
        private bool windingUp;
        private void Start()
        {
            gameObject.layer = 8; controller = GetComponent<CharacterController>();
            float h = Kind == CreatureKind.Bat ? .7f : Kind == CreatureKind.AshHound ? 1.2f : 2.4f;
            float r = Kind == CreatureKind.Brute ? .55f : Kind == CreatureKind.AshHound ? .4f : .35f;
            controller.height = h; controller.radius = r;
            controller.center = Vector3.up * controller.height * .5f; controller.stepOffset = .6f;
            home = transform.position; destination = home;
            switch (Kind)
            {
                case CreatureKind.Warden: maxHealth = 180; break;
                case CreatureKind.Brute: maxHealth = 100; break;
                case CreatureKind.Bat: maxHealth = 20; break;
                case CreatureKind.MireStalker: maxHealth = 55; break;
                case CreatureKind.AshHound: maxHealth = 40; break;
                case CreatureKind.ShadowArcher: maxHealth = 35; break;
                default: maxHealth = 45; break;
            }
            health = maxHealth;
            // Use existing model for known kinds, generate placeholder for new ones
            if ((int)Kind <= 4)
                visual = VoxelArt.Monster(transform, Kind);
            else
                visual = CreatePlaceholder();
            lastSeen = -100;
        }
        private Transform CreatePlaceholder()
        {
            var art = new ArtMesh();
            Color body = Kind == CreatureKind.MireStalker ? ReferenceCharacters.Hex("3A5533")
                : Kind == CreatureKind.AshHound ? ReferenceCharacters.Hex("6B3525")
                : ReferenceCharacters.Hex("2B2540");
            Color glow = Kind == CreatureKind.MireStalker ? ReferenceCharacters.Hex("A8D845")
                : Kind == CreatureKind.AshHound ? ReferenceCharacters.Hex("FF6830")
                : ReferenceCharacters.Hex("9B55FF");
            float scale = Kind == CreatureKind.AshHound ? .7f : 1f;
            art.Box(Vector3.up * 1.2f * scale, new Vector3(.5f, .7f, .3f) * scale, body);
            art.Box(Vector3.up * 1.8f * scale, new Vector3(.35f, .35f, .3f) * scale, body);
            // Eyes
            for (int s = -1; s <= 1; s += 2)
                art.Box(new Vector3(s * .1f, 1.85f * scale, .17f * scale), new Vector3(.06f, .04f, .02f) * scale, glow, default(Vector3), true);
            // Legs
            for (int s = -1; s <= 1; s += 2)
            {
                art.Box(new Vector3(s * .15f * scale, .5f * scale, 0), new Vector3(.12f, .8f, .15f) * scale, body * .8f);
                art.Box(new Vector3(s * .15f * scale, .07f * scale, .03f), new Vector3(.14f, .12f, .2f) * scale, body * .6f);
            }
            // Unique features
            if (Kind == CreatureKind.MireStalker)
            {
                for (int i = 0; i < 5; i++)
                    art.Box(new Vector3((i - 2) * .08f, 2.0f * scale + i * .05f, -.1f), new Vector3(.04f, .2f, .04f) * scale, ReferenceCharacters.Hex("5C7D3A"), new Vector3(0, 0, (i - 2) * 12));
            }
            else if (Kind == CreatureKind.AshHound)
            {
                art.Box(new Vector3(0, .9f * scale, -.25f * scale), new Vector3(.2f, .15f, .4f) * scale, body * .8f); // tail
                for (int s = -1; s <= 1; s += 2)
                    art.Crystal(new Vector3(s * .12f, 1.6f * scale, 0), .04f, .15f, glow); // ears
            }
            else if (Kind == CreatureKind.ShadowArcher)
            {
                art.Bar(new Vector3(.3f * scale, 1.3f * scale, .1f), new Vector3(.3f * scale, .6f * scale, .15f), .04f, body * 1.3f); // bow
                art.Crystal(new Vector3(.3f * scale, 1.4f * scale, .1f), .06f, .3f, glow); // arrow tip
            }
            return art.Build(transform, Kind.ToString() + " model");
        }
        private void Update()
        {
            if (!Session || Session.Paused || !Session.Player || !controller) return;
            if (State == CreatureState.Defeated) {
                visual.localScale = Vector3.one * Mathf.Max(0, 1 - (Time.time - deathAt) * 2);
                if (Time.time - deathAt > .5f) Destroy(gameObject); return;
            }
            float distance = Vector3.Distance(transform.position, Session.Player.transform.position);
            if (distance > 70) { Destroy(gameObject); return; }
            if (Time.time >= thinkAt) { thinkAt = Time.time + (distance > 35 ? 1.2f : .25f); Think(distance); }
            Vector3 delta = destination - transform.position; delta.y = 0;
            float speed = MoveSpeed();
            Vector3 direction = delta.sqrMagnitude > 1 ? delta.normalized : Vector3.zero;
            if ((State == CreatureState.Attack && Kind != CreatureKind.ShadowArcher)||windingUp) direction = Vector3.zero;
            if (direction != Vector3.zero) {
                Vector3 ahead = transform.position + direction * 1.1f;
                if (Kind != CreatureKind.Bat && Kind != CreatureKind.Warden) {
                    if (Physics.Raycast(transform.position + Vector3.up * .8f, direction, .8f, 1 << 0)) {
                        // Bounded local obstacle steering; not a general cave pathfinder.
                        Vector3 left = Quaternion.Euler(0, 75, 0) * direction;
                        direction = Physics.Raycast(transform.position + Vector3.up, left, 1.2f, 1 << 0) ? -left : left;
                    }
                    if (!Physics.Raycast(ahead + Vector3.up * .5f, Vector3.down, 3.0f, 1 << 0)) direction = Vector3.zero;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(delta), Time.deltaTime * 5);
            }
            if (!Session.World.HasColliderAt(transform.position + direction)) return;
            if (Kind == CreatureKind.Bat || Kind == CreatureKind.Warden) {
                float ground = Session.World.Data.Terrain.Surface(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z)) + (Kind == CreatureKind.Bat ? 3 : 1);
                gravity = Mathf.Clamp((ground - transform.position.y) * 2, -3, 3);
            } else { if (controller.isGrounded) gravity = -2; else gravity = Mathf.Max(-25, gravity - 20 * Time.deltaTime); }
            controller.Move((direction * speed + knockback + Vector3.up * gravity) * Time.deltaTime); knockback = Vector3.Lerp(knockback, Vector3.zero, Time.deltaTime * 8);
            visual.localPosition = Vector3.up * (Kind == CreatureKind.Warden || Kind == CreatureKind.Bat ? Mathf.Sin(Time.time * 2) * .12f : Mathf.Sin(Time.time * 7) * direction.magnitude * .04f);

            // A visible wind-up lets the player retreat. Resolve against current range and walls.
            if(windingUp) {
                visual.localRotation=Quaternion.Euler(-25*Mathf.Clamp01((strikeAt-Time.time)/.55f),0,0);
                if(Time.time>=strikeAt) {
                    windingUp=false;visual.localRotation=Quaternion.identity;
                    Vector3 eye=transform.position+Vector3.up, target=Session.Player.transform.position+Vector3.up;
                    if(Vector3.Distance(transform.position,Session.Player.transform.position)<2.8f&&!Physics.Linecast(eye,target,1<<0)) {
                        float dmg=Kind==CreatureKind.Warden?16:Kind==CreatureKind.Brute?20:Kind==CreatureKind.AshHound?12:8;
                        Session.Player.Damage(dmg*(Session.IsNight?1.25f:1));
                    }
                }
            }
            if (!windingUp && State == CreatureState.Attack && Kind != CreatureKind.ShadowArcher && Time.time >= attackAt && distance < 2.8f) {
                float cd = Kind == CreatureKind.Brute ? 1.8f : Kind == CreatureKind.AshHound ? .8f : 1.2f;
                strikeAt=Time.time+.55f;attackAt=strikeAt+cd;windingUp=true;Session.Tone(130);
            }

            // Shadow Archer ranged attack
            if (Kind == CreatureKind.ShadowArcher && State == CreatureState.Attack && distance < 18 && distance > 3 && Time.time >= rangedCooldown)
            {
                rangedCooldown = Time.time + 2.5f;
                ShootArrow();
            }
        }
        private float MoveSpeed()
        {
            switch (Kind)
            {
                case CreatureKind.Brute: return 1.45f;
                case CreatureKind.Bat: return 4.4f;
                case CreatureKind.Warden: return 2.7f;
                case CreatureKind.AshHound: return 4.0f;
                case CreatureKind.MireStalker: return 2.0f;
                case CreatureKind.ShadowArcher: return 2.2f;
                default: return 2.5f;
            }
        }
        private void ShootArrow()
        {
            if (!Session.Player) return;
            Vector3 dir = (Session.Player.transform.position + Vector3.up - transform.position).normalized;
            var arrowObj = new GameObject("Shadow arrow");
            arrowObj.transform.position = transform.position + Vector3.up * 1.5f + dir;
            var arrow = arrowObj.AddComponent<Projectile>();
            arrow.Direction = dir;
            arrow.Speed = 12;
            arrow.Damage = Session.IsNight?7.5f:6;
            arrow.Session = Session;
            arrow.IsPlayerProjectile = false;
            Session.Tone(300);
        }
        private void Think(float distance)
        {
            Vector3 target = Session.Player.transform.position;
            Vector3 eye = transform.position + Vector3.up * 1.5f, toPlayer = target + Vector3.up - eye;
            float detectRange = Kind == CreatureKind.Warden ? 32 : Kind == CreatureKind.ShadowArcher ? 24 : Kind == CreatureKind.AshHound ? 20 : 18;
            bool sees = distance < detectRange && !Physics.Raycast(eye, toPlayer.normalized, toPlayer.magnitude, 1 << 0);
            if (Session.Mode == GameMode.Creative) sees = false;
            if (sees) {
                lastSeen = Time.time; destination = target;
                bool shouldRetreat = health < maxHealth * .18f && Kind != CreatureKind.Warden;
                float attackRange = Kind == CreatureKind.ShadowArcher ? 15 : 2.5f;
                State = shouldRetreat ? CreatureState.Retreat : distance < attackRange ? CreatureState.Attack : CreatureState.Pursue;
            }
            else if (Time.time - lastSeen < 5) State = CreatureState.Investigate;
            else if (Vector3.Distance(transform.position, home) > 15) { State = CreatureState.Return; destination = home; }
            else { State = CreatureState.Patrol; if (Vector3.Distance(transform.position, destination) < 2) destination = home + new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f)); }
            if (State == CreatureState.Retreat) destination = transform.position + (transform.position - target).normalized * 8;
        }
        public void Damage(float amount, Vector3 attacker)
        {
            if (State == CreatureState.Defeated) return;
            health -= amount; lastSeen = Time.time; destination = attacker; knockback = (transform.position - attacker).normalized * 5;
            if (health <= 0) {
                State = CreatureState.Defeated; deathAt = Time.time; controller.enabled = false;
                if(Session.Mode==GameMode.Survival&&Session.IsNight)Session.Quest.NightVictory=true;
                int reward; int rewardCount = 2;
                switch (Kind)
                {
                    case CreatureKind.Brute: reward = (int)Block.Crystal; break;
                    case CreatureKind.Warden: reward = (int)Block.Crystal; rewardCount = 5; break;
                    case CreatureKind.MireStalker: reward = (int)Block.Moss; rewardCount = 3; break;
                    case CreatureKind.AshHound: reward = (int)Block.EmberCrystal; break;
                    case CreatureKind.ShadowArcher: reward = (int)Block.ShadowOre; break;
                    default: reward = Items.Ration; break;
                }
                // Drop items on the ground instead of directly adding to inventory
                if (!Session.Player.Inventory.Add(reward, rewardCount))
                    DroppedItem.Spawn(Session, transform.position, reward, rewardCount);
                else
                    Session.Notify("Recovered " + rewardCount + " " + Items.Name(reward));
            }
        }
    }

    /// <summary>Simple projectile for bows and shadow archers.</summary>
    public sealed class Projectile : MonoBehaviour
    {
        public Vector3 Direction;
        public float Speed = 15;
        public float Damage = 8;
        public GameSession Session;
        public bool IsPlayerProjectile = true;
        private float spawnTime;

        private void Start()
        {
            spawnTime = Time.time;
            if(Session&&Session.World)transform.SetParent(Session.World.transform,true);
            // Create arrow visual
            var art = new ArtMesh();
            Color color = IsPlayerProjectile ? ReferenceCharacters.Hex("69E9F8") : ReferenceCharacters.Hex("9B55FF");
            art.Bar(Vector3.zero, Direction.normalized * .5f, .03f, color, true);
            art.Build(transform, "Projectile visual");

            gameObject.layer = 2;
            var col = gameObject.AddComponent<SphereCollider>();
            col.radius = .15f;
            col.isTrigger = true;
        }

        private void Update()
        {
            if(!Session||!Session.Player||!Session.World){Destroy(gameObject);return;}
            if (Session.Paused) return;
            if (Time.time - spawnTime > 5) { Destroy(gameObject); return; }

            // Check hits
            RaycastHit hit;
            int mask=IsPlayerProjectile?~(1<<2):(1<<0)|(1<<2);
            if (Physics.SphereCast(transform.position,.10f,Direction,out hit,Speed*Time.deltaTime,mask,QueryTriggerInteraction.Ignore))
            {
                if (IsPlayerProjectile)
                {
                    var creature = hit.collider.GetComponentInParent<Creature>();
                    if (creature) { creature.Damage(Damage, Session.Player.transform.position); Session.Tone(200); }
                }
                else
                {
                    if (hit.collider.GetComponentInParent<Explorer>() && Session.Player)
                    {
                        Session.Player.Damage(Damage);
                    }
                }
                Destroy(gameObject);
                return;
            }
            transform.position += Direction * Speed * Time.deltaTime;

            // Hit world geometry
            Block block = Session.World.Data.Get(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y),
                Mathf.FloorToInt(transform.position.z));
            if (Blocks.Solid(block)) { Destroy(gameObject); }
        }
    }
}
