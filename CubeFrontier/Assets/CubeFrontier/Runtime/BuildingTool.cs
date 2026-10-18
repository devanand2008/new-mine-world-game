using System.Collections.Generic;
using CubeFrontier.Core;
using UnityEngine;
using Terrain=CubeFrontier.Core.Terrain;
namespace CubeFrontier
{
    public sealed class BuildingTool : MonoBehaviour
    {
        public GameSession Session;
        public bool Active { get; private set; }
        public bool ConsumedInput { get { return inputFrame == Time.frameCount; } }
        public string Status { get; private set; }
        public int Choice { get; private set; }
        int rotation,inputFrame=-1;Cell origin;bool valid,hasOrigin,awaitingRelease;float validateAt,enableAt;
        Transform ghost;Material ghostMaterial;LineRenderer footprint;
        List<BlockChange> pending,undo;
        public void ResetPlacementInput(){awaitingRelease=true;enableAt=Time.unscaledTime+.25f;inputFrame=Time.frameCount;}
        public void Select(int index)
        {
            if(!Session.Player.Creative)return;
            Choice=Mathf.Clamp(index,0,BuildingPlan.All.Length-1);rotation=0;Active=true;hasOrigin=false;
            // The menu click selects a plan; a separate click must place it in the world.
            ResetPlacementInput();
            var plan=BuildingPlan.All[Choice];
            if(ghost)Destroy(ghost.gameObject);
            if(ghostMaterial)Destroy(ghostMaterial);
            ghostMaterial=new Material(Resources.Load<Material>("FrontierBlueprint"));
            var mesh=new ArtMesh();
            foreach(var c in plan.Cells)if(c.Value!=Block.Air)mesh.Box(new Vector3(c.Key.X+.5f,c.Key.Y+.5f,c.Key.Z+.5f),Vector3.one*.98f,Color.white);
            ghost=mesh.Build(transform,"House placement preview",Vector3.zero,ghostMaterial);ghost.gameObject.layer=2;
            ghost.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            if(!footprint){var o=new GameObject("Build footprint");o.transform.SetParent(transform,false);footprint=o.AddComponent<LineRenderer>();footprint.sharedMaterial=Resources.Load<Material>("OutlineShader");footprint.widthMultiplier=.065f;footprint.positionCount=5;footprint.startColor=footprint.endColor=Color.cyan;}
            Session.Resume();Session.Notify("Aim at the ground. R rotates; left click builds; right click cancels. Z undoes the last house.");
        }
        public void Cancel(){inputFrame=Time.frameCount;Active=false;hasOrigin=false;if(ghost)ghost.gameObject.SetActive(false);if(footprint)footprint.enabled=false;Status="";}
        void Update()
        {
            if(!Session||!Session.Player)return;
            if(Session.Paused){if(ghost)ghost.gameObject.SetActive(false);if(footprint)footprint.enabled=false;return;}
            if(!Active){if(Session.Player.Creative&&Input.GetKeyDown(KeyCode.Z))Undo();return;}
            if(awaitingRelease){
                if(Time.unscaledTime<enableAt||Input.GetMouseButton(0)||Input.GetMouseButton(1))return;
                awaitingRelease=false;
            }
            if(Input.GetMouseButtonDown(1)){Cancel();return;}
            if(Input.GetKeyDown(KeyCode.R)){rotation=(rotation+1)%4;hasOrigin=false;}
            RaycastHit hit;var view=Session.Player.View;var plan=BuildingPlan.All[Choice];
            bool target=Physics.Raycast(view.transform.position,view.transform.forward,out hit,38,1<<0,QueryTriggerInteraction.Ignore);
            if(!target){Status="Aim at nearby ground to position the house.";ghost.gameObject.SetActive(false);footprint.enabled=false;return;}
            int w=plan.RotatedWidth(rotation),d=plan.RotatedDepth(rotation);
            Cell next=new Cell(Mathf.FloorToInt(hit.point.x)-w/2,Mathf.FloorToInt(hit.point.y+.02f),Mathf.FloorToInt(hit.point.z)-d/2);
            if(!hasOrigin||!next.Equals(origin)||Time.time>validateAt||Input.GetMouseButtonDown(0)){
                origin=next;hasOrigin=true;validateAt=Time.time+.4f;
                string reason="Aim at the top of a ground block.";valid=hit.normal.y>.5f&&BuildingPlacement.TryPlan(Session.World.Data,plan,origin,rotation,out pending,out reason);
                if(hit.normal.y<=.5f){reason="Aim at the top of a ground block.";pending=null;}
                // Reserve collision space for both the player and creatures, including foundation cells.
                if(valid){var box=new Bounds(new Vector3(origin.X+w*.5f,origin.Y+(plan.Height-6)*.5f,origin.Z+d*.5f),new Vector3(w,plan.Height+6,d));
                    if(box.Intersects(Session.Player.OccupiedBounds)||Physics.CheckBox(box.center,box.extents,Quaternion.identity,1<<8)){valid=false;reason="Move yourself and nearby creatures outside the footprint.";}}
                if(valid)for(int x=0;x<=w;x+=w)for(int z=0;z<=d;z+=d)if(!Session.World.HasColliderAt(new Vector3(origin.X+x,origin.Y,origin.Z+z))){valid=false;reason="Wait for this area's terrain to load.";}
                Status=valid?plan.Name+"  /  LEFT CLICK TO BUILD":reason;
            }
            ghost.gameObject.SetActive(true);ghost.localRotation=Quaternion.identity;
            ghost.position=new Vector3(origin.X,origin.Y,origin.Z);
            if(rotation==1)ghost.position+=Vector3.right*plan.Depth;else if(rotation==2)ghost.position+=new Vector3(plan.Width,0,plan.Depth);else if(rotation==3)ghost.position+=Vector3.forward*plan.Width;
            ghost.rotation=Quaternion.Euler(0,-rotation*90,0);
            Color tint=valid?new Color(.2f,1,.74f,.19f):new Color(1,.24f,.17f,.16f);ghostMaterial.SetColor("_Tint",tint);
            footprint.enabled=true;footprint.startColor=footprint.endColor=tint;
            footprint.SetPositions(new[]{new Vector3(origin.X,origin.Y+.05f,origin.Z),new Vector3(origin.X+w,origin.Y+.05f,origin.Z),new Vector3(origin.X+w,origin.Y+.05f,origin.Z+d),new Vector3(origin.X,origin.Y+.05f,origin.Z+d),new Vector3(origin.X,origin.Y+.05f,origin.Z)});
            if(valid&&Input.GetMouseButtonDown(0)){
                foreach(var c in pending)Session.World.Change(c.Cell,c.After);
                undo=pending;Session.Notify(plan.Name+" built. Every block is editable. Z undoes this house until you edit it.");Session.Tone(640);Cancel();
            }
        }
        void Undo()
        {
            if(!BuildingPlacement.CanUndo(Session.World.Data,undo)){Session.Notify("No unchanged house to undo. Edited houses are kept safe.");return;}
            foreach(var c in undo)Session.World.Change(c.Cell,c.Before);undo=null;Session.Notify("Last house removed; its original site was restored.");
        }
        void OnDestroy(){if(ghostMaterial)Destroy(ghostMaterial);}
    }
    public static class StarterSettlement
    {
        // Only called for a fresh world. The complete settlement is stored as ordinary saved edits.
        public static void Create(WorldData data,int sx,int sz)
        {
            Build(data,BuildingPlan.All[0],sx-15,sz+13);
            Build(data,BuildingPlan.All[1],sx+5,sz+15);
            Build(data,BuildingPlan.All[2],sx-4,sz+32);
            Build(data,BuildingPlan.All[3],sx+23,sz+24);
            // Broad winding approach; natural one-block terraces stay traversable.
            for(int z=3;z<=30;z++)for(int x=-1;x<=2;x++){
                int wx=sx+x,wz=sz+z,y=data.Terrain.Surface(wx,wz);if(y<=Terrain.SeaLevel)continue;
                data.Set(new Cell(wx,y,wz),Block.Path);
                for(int h=1;h<=5;h++){var c=new Cell(wx,y+h,wz);Block b=data.Get(c.X,c.Y,c.Z);if(b==Block.Wood||b==Block.Leaves||b==Block.Crystal)data.Set(c,Block.Air);}
            }
            for(int i=0;i<4;i++){
                int x=sx+(i%2==0?-3:4),z=sz+6+i*7,y=data.Terrain.Surface(x,z);
                if(y<=Terrain.SeaLevel)continue;
                for(int h=1;h<=3;h++)data.Set(new Cell(x,y+h,z),Block.DarkTimber);
                data.Set(new Cell(x,y+4,z),Block.Lantern);
            }
        }
        static void Build(WorldData data,BuildingPlan p,int x,int z)
        {
            int min=Terrain.Height,max=0;
            for(int dx=0;dx<p.Width;dx++)for(int dz=0;dz<p.Depth;dz++){int y=data.Terrain.Surface(x+dx,z+dz);min=System.Math.Min(min,y);max=System.Math.Max(max,y);}
            int floor=System.Math.Min(Terrain.Height-p.Height-2,(min+max)/2+1);
            if(floor<=Terrain.SeaLevel+1||max-min>10)return;
            for(int dx=0;dx<p.Width;dx++)for(int dz=0;dz<p.Depth;dz++){
                int ground=data.Terrain.Surface(x+dx,z+dz);
                for(int y=ground+1;y<floor;y++)data.Set(new Cell(x+dx,y,z+dz),Block.Cobblestone);
                for(int y=floor;y<=System.Math.Min(Terrain.Height-1,System.Math.Max(floor+p.Height,ground+7));y++)data.Set(new Cell(x+dx,y,z+dz),Block.Air);
            }
            foreach(var c in p.Cells)data.Set(new Cell(x+c.Key.X,floor+c.Key.Y,z+c.Key.Z),c.Value);
            // An outdoor stepped entry meets the unmodified terrain in front of the doorway.
            int door=x+p.Width/2;
            for(int dz=1;dz<=7;dz++){
                int ground=data.Terrain.Surface(door,z-dz),y=System.Math.Max(ground,floor-dz);
                for(int dx=-1;dx<=1;dx++){
                    for(int h=ground;h<=y;h++)data.Set(new Cell(door+dx,h,z-dz),Block.Cobblestone);
                    for(int h=y+1;h<=System.Math.Max(floor+3,ground+4);h++)data.Set(new Cell(door+dx,h,z-dz),Block.Air);
                }
            }
        }
    }
}
