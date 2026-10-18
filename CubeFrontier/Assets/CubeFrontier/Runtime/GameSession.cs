using System;
using System.Collections.Generic;
using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    public sealed class GameSession : MonoBehaviour
    {
        public VoxelWorld World { get; private set; }
        public Explorer Player { get; private set; }
        public BuildingTool Builder { get; private set; }
        public FrontierQuest Quest { get; private set; } = new FrontierQuest();
        public FrontierMerchant Merchant { get; private set; }
        public CampState CampData { get; private set; } = new CampState();
        public CampStations CampStations { get; private set; }
        public bool IsNight { get { return Day>.75f||Day<.25f; } }
        public Weather Weather { get; private set; }
        public bool Settlement = true;
        public LocalSaves Saves { get; private set; }
        public GameMode Mode { get; private set; }
        public bool Paused { get; private set; } = true;
        public float Day = .25f, HitFlash, Volume = .35f;
        public bool IntenseEffects = true;
        public float Sensitivity = 2;
        public int Quality = 1;
        public string Message { get; private set; }
        public bool HasGame { get { return Player != null; } }
        public string HorrorObjective { get; private set; }
        public int SaveSlot { get; private set; }
        private Light sun;
        private AudioSource audioSource, ambient;
        private AudioClip ambientClip;
        private readonly Dictionary<int, AudioClip> tones = new Dictionary<int, AudioClip>();
        private float messageUntil, spawnAt, horrorElapsed;
        private int restoredBeacons;
        private Vector3[] hauntedLocations;
        private readonly List<GameObject> markers = new List<GameObject>();
        private readonly List<Creature> creatures = new List<Creature>();
        private GameObject worldRoot;
        private Camera menuCamera;
        private FrontierUI ui;

        private void Awake()
        {
            Application.targetFrameRate = 60; gameObject.name = "CubeFrontier";
            Saves = gameObject.AddComponent<LocalSaves>(); ui = gameObject.AddComponent<FrontierUI>(); ui.Session = this;
            audioSource = gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake = false;
            ambient = gameObject.AddComponent<AudioSource>(); ambient.playOnAwake = false; ambient.loop = true;
            var cam = new GameObject("Menu camera"); cam.transform.SetParent(transform); menuCamera = cam.AddComponent<Camera>();
            menuCamera.clearFlags = CameraClearFlags.SolidColor; menuCamera.backgroundColor = new Color(.96f, .95f, .90f);
            Sensitivity = PlayerPrefs.GetFloat("sensitivity", 2); Volume = PlayerPrefs.GetFloat("volume", .35f); Quality = PlayerPrefs.GetInt("quality", 1); IntenseEffects = PlayerPrefs.GetInt("horror-effects", 1) == 1;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
        public void Begin(int seed, GameMode mode, int slot, WorldSave saved = null)
        {
            if (worldRoot) { worldRoot.SetActive(false); Destroy(worldRoot); }
            foreach (var enemy in creatures) if (enemy) Destroy(enemy.gameObject); creatures.Clear();
            foreach (var marker in markers) if (marker) Destroy(marker); markers.Clear();
            Mode = saved == null ? mode : saved.Mode; SaveSlot = slot; Day = saved == null ? Mode == GameMode.Horror ? .74f : .39f : saved.Day;
            Quest = saved==null ? new FrontierQuest() : FrontierQuest.Restore(saved.Quest.Wood,saved.Quest.Stone,saved.Quest.Placed,saved.Quest.Flags);
            CampData = saved == null ? new CampState() : saved.Camp.Clone();
            horrorElapsed = 0; restoredBeacons = 0; HorrorObjective = "";
            var data = new WorldData(saved == null ? seed : saved.Seed,saved==null?2:saved.GeneratorVersion);
            if (saved != null) foreach (var edit in saved.Edits) data.Set(edit.Key, edit.Value);
            worldRoot = new GameObject("Frontier world"); World = worldRoot.AddComponent<VoxelWorld>();
            World.Material = Resources.Load<Material>("FrontierVoxel");
            if (!World.Material) { Notify("Project assets are missing. Run Cube Frontier > Prepare Project in the Unity Editor."); Destroy(worldRoot); World = null; return; }
            World.Initialize(data); World.RenderDistance = Quality + 2;
            Vector3 arrival = World.FindSpawn();
            if(saved==null && Mode!=GameMode.Horror && Settlement) StarterSettlement.Create(data,Mathf.FloorToInt(arrival.x),Mathf.FloorToInt(arrival.z));
            Builder = worldRoot.AddComponent<BuildingTool>(); Builder.Session = this;
            CampStations = worldRoot.AddComponent<CampStations>(); CampStations.Session = this;
            var playerObj = new GameObject("Explorer"); playerObj.layer = 2; playerObj.transform.SetParent(worldRoot.transform);
            Player = playerObj.AddComponent<Explorer>(); Player.Session = this; Player.Sensitivity = Sensitivity;
            Player.SetCharacter(saved==null?ReferenceCharacters.Selected:saved.Character);
            Player.Teleport(arrival);
            if (saved != null) {
                Player.Inventory.Restore(saved.Inventory); Player.Health = saved.Health; Player.Hunger = saved.Hunger; Player.Stamina = saved.Stamina; Player.Selected = saved.Selected;
                Player.Experience = saved.Experience;
                if (saved.Equipment != null && saved.Equipment.Length == Equipment.Slots) Player.Equipment.Restore(saved.Equipment);
                Vector3 position = new Vector3(saved.X, saved.Y, saved.Z);
                bool obstructed = false;
                for (int y = 0; y < 2; y++) if (Blocks.Solid(data.Get(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y) + y, Mathf.FloorToInt(position.z)))) obstructed = true;
                Player.Teleport(obstructed ? World.FindSpawn(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.z)) : position);
                Player.transform.rotation = Quaternion.Euler(0, saved.Yaw, 0); Player.Pitch = saved.Pitch;
            } else if (Mode == GameMode.Creative) {
                foreach (Block block in new[] { Block.Grass, Block.WhitePlaster, Block.StoneBricks, Block.Wood, Block.Leaves, Block.Glass, Block.RedRoof, Block.DarkTimber, Block.Torch }) Player.Inventory.Add((int)block, 64);
            } else { Player.Inventory.Add(Items.Pickaxe, 1); Player.Inventory.Add((int)Block.Wood, 12); Player.Inventory.Add(Items.Ration, 4); Player.Inventory.Add((int)Block.Torch, 8); }
            World.Target = Player.transform;
            if(Mode==GameMode.Survival) {
                var merchantObj=new GameObject("Mara - frontier merchant"); merchantObj.transform.SetParent(worldRoot.transform);
                merchantObj.transform.position=World.FindSpawn(Mathf.FloorToInt(arrival.x)+5,Mathf.FloorToInt(arrival.z));
                Merchant=merchantObj.AddComponent<FrontierMerchant>(); Merchant.Session=this;
            } else Merchant=null;
            worldRoot.AddComponent<FrontierAtmosphere>().Target = Player.transform;

            // Initialize weather system
            Weather = worldRoot.AddComponent<Weather>();
            Weather.Initialize(this, Player.transform);

            RenderSettings.skybox = Resources.Load<Material>("FrontierSky");
            var sunObj = new GameObject("Eclipse sun"); sunObj.transform.SetParent(worldRoot.transform); sun = sunObj.AddComponent<Light>(); sun.type = LightType.Directional; sun.shadows = LightShadows.Soft;
            RenderSettings.sun = sun; RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            menuCamera.enabled = false; spawnAt = Time.time + 20;
            if (Mode == GameMode.Horror) SetupHauntedLocations();
            CreateAmbient(); ApplySettings(); Resume();
            Notify(Mode == GameMode.Creative ? "F flies normally. G flies through blocks. Space rises; Ctrl descends. E opens blocks." : "J opens your quest and equipment. Find Mara, then press T to trade. E opens crafting.");
        }
        private void Update()
        {
            if (Time.unscaledTime > messageUntil) Message = "";
            HitFlash = Mathf.Max(0, HitFlash - Time.unscaledDeltaTime);
            if (!HasGame) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { if (Paused) Resume(); else Pause(); }
            if (Input.GetKeyDown(KeyCode.E) && !(Paused && ui.InventoryOpen && Player.Creative && ui.SearchFocused)) { if (Paused && ui.InventoryOpen) Resume(); else if (!Paused) { Pause(); ui.InventoryOpen = true; } }
            if (Input.GetKeyDown(KeyCode.B)) { if(Paused && ui.BuildingsOpen) Resume(); else if(!Paused) { Pause(); ui.BuildingsOpen=true; } }
            if(Input.GetKeyDown(KeyCode.J)) { if(Paused&&ui.JournalOpen) Resume(); else if(!Paused){Pause();ui.JournalOpen=true;} }
            if(Input.GetKeyDown(KeyCode.T)&&!Paused&&Merchant) { if(Merchant.CanTrade){Pause();ui.MerchantOpen=true;} else Notify("Move within 4 metres of Mara to trade. J shows her distance."); }
            if(Input.GetKeyDown(KeyCode.V)) { if(Paused&&ui.CampOpen) Resume(); else if(!Paused) CampStations.TryOpen(); }
            if (Paused) return;
            if(Mode==GameMode.Survival&&!Quest.Sheltered&&Quest.Placed>=12) {
                Vector3 p=Player.transform.position;
                if(FrontierQuest.IsShelter(World.Data,new Cell(Mathf.FloorToInt(p.x),Mathf.FloorToInt(p.y),Mathf.FloorToInt(p.z)))) {Quest.Sheltered=true;Notify("Shelter verified! Prepare for the night encounter. J shows your quest.");}
            }
            Day = Mathf.Repeat(Day + Time.deltaTime / 900, 1);
            float daylight = Mathf.Clamp01(Mathf.Sin((Day - .25f) * Mathf.PI * 2) * 1.4f + .2f);

            // Weather affects daylight
            float weatherDim = Weather != null ? 1 - Weather.Intensity * .3f : 1;
            float effectiveDaylight = daylight * weatherDim;

            sun.transform.rotation = Quaternion.Euler(Day * 360 - 90, -35, 0); sun.intensity = Mathf.Lerp(.07f, 1.35f, effectiveDaylight);
            float goldenHour=Mathf.Clamp01(1-Mathf.Abs(Mathf.Sin((Day-.25f)*Mathf.PI*2))*4);
            sun.color = Color.Lerp(Color.Lerp(new Color(.38f,.65f,.95f),new Color(1,.97f,.88f),effectiveDaylight),new Color(1,.68f,.38f),goldenHour*.65f);
            RenderSettings.ambientLight = Color.Lerp(new Color(.16f, .20f, .30f), new Color(.73f, .78f, .79f), effectiveDaylight);
            Shader.SetGlobalColor("_FrontierAmbient", Color.Lerp(new Color(.12f,.16f,.23f),new Color(.55f,.62f,.65f),effectiveDaylight));
            RenderSettings.fogColor = Color.Lerp(Color.Lerp(new Color(.035f,.055f,.13f),new Color(.67f,.84f,.88f),effectiveDaylight),new Color(.86f,.59f,.43f),goldenHour*.38f);
            Shader.SetGlobalColor("_FrontierHorizon",RenderSettings.fogColor.linear);
            Shader.SetGlobalColor("_FrontierSky",Color.Lerp(new Color(.015f,.028f,.07f),new Color(.34f,.68f,.94f),effectiveDaylight).linear);
            Shader.SetGlobalVector("_FrontierSun",-sun.transform.forward);
            RenderSettings.fogMode=FogMode.Linear; RenderSettings.fogStartDistance=Mode==GameMode.Horror?8:(Quality+2)*8;
            RenderSettings.fogEndDistance=Mode==GameMode.Horror?46:(Quality+2)*16-3;
            Player.View.backgroundColor = RenderSettings.fogColor;
            creatures.RemoveAll(c => !c);
            if (Mode != GameMode.Creative && Time.time > spawnAt && creatures.Count < 7 && World.Ready) {
                spawnAt = Time.time + (Mode == GameMode.Horror ? 12 : 25);
                SpawnCreature();
            }
            if (Mode == GameMode.Horror) UpdateHorror();
        }
        private void SpawnCreature()
        {
            float angle = UnityEngine.Random.Range(0, Mathf.PI * 2); Vector3 candidate = Player.transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * UnityEngine.Random.Range(16, 30);
            if (!World.HasColliderAt(candidate)) return;
            Vector3 spawn = World.FindSpawn(Mathf.FloorToInt(candidate.x), Mathf.FloorToInt(candidate.z)); if (!World.HasColliderAt(spawn)) return;
            var obj = new GameObject("Frontier creature"); obj.transform.SetParent(worldRoot.transform); obj.transform.position = spawn;
            var creature = obj.AddComponent<Creature>(); creature.Session = this;

            if (Mode == GameMode.Horror && !creatures.Exists(c => c.Kind == CreatureKind.Warden))
                creature.Kind = CreatureKind.Warden;
            else {
                // Biome-based spawning
                Biome biome = World.Data.Terrain.BiomeAt(Mathf.FloorToInt(spawn.x), Mathf.FloorToInt(spawn.z));
                bool isNight = Day > .75f || Day < .25f;
                switch (biome)
                {
                    case Biome.SwampWetlands: creature.Kind = CreatureKind.MireStalker; break;
                    case Biome.EclipseWastes: creature.Kind = isNight ? CreatureKind.ShadowArcher : CreatureKind.Stalker; break;
                    case Biome.AshenMountains: case Biome.FrozenHighlands: creature.Kind = CreatureKind.AshHound; break;
                    case Biome.HauntedForest: creature.Kind = isNight ? CreatureKind.Warden : CreatureKind.Stalker; break;
                    case Biome.AncientRuins: creature.Kind = CreatureKind.ShadowArcher; break;
                    default: creature.Kind = (CreatureKind)UnityEngine.Random.Range(0, 3); break;
                }
            }
            creatures.Add(creature);
        }
        private void SetupHauntedLocations()
        {
            Vector3 start = World.FindSpawn(); hauntedLocations = new Vector3[3];
            for (int i = 0; i < 3; i++) {
                Vector3 p = World.FindSpawn(Mathf.RoundToInt(start.x + Mathf.Cos(i * 2.094f) * 22), Mathf.RoundToInt(start.z + Mathf.Sin(i * 2.094f) * 22)); hauntedLocations[i] = p;
                int x = Mathf.FloorToInt(p.x), y = Mathf.FloorToInt(p.y), z = Mathf.FloorToInt(p.z);
                for (int dx = -2; dx <= 2; dx++) for (int dz = -2; dz <= 2; dz++) World.Change(new Cell(x + dx, y - 1, z + dz), Block.Basalt);
                for (int dy = 0; dy < 4; dy++) { World.Change(new Cell(x - 2, y + dy, z - 2), Block.Basalt); World.Change(new Cell(x + 2, y + dy, z + 2), Block.Basalt); }
                var marker = new GameObject("Haunted beacon " + (i + 1)); marker.transform.SetParent(worldRoot.transform); marker.transform.position = p;
                VoxelArt.Box(marker.transform, "Relic", Vector3.up * 1.1f, new Vector3(.35f, .8f, .35f), new Color(.1f, .75f, .9f), true);
                var light = marker.AddComponent<Light>(); light.color = Color.cyan; light.range = 6; light.intensity = 1; markers.Add(marker);
            }
        }
        private void UpdateHorror()
        {
            horrorElapsed += Time.deltaTime;
            float nearest = float.MaxValue; int index = -1;
            for (int i = 0; i < markers.Count; i++) if (markers[i].activeSelf) {
                float d = Vector3.Distance(Player.transform.position, hauntedLocations[i]); if (d < nearest) { nearest = d; index = i; }
                markers[i].transform.GetChild(0).Rotate(0, Time.deltaTime * 25, 0);
                markers[i].GetComponent<Light>().intensity = IntenseEffects ? .8f + Mathf.PerlinNoise(Time.time * 4, i) * .8f : 1;
            }
            HorrorObjective = "RESTORE THE WATCH  " + restoredBeacons + "/3  |  Dawn in " + Mathf.Max(0, 240 - horrorElapsed).ToString("0") + "s";
            if (index >= 0) HorrorObjective += "  |  Beacon " + (index + 1) + ": " + nearest.ToString("0") + "m";
            if (nearest < 3) {
                HorrorObjective += "  |  Hold the relic: press R";
                if (Input.GetKeyDown(KeyCode.R)) {
                    markers[index].SetActive(false); restoredBeacons++; Player.Inventory.Add((int)Block.Crystal, 1); Tone(760); Notify("A relic returns to the watch. The beacon is restored.");
                }
            }
            if (restoredBeacons == 3) { HorrorObjective = "The watch is restored. You escaped the eclipse."; Notify(HorrorObjective); Pause(); ui.HorrorResult = HorrorObjective; }
            else if (horrorElapsed >= 240) { HorrorObjective = "Dawn found the watch unguarded. The eclipse remains."; Notify(HorrorObjective); Pause(); ui.HorrorResult = HorrorObjective; }
        }
        public void Save()
        {
            if (!HasGame) return;
            if (Mode == GameMode.Horror) { Notify("Horror runs are session-based. Survival and Creative worlds support saving."); return; }
            Vector3 p = Player.SavePosition;
            var save = new WorldSave { Seed = World.Data.Terrain.Seed, Mode = Mode, X = p.x, Y = p.y, Z = p.z, Yaw = Player.transform.eulerAngles.y, Pitch = Player.Pitch, Health = Player.Health, Hunger = Player.Hunger, Stamina = Player.Stamina, Day = Day, Selected = Player.Selected, Inventory = Player.Inventory.Export(), Equipment = Player.Equipment.Export(), Experience = Player.Experience };
            save.GeneratorVersion=World.Data.Terrain.GeneratorVersion;
            save.Character=Player.Character; save.Quest=Quest; save.Camp=CampData.Clone();
            foreach (var edit in World.Data.Edits) save.Edits.Add(edit); Saves.Write(SaveSlot, save);
        }
        public void Pause() { Paused = true; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; Time.timeScale = 0; }
        public void Resume() { Paused = false; ui.InventoryOpen = false; ui.SettingsOpen = false; ui.BuildingsOpen = false; ui.JournalOpen=false;ui.MerchantOpen=false;ui.CampOpen=false; Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; Time.timeScale = 1; if(Player)Player.ResetLookInput(); if(Builder&&Builder.Active)Builder.ResetPlacementInput(); }
        public void Craft(Recipe recipe) {
            if(Player.Inventory.Craft(recipe)){if(Mode==GameMode.Survival)Quest.Craft(recipe.Output); Notify("Crafted "+recipe.Count+" "+Items.Name(recipe.Output));}
            else Notify("Missing ingredients or room in your pack.");
        }
        public void NightWatch() {
            if(Mode!=GameMode.Survival||!Merchant||!Merchant.CanTrade||!Quest.Sheltered||Quest.NightVictory)return;
            Day=.78f; spawnAt=Time.time; Resume(); Notify("Night watch begins. Defeat a creature, then return to Mara. Food and armour help.");
        }
        public void Menu()
        {
            if (HasGame && Mode != GameMode.Horror) Save();
            Paused = true; Time.timeScale = 1; Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            if (worldRoot) { worldRoot.SetActive(false); Destroy(worldRoot); }
            Player = null; World = null; Weather = null; CampStations = null; creatures.Clear(); markers.Clear(); ambient.Stop(); menuCamera.enabled = true; ui.ResetMenu();
        }
        public void Notify(string message) { Message = message; messageUntil = Time.unscaledTime + 6; }
        public void ApplySettings()
        {
            Quality = Mathf.Clamp(Quality, 0, 2); Sensitivity = Mathf.Clamp(Sensitivity, .3f, 5); Volume = Mathf.Clamp01(Volume);
            PlayerPrefs.SetFloat("sensitivity", Sensitivity); PlayerPrefs.SetFloat("volume", Volume); PlayerPrefs.SetInt("quality", Quality); PlayerPrefs.SetInt("horror-effects", IntenseEffects ? 1 : 0); PlayerPrefs.Save();
            if (Player) { Player.Sensitivity = Sensitivity; Player.View.farClipPlane = 60 + Quality * 35; }
            if (World) World.RenderDistance = Quality + 2;
            QualitySettings.shadowDistance = Quality == 0 ? 0 : Quality == 1 ? 35 : 65;
            ambient.volume = Volume * .18f;
        }
        public void Tone(int frequency)
        {
            AudioClip clip;
            if (!tones.TryGetValue(frequency, out clip)) {
                int samples = 4410; float[] data = new float[samples];
                for (int i = 0; i < samples; i++) { float t = i / 44100f; data[i] = Mathf.Sin(t * frequency * Mathf.PI * 2) * Mathf.Exp(-t * 38) * .25f; }
                clip = AudioClip.Create("Original frontier cue " + frequency, samples, 1, 44100, false); clip.SetData(data, 0); tones[frequency] = clip;
            }
            audioSource.PlayOneShot(clip, Volume);
        }
        private void CreateAmbient()
        {
            ambient.Stop(); if (ambientClip) Destroy(ambientClip);
            int count = 44100 * 8; float[] data = new float[count]; var rng = new System.Random(931); float noise = 0;
            for (int i = 0; i < count; i++) {
                float t = i / 44100f; noise = noise * .985f + ((float)rng.NextDouble() * 2 - 1) * .015f;
                float envelope = Mathf.Sin(Mathf.PI * i / count); float drone = Mode == GameMode.Horror ? Mathf.Sin(t * Mathf.PI * 2 * 55) * .10f + Mathf.Sin(t * Mathf.PI * 2 * 82.5f) * .05f : 0;
                data[i] = (noise + drone) * envelope;
            }
            ambientClip = AudioClip.Create("Original wind and eclipse drone", count, 1, 44100, false); ambientClip.SetData(data, 0); ambient.clip = ambientClip; ambient.Play();
        }
        private void OnApplicationFocus(bool focus) { if (!focus && HasGame) { Pause(); if (Mode != GameMode.Horror) Save(); } }
        private void OnApplicationQuit() { if (HasGame && Mode != GameMode.Horror) Save(); }
        private void OnDestroy() { Time.timeScale = 1; foreach (var clip in tones.Values) if (clip) Destroy(clip); if (ambientClip) Destroy(ambientClip); }
    }
}
