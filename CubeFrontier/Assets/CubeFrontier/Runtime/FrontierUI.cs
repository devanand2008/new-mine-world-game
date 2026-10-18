using System;
using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    public sealed partial class FrontierUI : MonoBehaviour
    {
        public GameSession Session;
        public bool InventoryOpen, SettingsOpen, BuildingsOpen, JournalOpen, MerchantOpen, CampOpen;
        public string HorrorResult = "";
        private string page = "home", seed = "714", error = "";
        private int worldSlot, swapping = -1;
        private GameMode pendingMode;
        private bool confirmReplace;
        private GUIStyle title, heading, body, small, button, slotStyle;
        private Texture2D explorerArt, wardenArt;
        private CharacterGallery gallery;
        private CreativeVista vista;
        private bool showHelp;
        private GUIStyle field, toggle;
        private readonly Color ink = new Color(.035f, .075f, .10f), panel = new Color(.075f, .13f, .18f, .98f), cyan = new Color(.43f, .88f, .78f), muted = new Color(.67f, .77f, .80f), orange = new Color(.99f, .74f, .32f), textInk = new Color(.94f, .96f, .92f);

        public void ResetMenu() { page = "home"; InventoryOpen = SettingsOpen = BuildingsOpen = JournalOpen = MerchantOpen = CampOpen = false; HorrorResult = ""; confirmReplace = false; error = ""; }
        private void Styles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 53, fontStyle = FontStyle.Bold, wordWrap = false };
            heading = new GUIStyle(GUI.skin.label) { fontSize = 27, fontStyle = FontStyle.Bold, wordWrap = true };
            body = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            small = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            button = new GUIStyle(GUI.skin.button) { fontSize = 17, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(18, 14, 10, 10) };
            slotStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, wordWrap = true, alignment = TextAnchor.MiddleCenter };
            field = new GUIStyle(GUI.skin.textField) { fontSize = 18, padding = new RectOffset(12, 10, 5, 4) };
            field.normal.textColor = field.focused.textColor = new Color(.12f, .23f, .27f);
            field.normal.background = field.focused.background = Texture2D.whiteTexture;
            toggle = new GUIStyle(GUI.skin.toggle) { fontSize = 15 };
            toggle.normal.textColor = toggle.onNormal.textColor = toggle.hover.textColor = toggle.onHover.textColor = textInk;
            explorerArt = Resources.Load<Texture2D>("ExplorerConcept"); wardenArt = Resources.Load<Texture2D>("WardenConcept");
        }
        private void OnGUI()
        {
            Styles(); float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 800 * scale) / 2), Quaternion.identity, new Vector3(scale, scale, 1));
            GUI.color = Color.white;
            if (!vista) vista = gameObject.AddComponent<CreativeVista>();
            vista.Show(!Session.HasGame && page == "home" && !SettingsOpen);
            if (!Session.HasGame) MainMenu();
            else {
                HUD();
                if (Session.Paused) {
                    RectFill(new Rect(0, 0, 1280, 800), new Color(.015f, .035f, .06f, .72f));
                    if (SettingsOpen) Settings(); else if (InventoryOpen) Pack(); else if(BuildingsOpen) BuildingMenu(); else if(JournalOpen) Journal(); else if(MerchantOpen) Trade(); else if(CampOpen) CampPanel(); else PauseMenu();
                }
                if (!Session.World.Ready) { Card(new Rect(350, 300, 580, 140), panel); Label(385, 325, 520, 40, "A NEW HORIZON", heading, cyan); Label(385, 375, 520, 40, "Preparing the land around you...", body, textInk); }
                if (Session.IntenseEffects && Session.HitFlash > 0) RectFill(new Rect(0, 0, 1280, 800), new Color(.8f, .1f, .07f, Session.HitFlash * .35f));
            }
            if (!string.IsNullOrEmpty(Session.Message)) { float toastY=Session.Paused?24:Session.Builder&&Session.Builder.Active?264:566; Card(new Rect(280, toastY, 720, 64), panel); RectFill(new Rect(280,toastY,4,64),orange); Label(302, toastY+11, 676, 48, Session.Message, body, textInk); }
            GUI.matrix = Matrix4x4.identity; GUI.color = Color.white;
        }
        private void MainMenu()
        {
            RectFill(new Rect(-1280, -800, 3840, 2400), ink);
            for(int band=0;band<24;band++)RectFill(new Rect(0,92+band*30,1280,30),Color.Lerp(new Color(.13f,.29f,.35f),ink,band/23f));
            for(int x=0;x<1280;x+=64)RectFill(new Rect(x,92,1,708),new Color(.5f,.85f,.8f,.035f));
            for(int y=92;y<800;y+=64)RectFill(new Rect(0,y,1280,1),new Color(.5f,.85f,.8f,.035f));
            RectFill(new Rect(0, 0, 1280, 92), panel);
            BlockMark(48,25,40,cyan);
            Label(103, 27, 600, 32, "CUBE FRONTIER", heading, textInk);
            Label(104, 61, 600, 23, "E C L I P S E   /   AN OPEN WORLD OF POSSIBILITIES", small, muted);
            Label(948, 39, 300, 28, "THE CAMP UPDATE  /  0.9", small, cyan);
            if (SettingsOpen) { Settings(); return; }
            if (page == "gallery") { Gallery(); return; }
            if (page != "home") { InfoPage(); return; }
            GUI.enabled = !confirmReplace;
            Label(70, 119, 510, 28, "SMALL BLOCKS. GRAND ADVENTURES.", small, orange);
            Label(66, 153, 575, 155, "Build your\nnext adventure.", title, textInk);
            Label(70, 306, 505, 53, "From your first shelter to your own little kingdom.\nMake a world worth getting lost in.", body, muted);
            ModeCard(70, 379, "01", "Creative playground", "Unlimited blocks. Normal flight + fly-through.", cyan, GameMode.Creative);
            ModeCard(70, 470, "02", "Survival adventure", "Gather, trade, craft, and protect your home.", orange, GameMode.Survival);
            ModeCard(70, 561, "03", "Eclipse night", "Restore the beacons. Survive the darkness.", new Color(.72f,.62f,.95f), GameMode.Horror);
            Card(new Rect(615, 116, 595, 428), new Color(.77f,.91f,.94f));
            if (vista && vista.Image) GUI.DrawTexture(new Rect(620,121,585,418),vista.Image,ScaleMode.ScaleToFit);
            Card(new Rect(640, 132, 196, 34), panel);
            Label(650,139,183,24,"YOUR NEXT HORIZON",small,cyan);
            Card(new Rect(640, 489, 544, 36), panel);
            Label(657,496,510,25,Blocks.PlaceableCount+" blocks to build with    /    A thousand ways to begin",small,textInk);
            Card(new Rect(615, 561, 595, 146), panel);
            Label(637,578,340,25,"YOUR WORLD  /  CHOOSE A SAVE SLOT",small,cyan);
            Label(637,615,70,27,"Seed",body,muted);
            seed=GUI.TextField(new Rect(700,611,149,34),seed,11,field);
            Label(868,616,55,26,"Slot",small,muted);
            for(int i=0;i<3;i++) if(Button(918+i*48,611,42,34,(i+1).ToString(),worldSlot==i?cyan:muted)){worldSlot=i;confirmReplace=false;}
            if(Session.Saves.Exists(worldSlot) && Button(1070,611,118,34,"Load world",cyan)){
                try { var save=Session.Saves.Read(worldSlot);Session.Begin(save.Seed,save.Mode,worldSlot,save);error=""; }
                catch(Exception e){error=e.Message;}
            }
            Session.Settlement=GUI.Toggle(new Rect(637,667,550,28),Session.Settlement,"  Include a starter village in your new world",toggle);
            if(Button(70,660,510,47,"Meet your explorer  /  "+ReferenceCharacters.Names[ReferenceCharacters.Selected],cyan)){
                page="gallery";if(!gallery)gallery=gameObject.AddComponent<CharacterGallery>();gallery.Show(ReferenceCharacters.Selected,false);
            }
            if(!string.IsNullOrEmpty(error)) Label(70,711,1140,28,error,small,orange);
            if(Button(70,747,150,35,"Settings",muted))SettingsOpen=true;
            if(Button(234,747,150,35,"Credits",muted))page="credits";
            if(Button(398,747,250,35,"Multiplayer status",muted))page="multiplayer";
            Label(817,754,400,25,"LOCAL WORLDS   /   KEYBOARD + MOUSE",small,muted);
            GUI.enabled = true;
            if(confirmReplace){
                RectFill(new Rect(0,92,1280,708),new Color(.07f,.20f,.25f,.65f));
                Card(new Rect(350,265,580,255),panel);
                Label(382,294,520,46,"This slot already has a world",heading,textInk);
                Label(382,350,510,68,"Starting a new world in slot "+(worldSlot+1)+" will replace its saved world the next time you save. Choose another slot to keep both.",body,muted);
                if(Button(382,450,295,44,"Start replacement world",orange))Launch(pendingMode);
                if(Button(693,450,205,44,"Keep existing world",cyan))confirmReplace=false;
            }
        }
        private void ModeCard(float x,float y,string number,string name,string detail,Color accent,GameMode mode)
        {
            Rect r=new Rect(x,y,510,78);
            bool hover=r.Contains(Event.current.mousePosition);
            Card(r,hover?Color.Lerp(panel,accent,.09f):panel);
            RectFill(new Rect(x,y+12,5,54),accent);
            BlockMark(x+20,y+23,29,accent);
            Label(x+69,y+9,415,34,name,heading,textInk);
            Label(x+71,y+48,415,24,detail,small,muted);
            if(GUI.Button(r,GUIContent.none,GUIStyle.none))StartMode(mode);
        }
        private void StartMode(GameMode mode)
        {
            pendingMode = mode;
            if (mode != GameMode.Horror && Session.Saves.Exists(worldSlot)) confirmReplace = true; else Launch(mode);
        }
        private void Gallery()
        {
            Label(70,115,1000,48,"CHARACTERS OF THE FRONTIER",heading,cyan);
            Label(70,165,1070,38,"Find your explorer. Choose an appearance for your next world.",body,muted);
            if(Button(70,215,254,42,"EXPLORERS  /  13",gallery.Bestiary?muted:cyan))gallery.Show(ReferenceCharacters.Selected,false);
            if(Button(336,215,254,42,"BESTIARY  /  9",gallery.Bestiary?cyan:muted))gallery.Show(0,true);
            string[] names=gallery.Bestiary?ReferenceCreatures.Names:ReferenceCharacters.Names;
            for(int i=0;i<names.Length;i++) if(Button(70+(i%2)*266,277+(i/2)*51,254,44,names[i],i==gallery.Index?cyan:textInk))gallery.Show(i,gallery.Bestiary);
            if(gallery.Image)GUI.DrawTexture(new Rect(624,204,582,430),gallery.Image,ScaleMode.ScaleToFit);
            Label(650,651,550,33,names[gallery.Index],heading,cyan);
            Label(650,694,550,48,gallery.Bestiary?ReferenceCreatures.Details[gallery.Index]:ReferenceCharacters.Details[gallery.Index],small,muted);
            if(Button(70,656,250,44,gallery.Bestiary?"3D DESIGN PREVIEW":"Wear this appearance",gallery.Bestiary?muted:cyan)&&!gallery.Bestiary){PlayerPrefs.SetInt("character-appearance",gallery.Index);PlayerPrefs.Save();Session.Notify(names[gallery.Index]+" selected for your next new world. F5 changes the camera.");}
            if(Button(336,656,76,44,"<",muted))gallery.Turn(-45);
            if(Button(423,656,76,44,">",muted))gallery.Turn(45);
            if(Button(510,656,80,44,gallery.Rotate?"Stop":"Spin",muted))gallery.Rotate=!gallery.Rotate;
            Label(70,707,520,48,gallery.Bestiary?"Stalker, Brute, Bat, Warden and Mire Stalker appear in-game. Other gallery creatures are design previews.":"Cosmetic appearances share the existing explorer controls. Class skills and displayed character equipment are cosmetic.",small,muted);
            if(Button(70,758,230,33,"Back to expedition menu",cyan)){gallery.Hide();page="home";}
        }
        private void Launch(GameMode mode)
        {
            int value; if (!int.TryParse(seed, out value)) { error = "Enter a whole-number seed between -2147483648 and 2147483647."; return; }
            confirmReplace = false; error = ""; Session.Begin(value, mode, worldSlot);
        }
        private void InfoPage()
        {
            RectFill(new Rect(70, 145, 1140, 540), panel);
            Label(105, 175, 1000, 50, page == "credits" ? "A NEW FRONTIER" : "MULTIPLAYER DEVELOPMENT STATUS", heading, cyan);
            string copy = page == "credits"
                ? "Cube Frontier: Eclipse / The Horizon Update 0.8\n\nOriginal procedural terrain, block textures, layered character models, synthesized sound cues and wind ambience were created for this project.\n\nAll 18 supplied concept sheets and the 49-page character guide informed the 13 explorer appearances and 9 creature designs. Runtime models are low-poly adaptations of that artwork. Class skills, pet abilities and mount controls are proposed features, not implemented mechanics.\n\nBuilt for Unity 6 / Universal Render Pipeline. No Minecraft assets are included."
                : "This build currently runs local worlds only.\n\nLive multiplayer, room-code authorization, administrator login, and cloud persistence have not been implemented or tested.\n\nThe planned service boundary uses Unity Multiplayer Services sessions with browser-compatible transport, plus a separate authenticated administrative API. The three-digit room ID will never be treated as a password.\n\nSee Docs/IMPLEMENTATION.md for the remaining milestones and connection-test requirements.";
            Label(105, 245, 1010, 365, copy, body, textInk);
            if (Button(105, 612, 180, 45, "Back", cyan)) page = "home";
        }
        private void HUD()
        {
            var player=Session.Player;
            Card(new Rect(24,24,300,player.Creative?84:139),panel);
            Label(42,36,270,25,player.Creative?"CREATIVE  /  THE SKY IS YOURS":"SURVIVAL  /  YOUR EXPEDITION",small,cyan);
            if(player.Creative) Label(42,67,270,30,player.NoClip?"G  FLY-THROUGH ACTIVE":player.Flying?"F  Flying    G  Fly-through":"F  Take flight    G  Fly-through",small,player.NoClip?orange:muted);
            else {
                Meter(42,84,250,player.Health,new Color(.89f,.36f,.35f),"HEALTH");
                Meter(42,111,250,player.Hunger,orange,"HUNGER");
                Meter(42,138,250,player.Stamina,cyan,"STAMINA");
            }
            Card(new Rect(956,24,300,84),panel);
            string biome=Session.World.Data.Terrain.BiomeAt(Mathf.FloorToInt(player.transform.position.x),Mathf.FloorToInt(player.transform.position.z)).ToString();
            biome=System.Text.RegularExpressions.Regex.Replace(biome,"([a-z])([A-Z])","$1 $2");
            Label(976,37,265,30,biome,body,textInk);
            int minutes=Mathf.FloorToInt(Session.Day*1440);Label(976,72,265,25,(Session.IsNight?"NIGHT":"DAYLIGHT")+"  /  "+(minutes/60).ToString("00")+":"+(minutes%60).ToString("00"),small,Session.IsNight?cyan:orange);
            if(!Session.Paused){
                float yaw=player.transform.eulerAngles.y;string[] directions={"N","NE","E","SE","S","SW","W","NW"};
                Card(new Rect(554,24,172,40),panel);Label(574,33,140,28,directions[Mathf.RoundToInt(yaw/45)%8]+"   "+Mathf.RoundToInt(yaw)+" degrees",small,textInk);
                if(Session.Mode==GameMode.Survival&&!Session.Quest.Claimed){Card(new Rect(24,177,300,72),panel);Label(42,188,270,22,"FIRST FRONTIER   [J]",small,orange);Label(42,214,266,28,NextObjective(),small,textInk);}
            }
            if(!Session.Paused && Event.current.type==EventType.KeyDown && Event.current.keyCode==KeyCode.H)showHelp=!showHelp;
            if(showHelp){Card(new Rect(24,264,360,166),panel);Label(42,281,325,145,"WASD Move   Shift Sprint   Space Jump\nLMB Mine / Attack   RMB Place / Eat\nE Blocks / Pack   B Houses   F5 Camera\nV Use chest / furnace / bed\n"+(player.Creative?"F Flight   G Fly-through   Ctrl Down":"Q Drop item   1-9 Select hotbar")+"\nEsc Pause / Save   H Hide help",small,muted);}
            if(Session.Mode==GameMode.Horror){Card(new Rect(338,112,880,60),panel);Label(353,123,850,48,Session.HorrorObjective,body,orange);}
            if(!Session.Paused){RectFill(new Rect(631,398,18,4),textInk);RectFill(new Rect(638,391,4,18),textInk);RectFill(new Rect(633,399,14,2),Color.white);RectFill(new Rect(639,393,2,14),Color.white);}
            if(!string.IsNullOrEmpty(player.TargetName)){Card(new Rect(515,437,250,31),panel);Label(530,441,225,25,player.TargetName,small,textInk);}
            if(player.MiningProgress>0){RectFill(new Rect(580,478,120,5),panel);RectFill(new Rect(580,478,120*player.MiningProgress,5),cyan);}
            Card(new Rect(270,700,740,88),panel);
            for(int i=0;i<9;i++){
                Slot slot=player.Inventory.At(i);float x=282+i*80;
                Card(new Rect(x,709,72,69),i==player.Selected?cyan:ink);
                if(i==player.Selected){Card(new Rect(x+3,712,66,63),new Color(.14f,.28f,.31f));RectFill(new Rect(x+20,705,32,3),orange);}
                Label(x+7,713,20,20,(i+1).ToString(),small,muted);
                if(slot.Count>0){DrawItem(new Rect(x+22,730,35,35),slot.Item);if(!player.Creative)Label(x+43,754,30,20,slot.Count.ToString(),small,textInk);}
                if(slot.Count>0&&Items.Durability(slot.Item)>0){RectFill(new Rect(x+10,771,52,3),ink);RectFill(new Rect(x+10,771,52f*slot.Durability/Items.Durability(slot.Item),3),cyan);}
            }
            Slot selected=player.Inventory.At(player.Selected);
            if(selected.Count>0){Card(new Rect(460,655,360,35),panel);Label(478,660,330,28,Items.Name(selected.Item),body,textInk);}
            Card(new Rect(24,715,230,63),panel);Label(40,725,205,48,player.Creative?"E Blocks   B Houses   J Gear\nH Controls   Esc Pause":"E Craft   J Quest / Gear\nT Trade   Esc Pause",small,muted);
            if(!string.IsNullOrEmpty(Session.Saves.Status)){Card(new Rect(1025,713,231,65),panel);Label(1040,723,200,48,Session.Saves.Status,small,muted);}
            if(Session.Builder&&Session.Builder.Active){Card(new Rect(280,544,720,95),panel);Label(302,556,676,46,Session.Builder.Status,body,cyan);Label(302,606,676,26,"R Rotate   LMB Build   RMB Cancel   Z Undo last house",small,muted);}
        }
        private void Meter(float x, float y, float width, float value, Color color, string text)
        {
            Label(x, y - 13, 85, 22, text, small, muted);
            float segment=(width-122)/10;
            for(int i=0;i<10;i++)RectFill(new Rect(x+86+i*segment,y-6,segment-2,8),value>i*10?color:ink);
            Label(x + width - 28, y - 14, 40, 25, value.ToString("0"), small, textInk);
        }
        private void PauseMenu()
        {
            Card(new Rect(380, 130, 520, 550), panel);
            Label(420, 167, 435, 80, string.IsNullOrEmpty(HorrorResult) ? "TAKE A BREATH" : HorrorResult, heading, cyan);
            Label(420, 260, 425, 45, "The local world is paused.", body, muted);
            if (string.IsNullOrEmpty(HorrorResult) && Button(420, 320, 440, 48, "Resume expedition", cyan)) Session.Resume();
            if (Session.Mode != GameMode.Horror && Button(420, 384, 440, 48, Session.Saves.Busy ? "Save in progress..." : "Save world", textInk)) Session.Save();
            if (Button(420, 448, 440, 48, "Settings", textInk)) SettingsOpen = true;
            if (Button(420, 512, 440, 48, "Return to main menu", orange)) Session.Menu();
            Label(420, 591, 440, 60, Session.Mode == GameMode.Horror ? "Horror runs are not saved." : Session.Saves.Status ?? "Worlds save on this browser and device.", small, muted);
        }
        private void Settings()
        {
            Card(new Rect(290, 140, 700, 540), panel); Label(330, 175, 600, 50, "EXPEDITION SETTINGS", heading, cyan);
            Label(330, 255, 570, 30, "Mouse sensitivity  " + Session.Sensitivity.ToString("0.0"), body, textInk);
            Session.Sensitivity = GUI.HorizontalSlider(new Rect(330, 298, 570, 25), Session.Sensitivity, .3f, 5);
            Label(330, 332, 570, 30, "Audio volume  " + (Session.Volume * 100).ToString("0") + "%", body, textInk);
            Session.Volume = GUI.HorizontalSlider(new Rect(330, 377, 570, 25), Session.Volume, 0, 1);
            Label(330, 411, 570, 30, "Render quality / view distance", body, textInk);
            string[] qualities = { "Low / 2 chunks", "Medium / 3", "High / 4" };
            for (int i = 0; i < 3; i++) if (Button(330 + i * 197, 450, 185, 44, qualities[i], Session.Quality == i ? cyan : muted)) Session.Quality = i;
            Session.IntenseEffects = GUI.Toggle(new Rect(330, 522, 600, 35), Session.IntenseEffects, "  Horror flicker and damage tint", toggle);
            if (Button(330, 596, 275, 46, "Apply and return", cyan)) { Session.ApplySettings(); SettingsOpen = false; }
            if (Button(625, 596, 275, 46, "Toggle fullscreen", textInk)) Screen.fullScreen = !Screen.fullScreen;
        }
        private bool Button(float x, float y, float w, float h, string text, Color color)
        {
            Rect r=new Rect(x,y,w,h);bool hover=GUI.enabled&&r.Contains(Event.current.mousePosition);
            bool primary=color==cyan;Color fill=primary?Color.Lerp(cyan,Color.white,hover?.18f:0):Color.Lerp(panel,color,hover?.22f:.08f);
            Card(r,fill);RectFill(new Rect(x,y,3,h),primary?cyan:color*.8f);
            Color lettering=primary?ink:color;GUI.contentColor=Color.white;
            var saved=button.normal.background;button.normal.background=null;button.hover.background=null;button.active.background=null;button.focused.background=null;
            button.normal.textColor=button.hover.textColor=button.active.textColor=button.focused.textColor=lettering;
            bool clicked=GUI.Button(r,text,button);button.normal.background=saved;GUI.contentColor=Color.white;
            if(clicked){GUI.FocusControl(null);SearchFocused=false;}
            return clicked;
        }
        private static void Card(Rect rect, Color color)
        {
            RectFill(new Rect(rect.x+3,rect.y+4,rect.width,rect.height),new Color(0,0,0,.20f));
            RectFill(rect, color);
            RectFill(new Rect(rect.x,rect.y,rect.width,2),Color.Lerp(color,Color.white,.20f));
            RectFill(new Rect(rect.x,rect.y,2,rect.height),Color.Lerp(color,Color.white,.13f));
            RectFill(new Rect(rect.x,rect.yMax-2,rect.width,2),Color.Lerp(color,Color.black,.45f));
            RectFill(new Rect(rect.xMax-2,rect.y,2,rect.height),Color.Lerp(color,Color.black,.35f));
        }
        private void BlockMark(float x,float y,float size,Color color) {RectFill(new Rect(x,y+size*.28f,size,size*.72f),color);RectFill(new Rect(x+size*.2f,y,size*.6f,size*.28f),Color.Lerp(color,Color.white,.25f));RectFill(new Rect(x+size*.65f,y+size*.28f,size*.35f,size*.72f),color*.7f);}
        private string NextObjective(){var q=Session.Quest;if(q.Wood<4)return "Gather timber  "+q.Wood+" / 4";if(q.Stone<8)return "Mine stone  "+q.Stone+" / 8";if(!q.Crafted)return "Craft your first tool  [E]";if(!q.Sheltered)return "Build a shelter  "+q.Placed+" / 12";if(!q.NightVictory)return "Prepare for the night watch";return "Return to Mara for your reward";}
        private static void RectFill(Rect rect, Color color) {
            // WebGL's linear framebuffer converts IMGUI texture tints; the Windows player
            // presents these tints directly. Keep the authored sRGB palette consistent.
#if UNITY_WEBGL && !UNITY_EDITOR
            GUI.color=QualitySettings.activeColorSpace==ColorSpace.Linear?color.linear:color;
#else
            GUI.color=color;
#endif
            GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color=Color.white;
        }
        private static void Label(float x, float y, float w, float h, string text, GUIStyle style, Color color) { GUI.contentColor = Color.white; style.normal.textColor=color; GUI.Label(new Rect(x, y, w, h), text ?? "", style); }
    }
}


