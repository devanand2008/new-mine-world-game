using System.Collections.Generic;
using CubeFrontier.Core;
using UnityEngine;
namespace CubeFrontier
{
    public sealed class HeldItem : MonoBehaviour
    {
        public Explorer Player;
        Transform model;int last=-1;float swing;
        public static Mesh BlockMesh(Block block,float size=.23f)
        {
            Vector3[,] c={
                {new Vector3(1,0,0),new Vector3(1,1,0),new Vector3(1,1,1),new Vector3(1,0,1)},
                {new Vector3(0,0,1),new Vector3(0,1,1),new Vector3(0,1,0),new Vector3(0,0,0)},
                {new Vector3(0,1,1),new Vector3(1,1,1),new Vector3(1,1,0),new Vector3(0,1,0)},
                {new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(1,0,1),new Vector3(0,0,1)},
                {new Vector3(1,0,1),new Vector3(1,1,1),new Vector3(0,1,1),new Vector3(0,0,1)},
                {new Vector3(0,0,0),new Vector3(0,1,0),new Vector3(1,1,0),new Vector3(1,0,0)}};
            var v=new List<Vector3>();var uv=new List<Vector2>();var colors=new List<Color>();var tr=new List<int>();
            for(int d=0;d<6;d++){
                int tile=Blocks.Tile(block,d);float sx=1f/Blocks.AtlasColumns,sy=1f/Blocks.AtlasRows;
                float u=tile%Blocks.AtlasColumns*sx,t=tile/Blocks.AtlasColumns*sy,ix=.5f/(Blocks.AtlasColumns*Blocks.AtlasTileSize),iy=.5f/(Blocks.AtlasRows*Blocks.AtlasTileSize);
                for(int i=0;i<4;i++){Vector3 q=c[d,i];v.Add((q-Vector3.one*.5f)*size);float x=d<2?q.z:q.x,y=d==2||d==3?q.z:q.y;uv.Add(new Vector2(u+ix+x*(sx-2*ix),t+iy+y*(sy-2*iy)));colors.Add(new Color(1,1,1,Blocks.Emits(block)?.8f:0));}
                int a=d*4;tr.AddRange(new[]{a,a+1,a+2,a,a+2,a+3});
            }
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void LateUpdate()
        {
            if(!Player||!Player.Session)return;
            bool show=!Player.ThirdPerson&&!Player.Session.Paused&&!(Player.Session.Builder&&Player.Session.Builder.Active);
            int id=Player.Inventory.At(Player.Selected).Item;
            if(last!=id){last=id;Rebuild(id);}
            if(!model)return;model.gameObject.SetActive(show);if(!show)return;
            bool moving=Player.Controls.Movement.sqrMagnitude>.01f;
            if(Input.GetMouseButtonDown(1)||Input.GetMouseButton(0)&&swing<=0)swing=1;
            swing=Mathf.Max(0,swing-Time.deltaTime*4.5f);
            float wave=Mathf.Sin(swing*Mathf.PI);
            model.localPosition=new Vector3(.40f-wave*.14f,-.25f-wave*.10f+(moving?Mathf.Sin(Time.time*9)*.018f:Mathf.Sin(Time.time*2)*.005f),.82f-wave*.12f);
            model.localRotation=Quaternion.Euler(8-wave*28,-24+wave*15,-8-wave*16);
        }
        void Rebuild(int id)
        {
            if(model){model.gameObject.SetActive(false);Destroy(model.gameObject);}
            var root=new GameObject("First-person hand and selected item");root.layer=2;root.transform.SetParent(transform,false);model=root.transform;
            var hand=new ArtMesh();hand.Box(new Vector3(.025f,-.22f,0),new Vector3(.15f,.32f,.16f),ReferenceCharacters.Hex("235963"));hand.Box(new Vector3(0,-.08f,0),new Vector3(.16f,.13f,.17f),ReferenceCharacters.Hex("D6A27C"));hand.Box(new Vector3(0,-.10f,.023f),new Vector3(.17f,.08f,.15f),ReferenceCharacters.Hex("353A3D"));
            hand.Build(model,"Trail sleeve and glove");
            if(Blocks.Placeable(id)){
                var block=new GameObject(Items.Name(id));block.layer=2;block.transform.SetParent(model,false);block.transform.localPosition=Vector3.up*.09f;Mesh mesh=BlockMesh((Block)id);
                block.AddComponent<MeshFilter>().sharedMesh=mesh;block.AddComponent<MeshRenderer>().sharedMaterial=Resources.Load<Material>(Blocks.Transparent((Block)id)?"FrontierGlass":"FrontierVoxel");block.AddComponent<OwnedArtMesh>().Mesh=mesh;
            }else if(Items.Tool(id)||Items.Weapon(id)){
                var item=new ArtMesh();
                Color handle=ReferenceCharacters.Hex("715337");
                if(id==Items.Pickaxe||id==Items.IronPickaxe){
                    item.Bar(new Vector3(0,-.10f,.01f),new Vector3(0,.33f,.03f),.045f,handle);
                    Color head=id==Items.IronPickaxe?ReferenceCharacters.Hex("AAB9BD"):ReferenceCharacters.Hex("75DBE4");
                    for(int s=-1;s<=1;s+=2)for(int j=0;j<4;j++)item.Box(new Vector3(s*(.06f+j*.055f),.35f-j*j*.014f,.03f),new Vector3(.09f,.07f,.06f),head,new Vector3(0,0,-s*j*16),id==Items.Pickaxe);
                } else if(id==Items.Axe||id==Items.IronAxe){
                    item.Bar(new Vector3(0,-.10f,.01f),new Vector3(0,.33f,.03f),.045f,handle);
                    Color head=id==Items.IronAxe?ReferenceCharacters.Hex("AAB9BD"):ReferenceCharacters.Hex("ACC1C4");
                    item.Box(new Vector3(.09f,.29f,.03f),new Vector3(.21f,.20f,.05f),head,new Vector3(0,0,-15));
                } else if(id==Items.Sword||id==Items.IronSword){
                    item.Bar(new Vector3(0,-.10f,.01f),new Vector3(0,.33f,.03f),.045f,handle);
                    Color blade=id==Items.IronSword?ReferenceCharacters.Hex("C0CDD3"):ReferenceCharacters.Hex("9ECDD6");
                    item.Crystal(new Vector3(0,.38f,.03f),.045f,.43f,blade);
                } else if(id==Items.Shovel){
                    item.Bar(new Vector3(0,-.10f,.01f),new Vector3(0,.30f,.03f),.04f,handle);
                    item.Box(new Vector3(0,.34f,.03f),new Vector3(.12f,.15f,.04f),ReferenceCharacters.Hex("8A8F92"));
                } else if(id==Items.Bow){
                    item.Bar(new Vector3(-.05f,-.05f,.01f),new Vector3(.05f,.38f,.01f),.035f,handle);
                    item.Bar(new Vector3(.05f,.38f,.01f),new Vector3(.12f,.20f,.01f),.025f,handle);
                    item.Bar(new Vector3(-.05f,-.05f,.01f),new Vector3(-.12f,.13f,.01f),.025f,handle);
                    // String
                    item.Bar(new Vector3(-.12f,.13f,.01f),new Vector3(.12f,.20f,.01f),.008f,ReferenceCharacters.Hex("E7D8A7"));
                    item.Crystal(new Vector3(0,.16f,.02f),.03f,.08f,ReferenceCharacters.Hex("75DBE4"));
                } else if(id==Items.Dagger){
                    item.Bar(new Vector3(0,-.08f,.01f),new Vector3(0,.10f,.02f),.04f,ReferenceCharacters.Hex("353040"));
                    item.Crystal(new Vector3(0,.18f,.02f),.035f,.25f,ReferenceCharacters.Hex("B879EF"));
                } else if(id==Items.Greatsword){
                    item.Bar(new Vector3(0,-.15f,.01f),new Vector3(0,.15f,.02f),.05f,handle);
                    item.Box(new Vector3(0,.14f,.02f),new Vector3(.18f,.05f,.06f),ReferenceCharacters.Hex("D8A24F"));
                    item.Crystal(new Vector3(0,.40f,.02f),.06f,.55f,ReferenceCharacters.Hex("FFAE39"));
                    item.Crystal(new Vector3(-.08f,.32f,.02f),.03f,.12f,ReferenceCharacters.Hex("E85A28"),new Vector3(0,0,30));
                    item.Crystal(new Vector3(.08f,.32f,.02f),.03f,.12f,ReferenceCharacters.Hex("E85A28"),new Vector3(0,0,-30));
                } else if(id==Items.Shield){
                    item.Box(new Vector3(0,.05f,.08f),new Vector3(.28f,.32f,.03f),ReferenceCharacters.Hex("6A7A88"));
                    item.Box(new Vector3(0,.05f,.095f),new Vector3(.06f,.25f,.02f),ReferenceCharacters.Hex("D8A24F"));
                    item.Box(new Vector3(0,.05f,.095f),new Vector3(.20f,.05f,.02f),ReferenceCharacters.Hex("D8A24F"));
                }
                item.Build(model,"Held tool");
            }else if(Items.Consumable(id)){
                var item=new ArtMesh();
                Color foodColor=id==Items.GoldenApple?ReferenceCharacters.Hex("D8A24F"):id==Items.CookedMeat?ReferenceCharacters.Hex("8B5A30"):ReferenceCharacters.Hex("B99957");
                item.Box(new Vector3(0,.035f,.035f),new Vector3(.17f,.16f,.08f),foodColor);
                if(id==Items.GoldenApple)item.Crystal(new Vector3(0,.12f,.035f),.04f,.06f,ReferenceCharacters.Hex("FFD040"));
                item.Build(model,"Held food item");
            }else if(Items.Armor(id)){
                var item=new ArtMesh();
                Color armorColor=id>=Items.CrystalHelm?ReferenceCharacters.Hex("75DBE4"):id>=Items.IronHelm?ReferenceCharacters.Hex("AAB9BD"):ReferenceCharacters.Hex("6B5030");
                item.Box(new Vector3(0,.05f,.03f),new Vector3(.18f,.18f,.14f),armorColor);
                item.Build(model,"Held armor piece");
            }
        }
    }
}
