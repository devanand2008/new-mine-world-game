using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class Explorer : MonoBehaviour
    {
        public GameSession Session;
        public Camera View;
        public IPlayerInput Controls = new DesktopInput();
        public float Health = 100, Hunger = 100, Stamina = 100, Sensitivity = 2, Pitch;
        public bool Flying, ThirdPerson;
        public bool NoClip { get; private set; }
        public Bounds OccupiedBounds { get { return new Bounds(transform.position + Vector3.up * .9f, new Vector3(.64f, 1.8f, .64f)); } }
        public int Selected;
        public float MiningProgress { get; private set; }
        public string TargetName { get; private set; }
        public Inventory Inventory = new Inventory();
        public Equipment Equipment = new Equipment();
        public int Experience;
        public int Character { get; private set; }
        public void SetCharacter(int character) { Character=Mathf.Clamp(character,0,12); if(body)Destroy(body); body=ReferenceCharacters.Create(transform,Character); body.SetActive(ThirdPerson); }
        private CharacterController controller;
        private float verticalSpeed, attackAt, hungerClock, saveClock, placeAt, mineAt, dropAt, lavaClock;
        private Cell miningCell;
        private LineRenderer outline;
        private GameObject body;
        private Material outlineMaterial;
        private float ignoreLookUntil;
        public void ResetLookInput() { ignoreLookUntil=Time.unscaledTime+.4f; Input.ResetInputAxes(); }
        public bool Creative { get { return Session.Mode == GameMode.Creative; } }
        public bool InWater { get { var p = transform.position + Vector3.up; return Session.World.Data.Get(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), Mathf.FloorToInt(p.z)) == Block.Water; } }
        public bool InLava { get { var p = transform.position + Vector3.up; return Session.World.Data.Get(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), Mathf.FloorToInt(p.z)) == Block.Lava; } }
        private void Awake()
        {
            controller = GetComponent<CharacterController>(); controller.height = 1.8f; controller.radius = .32f; controller.center = Vector3.up * .9f; controller.stepOffset = .55f;
            var eye = new GameObject("Explorer camera"); eye.transform.SetParent(transform); eye.transform.localPosition = Vector3.up * 1.62f;
            View = eye.AddComponent<Camera>(); View.nearClipPlane = .08f; View.farClipPlane = 180; eye.AddComponent<AudioListener>();
            eye.AddComponent<HeldItem>().Player=this;
            outline = new GameObject("Block selection").AddComponent<LineRenderer>(); outline.transform.SetParent(transform);
            outlineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")); outlineMaterial.color = new Color(.06f, .06f, .06f);
            outline.sharedMaterial = outlineMaterial; outline.startWidth = outline.endWidth = .018f; outline.positionCount = 16; outline.enabled = false;
            body = VoxelArt.Explorer(transform); body.SetActive(false);
        }
        private void Update()
        {
            if (!Session || Session.Paused || !Session.World.Ready) { outline.enabled = false; return; }
            if (Cursor.lockState != CursorLockMode.Locked) { Session.Pause(); return; }
            Vector2 look = Time.unscaledTime<ignoreLookUntil?Vector2.zero:Controls.Look * Sensitivity; transform.Rotate(0, look.x, 0); Pitch = Mathf.Clamp(Pitch - look.y, -85, 85);
            if (Input.GetKeyDown(KeyCode.G) && Creative) SetNoClip(!NoClip);
            if (Input.GetKeyDown(KeyCode.F) && Creative) {
                if (!NoClip || SetNoClip(false)) { Flying = !Flying; verticalSpeed = 0; }
            }
            if (Input.GetKeyDown(KeyCode.F5)) { ThirdPerson = !ThirdPerson; body.SetActive(ThirdPerson); }
            UpdateCamera();
            for (int i = 0; i < 9; i++) if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i))) Selected = i;
            float scroll = Input.mouseScrollDelta.y; if (scroll != 0) Selected = (Selected + (scroll > 0 ? 8 : 1)) % 9;

            // Drop item with Q
            if (Input.GetKeyDown(KeyCode.Q) && Time.time >= dropAt) {
                Slot held = Inventory.At(Selected);
                if (held.Count > 0) {
                    dropAt = Time.time + .3f;
                    DroppedItem.Spawn(Session, transform.position + View.transform.forward * 1.5f + Vector3.up, held.Item, 1, held.Durability);
                    Inventory.ConsumeAt(Selected);
                    Session.Tone(200);
                }
            }

            Vector2 movement = Controls.Movement; bool sprint = Controls.Sprint && movement.y > 0 && Stamina > 1;
            bool water = InWater;
            bool lava = InLava;
            float speed = Flying ? 11 : water ? 2.6f : lava ? 1.5f : sprint ? 7 : 4.4f;
            Vector3 velocity = (transform.forward * movement.y + transform.right * movement.x) * speed;
            if (Flying) verticalSpeed = (Input.GetKey(KeyCode.Space) ? speed : 0) - (Input.GetKey(KeyCode.LeftControl) ? speed : 0);
            else {
                if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
                if (Controls.Jump && controller.isGrounded) verticalSpeed = 7;
                if (water) verticalSpeed = Input.GetKey(KeyCode.Space) ? 3 : -1;
                else if (lava) verticalSpeed = Input.GetKey(KeyCode.Space) ? 1.5f : -2;
                else verticalSpeed = Mathf.Max(-35, verticalSpeed - 20 * Time.deltaTime);
            }
            Vector3 next = transform.position + velocity * Time.deltaTime;
            if (!NoClip && !Session.World.HasColliderAt(next)) velocity = Vector3.zero;
            float falling = verticalSpeed; bool wasGrounded = controller.isGrounded;
            if (NoClip) {
                Vector3 destination = transform.position + (velocity + Vector3.up * verticalSpeed) * Time.deltaTime;
                destination.x = Mathf.Clamp(destination.x, -WorldData.Limit + 2, WorldData.Limit - 2);
                destination.z = Mathf.Clamp(destination.z, -WorldData.Limit + 2, WorldData.Limit - 2);
                destination.y = Mathf.Clamp(destination.y, -4, 250);
                transform.position = destination;
            } else {
                controller.Move((velocity + Vector3.up * verticalSpeed) * Time.deltaTime);
                if (!wasGrounded && controller.isGrounded && falling < -13 && !water) Damage((-falling - 12) * 4);
            }
            Stamina = Creative ? 100 : Mathf.Clamp(Stamina + (sprint && movement.sqrMagnitude > .01f ? -16 : 11) * Time.deltaTime, 0, 100);

            // Lava damage
            if (lava && !Creative) {
                lavaClock += Time.deltaTime;
                while(lavaClock >= .5f) { lavaClock -= .5f; Damage(7.5f); }
            } else lavaClock = 0;

            if (!Creative) {
                Hunger = Mathf.Max(0, Hunger - Time.deltaTime * (sprint ? .09f : .035f)); hungerClock += Time.deltaTime;
                if (hungerClock >= 2) { hungerClock = 0; if (Hunger == 0) Damage(2); else if (Hunger > 70) Health = Mathf.Min(100, Health + 1); }
            }
            if (transform.position.y < -5) Damage(100);
            if(Time.unscaledTime<ignoreLookUntil || (Session.Builder && (Session.Builder.Active || Session.Builder.ConsumedInput))) { outline.enabled=false;TargetName="";MiningProgress=0; }
            else Interact(); saveClock += Time.deltaTime;
            if (saveClock > 60) { saveClock = 0; if (Session.Mode != GameMode.Horror) Session.Save(); }
        }
        private void UpdateCamera()
        {
            Quaternion rotation = Quaternion.Euler(Pitch, transform.eulerAngles.y, 0);
            Vector3 pivot = transform.position + Vector3.up * 1.62f, destination = pivot;
            if (ThirdPerson) {
                Vector3 direction = rotation * Vector3.back; RaycastHit hit;
                destination += direction * (!NoClip && Physics.SphereCast(pivot, .2f, direction, out hit, 3.6f, 1 << 0, QueryTriggerInteraction.Ignore) ? Mathf.Max(.2f, hit.distance - .1f) : 3.6f);
            }
            View.transform.SetPositionAndRotation(destination, rotation);
        }
        private void Interact()
        {
            TargetName = ""; outline.enabled = false;
            Slot slot = Inventory.At(Selected);

            // Consumable items (ration, cooked meat, golden apple)
            if (Controls.Use && Items.Consumable(slot.Item) && Hunger < 100) {
                Inventory.ConsumeAt(Selected);
                Hunger = Mathf.Min(100, Hunger + Items.HungerRestore(slot.Item));
                if (slot.Item == Items.GoldenApple) Health = Mathf.Min(100, Health + 20);
                Session.Tone(420); return;
            }

            // Bow shooting
            if (Controls.Use && slot.Item == Items.Bow && Time.time >= attackAt) {
                int arrowSlot = -1;
                for (int i = 0; i < Inventory.Capacity; i++) { var s = Inventory.At(i); if (s.Item == Items.Arrow && s.Count > 0) { arrowSlot = i; break; } }
                if (arrowSlot >= 0 || Creative) {
                    attackAt = Time.time + Items.AttackSpeed(Items.Bow);
                    if (!Creative) Inventory.ConsumeAt(arrowSlot);
                    var arrowObj = new GameObject("Player arrow");
                    arrowObj.transform.position = View.transform.position + View.transform.forward;
                    var proj = arrowObj.AddComponent<Projectile>();
                    proj.Direction = View.transform.forward;
                    proj.Speed = 22;
                    proj.Damage = 12;
                    proj.Session = Session;
                    proj.IsPlayerProjectile = true;
                    if (!Creative) Inventory.Wear(Selected);
                    Session.Tone(350);
                    return;
                }
            }

            RaycastHit hit;
            if (!Physics.Raycast(View.transform.position, View.transform.forward, out hit, Creative?8:6, ~(1 << 2), QueryTriggerInteraction.Ignore)) { MiningProgress = 0; return; }
            var creature = hit.collider.GetComponentInParent<Creature>();
            if (creature) {
                TargetName = creature.DisplayName;
                if (Controls.Mine && Time.time >= attackAt && Vector3.Distance(transform.position, creature.transform.position) < 4) {
                    float dmg = Items.Weapon(slot.Item) ? Items.Damage(slot.Item) + 3 : slot.Item == Items.Pickaxe || slot.Item == Items.IronPickaxe ? 5 : 3;
                    float cd = Items.Weapon(slot.Item) ? Items.AttackSpeed(slot.Item) : .55f;
                    attackAt = Time.time + cd;
                    creature.Damage(dmg, transform.position);
                    if (!Creative) Inventory.Wear(Selected);
                    Session.Tone(140);
                }
                MiningProgress = 0; return;
            }
            if (!hit.collider.GetComponent<ChunkView>()) { MiningProgress = 0; return; }
            Vector3 point = hit.point - hit.normal * .01f;
            var cell = new Cell(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), Mathf.FloorToInt(point.z));
            Block block = Session.World.Data.Get(cell.X, cell.Y, cell.Z); TargetName = Blocks.Name(block); Outline(cell);
            if(CampStations.IsStation(block)) TargetName += "  [V] Open";
            if(Creative && Input.GetMouseButtonDown(2) && Blocks.Placeable((int)block))Inventory.SelectCreative(Selected,(int)block);
            if (!miningCell.Equals(cell)) { miningCell = cell; MiningProgress = 0; }
            if (Controls.Mine && Blocks.Breakable(block) && (!Creative || Time.time>=mineAt)) {
                string reason;
                if(Session.CampStations && !Session.CampStations.CanBreak(cell,out reason)) { MiningProgress=0;Session.Notify(reason);return; }
                float multiplier = ToolMultiplier(slot.Item, block);
                MiningProgress += Creative ? 1 : Time.deltaTime * multiplier / Blocks.Hardness(block);
                if (MiningProgress >= 1) {
                    if(Session.World.Change(cell, Block.Air)) {
                        if(Session.CampStations) Session.CampStations.OnRemoved(cell);
                        if(!Creative) {
                            if(!Inventory.Add((int)block,1)) {
                                DroppedItem.Spawn(Session, new Vector3(cell.X + .5f, cell.Y + .5f, cell.Z + .5f), (int)block, 1);
                                Session.Notify("Pack full — block dropped on the ground.");
                            }
                            Inventory.Wear(Selected); Experience++; Session.Quest.Mine(block);
                        }
                        Session.Tone(180);
                    }
                    MiningProgress = 0;
                    mineAt=Time.time+.15f;
                }
            } else MiningProgress = 0;
            if ((Controls.Use || (Creative && Input.GetMouseButton(1) && Time.time>=placeAt)) && Blocks.Placeable(slot.Item)) {
                Vector3 target = hit.point + hit.normal * .01f;
                var place = new Cell(Mathf.FloorToInt(target.x), Mathf.FloorToInt(target.y), Mathf.FloorToInt(target.z));
                Block existing = Session.World.Data.Get(place.X, place.Y, place.Z);
                var bounds = new Bounds(new Vector3(place.X + .5f, place.Y + .5f, place.Z + .5f), Vector3.one * .98f);
                if ((existing == Block.Air || existing == Block.Water) && !bounds.Intersects(OccupiedBounds) && !Physics.CheckBox(bounds.center, bounds.extents, Quaternion.identity, (1 << 8) | (1 << 9), QueryTriggerInteraction.Ignore)) {
                    if (Session.World.Change(place, (Block)slot.Item)) { if (!Creative) { Inventory.ConsumeAt(Selected); Session.Quest.Place(); } Session.Tone(280); placeAt=Time.time+.18f; }
                }
            }
        }
        private float ToolMultiplier(int tool, Block block)
        {
            // Pickaxe effectiveness
            if ((tool == Items.Pickaxe || tool == Items.IronPickaxe) &&
                (block == Block.Stone || block == Block.Ore || block == Block.Crystal || block == Block.Basalt ||
                 block == Block.ShadowOre || block == Block.EmberCrystal || block == Block.Moonstone ||
                 block == Block.AncientRelic || block == Block.Obsidian))
                return tool == Items.IronPickaxe ? 5 : 3;
            // Axe effectiveness
            if ((tool == Items.Axe || tool == Items.IronAxe) &&
                (block == Block.Wood || block == Block.Leaves || block == Block.Roots || block == Block.Mushroom))
                return tool == Items.IronAxe ? 5 : 3;
            // Shovel effectiveness
            if (tool == Items.Shovel &&
                (block == Block.Soil || block == Block.Sand || block == Block.Snow || block == Block.Clay ||
                 block == Block.Moss || block == Block.Gravel))
                return 4;
            return 1;
        }
        private void Outline(Cell cell)
        {
            Vector3 p = new Vector3(cell.X, cell.Y, cell.Z) - Vector3.one * .005f; float s = 1.01f;
            int[] indices = { 0, 1, 3, 2, 0, 4, 5, 1, 5, 7, 3, 7, 6, 2, 6, 4 };
            for (int i = 0; i < indices.Length; i++) { int n = indices[i]; outline.SetPosition(i, p + new Vector3((n & 1) * s, ((n >> 1) & 1) * s, ((n >> 2) & 1) * s)); }
            outline.enabled = true;
        }
        public void Damage(float amount)
        {
            if (Creative || Session.Paused) return;
            // Armor damage reduction
            float armor = Equipment.TotalArmor();
            float reduction = armor / (armor + 20); // diminishing returns
            amount *= (1 - reduction);
            Health = Mathf.Max(0, Health - amount); Session.HitFlash = .35f; Session.Tone(75);
            if (!Creative) Equipment.WearAll();
            if (Health <= 0) {
                Vector3 destination = Vector3.zero;
                bool home = Session.CampStations && Session.CampStations.TryRespawn(out destination);
                if(!home) destination=Session.World.FindSpawn();
                Teleport(destination); Health=100; Hunger=80; Stamina=100; lavaClock=0;
                Session.Notify(home ? "You wake beside your home bed. Your pack is intact." : "You wake at the frontier. Your pack is intact.");
            }
        }
        public bool CanOccupy(Vector3 position)
        {
            // Check voxel data as well as loaded physics: an unmeshed wall is still solid.
            for (int x = Mathf.FloorToInt(position.x - .32f); x <= Mathf.FloorToInt(position.x + .32f); x++)
                for (int y = Mathf.FloorToInt(position.y + .04f); y <= Mathf.FloorToInt(position.y + 1.76f); y++)
                    for (int z = Mathf.FloorToInt(position.z - .32f); z <= Mathf.FloorToInt(position.z + .32f); z++)
                        if (Blocks.Solid(Session.World.Data.Get(x, y, z))) return false;
            return true;
        }
        public bool SetNoClip(bool enabled)
        {
            if (!Creative) return false;
            if (enabled == NoClip) return true;
            if (!enabled && (!Session.World.HasColliderAt(transform.position) || !CanOccupy(transform.position))) {
                Session.Notify("Still inside blocks or loading terrain. Fly into open space, then press G again.");
                return false;
            }
            NoClip = enabled; controller.enabled = !enabled; verticalSpeed = 0;
            if (enabled) Flying = true;
            Session.Notify(enabled ? "Fly-through ON. WASD moves, Space rises, Ctrl descends. G restores collision in open space." : "Fly-through OFF. Solid blocks stop you again. F toggles normal flight.");
            return true;
        }
        public Vector3 SavePosition {
            get {
                Vector3 p = transform.position;
                return NoClip && !CanOccupy(p) ? Session.World.FindSpawn(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.z)) : p;
            }
        }
        public void Teleport(Vector3 position) { controller.enabled = false; transform.position = position; controller.enabled = !NoClip; verticalSpeed = 0; }
        private void OnDestroy() { if (outlineMaterial) Destroy(outlineMaterial); }
    }
}
