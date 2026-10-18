using System;
using System.IO;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CubeFrontier.Core;
namespace CubeFrontier.Editor
{
    [InitializeOnLoad]
    public static class MilestoneVerification
    {
        const string Flag="CubeFrontier.MilestoneVerification";
        static MilestoneVerification() { EditorApplication.playModeStateChanged+=Changed; }
        public static void Run() { SessionState.SetBool(Flag,true); EditorApplication.EnterPlaymode(); }
        static void Changed(PlayModeStateChange state) {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Flag,false)) {
                SessionState.SetBool(Flag,false);new GameObject("Editor-only milestone verification").AddComponent<MilestoneDriver>();
            }
        }
    }
    public sealed class MilestoneInput : IPlayerInput
    {
        public bool Mining,Using;
        public Vector2 Movement {get{return Vector2.zero;}} public Vector2 Look {get{return Vector2.zero;}}
        public bool Jump {get{return false;}} public bool Sprint {get{return false;}}
        public bool Mine {get{return Mining;}} public bool Use {get{return Using;}}
    }
    // Tests production interactions through physics and the input abstraction in Play Mode.
    // No test hooks, debug cheats or fixture data are compiled into the player.
    public sealed class MilestoneDriver : MonoBehaviour
    {
        GameSession session; Explorer player; MilestoneInput input=new MilestoneInput();
        readonly MethodInfo interact=typeof(Explorer).GetMethod("Interact",BindingFlags.Instance|BindingFlags.NonPublic);
        readonly FieldInfo spawnAt=typeof(GameSession).GetField("spawnAt",BindingFlags.Instance|BindingFlags.NonPublic);
        string output;int checks;bool ended;
        void Start() { output=Path.GetFullPath("../TestResults/PlayModeMilestone");Directory.CreateDirectory(output);Application.logMessageReceived+=Log;StartCoroutine(Verify()); }
        void Log(string message,string trace,LogType type){if(type==LogType.Exception||type==LogType.Error)Finish(false,message+"\n"+trace);}
        void Check(bool valid,string name){if(!valid){Finish(false,name);throw new Exception(name);}checks++;File.AppendAllText(Path.Combine(output,"checks.txt"),"PASS: "+name+"\n");}
        void Finish(bool success,string error="") {if(ended)return;ended=true;File.WriteAllText(Path.Combine(output,"result.json"),"{\"passed\":"+(success?"true":"false")+",\"checks\":"+checks+",\"error\":\""+error.Replace("\\","/").Replace("\"","'").Replace("\r","").Replace("\n"," ")+"\"}");EditorApplication.Exit(success?0:1);}
        void Aim(Vector3 from,Vector3 at){player.View.transform.position=from;player.View.transform.rotation=Quaternion.LookRotation(at-from);Physics.SyncTransforms();}
        IEnumerator Verify() {
            File.WriteAllText(Path.Combine(output,"checks.txt"),"");
            session=FindFirstObjectByType<GameSession>();if(!session)session=new GameObject("Test session").AddComponent<GameSession>();
            typeof(LocalSaves).GetField("directory",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(session.Saves,output);
            session.Quality=0;session.Settlement=false;
            var fixture=SaveCodec.Decode(File.ReadAllBytes("../Tests/Fixtures/quest-start-v4.cfe"));
            session.Begin(714,GameMode.Survival,0,fixture);player=session.Player;player.enabled=false;player.Controls=input;
            spawnAt.SetValue(session,float.MaxValue);
            float deadline=Time.realtimeSinceStartup+45;
            while(!session.World.Ready&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return new WaitForSeconds(2);
            Check(session.World.Ready&&player.Character==4,"Survival terrain, colliders and saved character initialize");
            player.Teleport(new Vector3(.5f,45.1f,.5f));
            var cell=new Cell(0,46,3);
            for(int i=0;i<17;i++) {
                session.World.Change(cell,i<9?Block.Wood:Block.Stone);yield return new WaitForSeconds(.7f);
                Aim(new Vector3(.5f,46.6f,.5f),new Vector3(.5f,46.5f,3.5f));input.Mining=true;
                deadline=Time.realtimeSinceStartup+8;
                while(session.World.Data.Get(cell.X,cell.Y,cell.Z)!=Block.Air&&Time.realtimeSinceStartup<deadline){interact.Invoke(player,null);yield return null;}
                input.Mining=false;
                Check(session.World.Data.Get(cell.X,cell.Y,cell.Z)==Block.Air,"Physics mining removed resource "+i);
            }
            Check(session.Quest.Wood==4&&session.Quest.Stone==8,"Mining events satisfy both gathering objectives");
            session.Craft(Recipes.All[1]);Check(player.Inventory.Count(Items.Axe)==1&&session.Quest.Crafted,"Real recipe crafts an axe and advances commission");
            player.Selected=1;input.Using=true;
            Cell[] walls={new Cell(-1,45,0),new Cell(-1,46,0),new Cell(1,45,0),new Cell(1,46,0),new Cell(0,45,1),new Cell(0,46,1),new Cell(1,47,0),new Cell(-1,47,0),new Cell(0,47,1),new Cell(1,45,-1),new Cell(1,46,-1)};
            foreach(var c in walls){
                Aim(new Vector3(c.X+.5f,c.Y+2.5f,c.Z+.5f),new Vector3(c.X+.5f,c.Y-1,c.Z+.5f));interact.Invoke(player,null);yield return new WaitForSeconds(.65f);
                Check(session.World.Data.Get(c.X,c.Y,c.Z)==Block.Wood,"Physics placement builds shelter at "+c.X+","+c.Y+","+c.Z);
            }
            Aim(new Vector3(.5f,47.5f,-2.5f),new Vector3(.5f,47.5f,1.5f));interact.Invoke(player,null);input.Using=false;yield return new WaitForSeconds(.8f);
            Check(session.World.Data.Get(0,47,0)==Block.Wood&&session.Quest.Placed==12&&session.Quest.Sheltered,"Twelve placements and physical enclosure verify the shelter");
            player.Teleport(new Vector3(5.5f,45.1f,-2.5f));yield return null;
            Check(session.Merchant.CanTrade,"Merchant is reachable on physical terrain");
            int wood=player.Inventory.Count((int)Block.Wood);Check(player.Inventory.Craft(MerchantOffers.All[0])&&player.Inventory.Count((int)Block.Wood)==wood-4,"Merchant exchange consumes real resources");
            Check(player.Inventory.Craft(MerchantOffers.All[1]),"Merchant supplies ore for weapon crafting");
            session.Craft(Recipes.All[2]);Check(player.Inventory.Count(Items.Sword)==1,"Player crafts a blade for night watch");
            session.NightWatch();spawnAt.SetValue(session,float.MaxValue);Check(session.IsNight,"Merchant starts night watch only after shelter");
            player.Teleport(new Vector3(10.5f,45.1f,-2));
            var enemyObject=new GameObject("Test night encounter");enemyObject.transform.SetParent(session.World.transform);enemyObject.transform.position=new Vector3(10.5f,45.1f,.2f);
            var enemy=enemyObject.AddComponent<Creature>();enemy.Session=session;enemy.Kind=CreatureKind.Stalker;yield return null;
            float health=player.Health;yield return new WaitForSeconds(.3f);Check(player.Health==health,"Melee wind-up does not damage immediately");
            player.Teleport(new Vector3(10.5f,45.1f,-6));yield return new WaitForSeconds(.4f);Check(player.Health==health,"Moving out of reach avoids the pending strike");
            player.Teleport(new Vector3(10.5f,45.1f,-2));yield return new WaitForSeconds(2.4f);Check(player.Health<health&&player.Health>0,"Night encounter deals real damage without killing the prepared explorer");
            for(int i=0;i<9;i++)if(player.Inventory.At(i).Item==Items.Sword)player.Selected=i;
            deadline=Time.realtimeSinceStartup+12;input.Mining=true;
            while(enemy&&enemy.State!=CreatureState.Defeated&&Time.realtimeSinceStartup<deadline) {
                Vector3 e=enemy.transform.position;player.Teleport(e-Vector3.forward*1.9f);Aim(player.transform.position+Vector3.up*1.6f,e+Vector3.up*1.2f);interact.Invoke(player,null);yield return null;
            }
            input.Mining=false;Check(session.Quest.NightVictory&&player.Health>0,"Player melee defeats a real night creature and records victory");
            Check(session.Quest.Ready&&session.Quest.Claim(player.Inventory),"Full gathering-crafting-shelter-combat commission can be rewarded");
            session.Save();var saved=session.Saves.Read(0);session.Begin(714,GameMode.Survival,0,saved);session.Player.enabled=false;
            Check(session.Quest.Claimed&&session.Player.Character==4&&session.Player.Inventory.Count((int)Block.Crystal)==8,"Save and reload preserve completed objective, reward and character");
            Check(session.World.Data.Get(0,47,0)==Block.Wood,"Constructed shelter persists across reload");
            Finish(true);
        }
    }
}
