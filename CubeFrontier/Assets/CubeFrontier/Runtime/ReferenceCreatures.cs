using UnityEngine;
using static CubeFrontier.ReferenceCharacters;
namespace CubeFrontier
{
 public static class ReferenceCreatures
 {
  public static readonly string[] Names={"Forest Stalker","Crystal Brute","Cave Bat","Hollow Warden","Mire Stalker","Veil Phantom","Rift Colossus","Crystal Fox","Stonehorn"};
  public static readonly string[] Details={"Bark plates, ivory mask, root antlers and moss","Layered rock, amber chest core and crystal spines","Angular red membranes, jointed wings and cyan eyes","Slate crown, floating armour and spectral cyan claws","Crouched roots, amber fungi and hanging vines","Ragged hood, ivory mask, red runes and floating cloth","Obsidian armour, violet fissures and a fractured rift halo","Pale coat, pointed ears and a crystalline brush tail","Mossy stone plates, mineral horns and expedition saddle"};
  static readonly Color Cyan=Hex("63E6EE"),Moss=Hex("637A35"),Bark=Hex("5B4830"),Rock=Hex("50545D");
  public static GameObject Create(Transform parent,int kind)
  {
   var root=new GameObject(Names[kind]);root.layer=parent.gameObject.layer;root.transform.SetParent(parent,false);
   if(kind>=7){Animal(root.transform,kind==7);return root;}
   var rig=root.AddComponent<ArtRig>();var body=new ArtMesh();
   bool tree=kind==0||kind==4,large=kind==1||kind==6,ghost=kind==3||kind==5;
   Color stone=tree?Bark:kind==5?Hex("302C35"):kind==3?Hex("303C49"):kind==6?Hex("302D3F"):Hex("635749");
   Color glow=tree?(kind==4?Hex("E7AE45"):Hex("9DDD5D")):kind==1?Hex("FFAE39"):kind==6?Hex("C465F5"):kind==5?Hex("EE595E"):Cyan;
   if(kind==2){Bat(root.transform,rig);return root;}
   float width=large?1.05f:tree?.50f:.70f;
   B(body,0,1.54f,-.02f,width,.76f,.44f,stone);B(body,0,1.02f,-.03f,width*.58f,.28f,.30f,stone*.7f);
   B(body,0,2.13f,0,.40f,.44f,.36f,stone);B(body,0,1.56f,.255f,.19f,.42f,.065f,glow,0,true);
   for(int s=-1;s<=1;s+=2)for(int j=0;j<4;j++){
    B(body,s*(width*.23f+.03f*(j%2)),1.85f-j*.16f,.22f,.24f+(large?.10f:0),.16f,.12f,stone*(.85f+j*.10f),s*(15-j*3));
    B(body,s*width*.19f,1.82f-j*.16f,-.25f,.25f,.18f,.13f,stone*(1.12f-j*.08f),s*12);
   }
   for(int s=-1;s<=1;s+=2){B(body,s*.108f,2.18f,.195f,.085f,.052f,.035f,glow,s*-12,true);B(body,s*.17f,2.01f,.173f,.08f,.19f,.11f,tree?Hex("C4BE8E"):stone*1.2f,s*7);}
   if(tree){
    B(body,0,2.12f,.22f,.13f,.40f,.08f,Hex("D7D2A8"));
    for(int s=-1;s<=1;s+=2){
     B(body,s*.12f,2.31f,.18f,.16f,.18f,.06f,Hex("D7D2A8"),s*20);
     Vector3 a=new Vector3(s*.15f,2.31f,0),b=new Vector3(s*.30f,2.68f,-.04f),c=new Vector3(s*.48f,2.83f,-.02f);
     body.Bar(a,b,.085f,Bark);body.Bar(b,c,.06f,Bark);body.Bar(b,new Vector3(s*.23f,2.94f,-.06f),.04f,Bark);body.Bar(c,new Vector3(s*.46f,3.02f,-.03f),.038f,Bark);
     for(int i=0;i<8;i++)B(body,s*(.23f+Mathf.Sin(i)*.07f),1.05f+i*.18f,.28f,.15f,.10f,.075f,Moss*(.8f+i%3*.12f),s*i*19);
    }
    if(kind==4){root.transform.localScale=new Vector3(1.25f,.79f,1.15f);for(int i=0;i<7;i++)Mushroom(body,new Vector3((i-3)*.15f,1.97f+Mathf.Sin(i)*.15f,-.10f),glow);}
   }
   if(kind==3){
    for(int i=0;i<7;i++)B(body,(i-3)*.075f,2.40f+(i%3)*.09f,-.05f,.065f,.38f+(i%2)*.16f,.17f,stone*(.9f+i%3*.15f));
    B(body,0,2.01f,.225f,.045f,.48f,.025f,glow,0,true);
    for(int i=0;i<9;i++)body.Crystal(new Vector3(Mathf.Sin(i*2.4f)*.96f,.74f+i*.20f,Mathf.Cos(i*2.4f)*.44f),.047f,.25f,glow);
   }
   if(kind==5){
    B(body,0,2.35f,-.035f,.59f,.16f,.50f,stone);for(int s=-1;s<=1;s+=2)B(body,s*.25f,2.15f,.04f,.13f,.45f,.36f,stone,s*12);
    B(body,0,2.10f,.21f,.20f,.32f,.07f,Hex("D8D1BA"));for(int s=-1;s<=1;s+=2)B(body,s*.065f,2.16f,.257f,.056f,.047f,.018f,glow,s*-17,true);
   }
   if(large){
    body.Crystal(new Vector3(0,1.60f,.33f),.17f,.52f,glow);
    for(int s=-1;s<=1;s+=2)for(int i=0;i<4;i++)body.Crystal(new Vector3(s*(.29f+i*.15f),2.10f+(i%2)*.15f,-.19f),.09f,.42f+(i%3)*.11f,glow,new Vector3(-12,0,s*-18));
    if(kind==6)for(int i=0;i<16;i++){
     float a=i*Mathf.PI/8;var p=new Vector3(Mathf.Sin(a)*.89f,2.09f+Mathf.Cos(a)*.89f,-.51f);
     body.Box(p,new Vector3(.27f,.17f,.14f),stone*1.3f,new Vector3(0,0,-i*22.5f));body.Box(p+Vector3.forward*.075f,new Vector3(.22f,.055f,.035f),glow,new Vector3(0,0,-i*22.5f),true);
    }
   }
   if(ghost)for(int i=0;i<13;i++){
    float a=i*2.399f,r=.12f+(i%3)*.09f;
    B(body,Mathf.Sin(a)*r,.32f+(i%5)*.17f,Mathf.Cos(a)*r,.12f,.23f+(i%3)*.06f,.11f,stone*(.8f+i%3*.10f),i%2==0?12:-9);
    if(kind==3)body.Crystal(new Vector3(Mathf.Sin(a)*r*.7f,.3f+(i%5)*.14f,Mathf.Cos(a)*r),.038f,.20f,glow);
   }
   body.Build(root.transform,"Mask, layered torso, crown and core");
   for(int s=-1;s<=1;s+=2){
    var arm=new ArtMesh();float ax=s*(width*.5f+.18f);
    B(arm,ax,1.42f,0,large?.38f:.16f,1.01f,large?.42f:.20f,stone,s*5);
    for(int j=0;j<5;j++){B(arm,ax+s*.02f,1.81f-j*.18f,.08f,large?.48f:.24f,.23f,large?.46f:.26f,stone*(.82f+(j%3)*.17f),s*(j%2==0?14:-8));if(tree)B(arm,ax+s*.08f,1.79f-j*.19f,.21f,.12f,.10f,.075f,Moss);}
    for(int j=0;j<3;j++)B(arm,ax+s*j*.065f,1.98f-j*.07f,-.03f,large?.60f:.42f,.14f,.49f,stone*(1.25f-j*.11f),s*15);
    if(large){
     B(arm,ax,.75f,.09f,.47f,.38f,.50f,stone);for(int j=0;j<4;j++)B(arm,ax+(j-1.5f)*.115f,.70f,.35f,.103f,.20f,.10f,stone*(.9f+j*.09f));arm.Crystal(new Vector3(ax+s*.10f,1.26f,.21f),.11f,.39f,glow,new Vector3(0,0,s*-25));
    }else{
     Color claw=kind==3?glow:kind==5?Hex("9A9184"):Bark;
     B(arm,ax,.81f,.065f,.22f,.20f,.19f,claw,0,kind==3);
     for(int j=0;j<4;j++){Vector3 a=new Vector3(ax+(j-1.5f)*.065f,.77f,.1f),b=a+new Vector3((j-1.5f)*.04f,-.22f,.12f),c=b+new Vector3(0,-.12f,-.03f);arm.Bar(a,b,.045f,claw,kind==3);arm.Bar(b,c,.028f,claw,kind==3);}
     if(tree)for(int j=0;j<3;j++)arm.Bar(new Vector3(ax+s*.13f,1.74f-j*.25f,.1f),new Vector3(ax+s*.16f,1.40f-j*.25f,.12f),.025f,Moss);
    }
    Transform at=arm.Build(root.transform,s<0?"Left arm":"Right arm",new Vector3(ax,1.93f,0));if(s<0)rig.LeftArm=at;else rig.RightArm=at;
    if(!ghost){
     var leg=new ArtMesh();float lx=s*(large?.30f:.20f);B(leg,lx,.54f,0,large?.39f:.16f,.97f,large?.39f:.18f,stone,s*-4);
     for(int j=0;j<4;j++)B(leg,lx,.86f-j*.19f,.09f,large?.42f:.23f,.22f,large?.38f:.25f,stone*(.86f+j*.09f),s*(j%2)*8);
     B(leg,lx,.10f,.16f,large?.48f:.26f,.19f,large?.64f:.43f,stone);
     if(large)leg.Crystal(new Vector3(lx,.61f,.23f),.065f,.28f,glow);else for(int j=0;j<3;j++)leg.Bar(new Vector3(lx,.13f,.11f),new Vector3(lx+(j-1)*.15f,.045f,.37f),.07f,Bark);
     Transform lt=leg.Build(root.transform,s<0?"Left leg":"Right leg",new Vector3(lx,1.01f,0));if(s<0)rig.LeftLeg=lt;else rig.RightLeg=lt;
    }
   }
   return root;
  }
  static void Mushroom(ArtMesh m,Vector3 p,Color amber){B(m,p.x,p.y,p.z,.045f,.17f,.045f,Hex("C9B480"));B(m,p.x,p.y+.095f,p.z,.22f,.055f,.18f,amber);B(m,p.x,p.y+.135f,p.z,.14f,.035f,.12f,amber*1.17f);}
  static void Bat(Transform root,ArtRig rig)
  {
   var m=new ArtMesh();Color dark=Hex("35333D"),red=Hex("8D4052");B(m,0,.54f,0,.30f,.35f,.26f,dark);B(m,0,.74f,.055f,.32f,.26f,.27f,dark);
   for(int s=-1;s<=1;s+=2){
    B(m,s*.13f,.94f,.014f,.10f,.22f,.13f,dark,s*-15);B(m,s*.085f,.77f,.198f,.06f,.055f,.026f,Cyan,s*-16,true);B(m,s*.059f,.643f,.203f,.028f,.075f,.032f,Hex("E5DDD1"),s*-10);
    m.Bar(new Vector3(s*.08f,.40f,.04f),new Vector3(s*.13f,.26f,.12f),.045f,dark);
    var wing=new ArtMesh();Vector3 a=new Vector3(s*.16f,.63f,0),b=new Vector3(s*.57f,1.05f,-.06f),c=new Vector3(s*1.14f,.85f,-.13f);
    Vector3[] edge={a,b,c,new Vector3(s*.91f,.48f,-.08f),new Vector3(s*.63f,.54f,-.03f),new Vector3(s*.43f,.33f,.02f),new Vector3(s*.29f,.49f,.04f)};
    for(int i=1;i<edge.Length-1;i++){wing.Face(a,edge[i],edge[i+1],red);wing.Face(a,edge[i+1],edge[i],red);}for(int i=0;i<edge.Length;i++)wing.Bar(edge[i],edge[(i+1)%edge.Length],.038f,dark);
    wing.Bar(b,edge[4],.025f,dark);wing.Bar(b,edge[5],.025f,dark);Transform w=wing.Build(root,s<0?"Left membrane":"Right membrane",a);if(s<0)rig.LeftArm=w;else rig.RightArm=w;
   }
   rig.Wings=true;m.Build(root,"Cave bat face and body");
  }
  static void Animal(Transform root,bool fox)
  {
   var m=new ArtMesh();Color coat=fox?Hex("C9E6E8"):Rock;
   B(m,0,.53f,0,.42f,.42f,.80f,coat);B(m,0,.73f,.38f,.39f,.35f,.35f,coat);B(m,0,.64f,.60f,.24f,.17f,.22f,fox?Hex("F4F1DE"):Rock*1.2f);B(m,0,.68f,.723f,.105f,.065f,.035f,Hex("263D48"));
   for(int s=-1;s<=1;s+=2){
    B(m,s*.123f,.79f,.565f,.077f,.066f,.035f,Cyan*.7f,0,true);B(m,s*.118f,.80f,.587f,.030f,.043f,.018f,Hex("263D48"));
    if(fox){B(m,s*.17f,1.01f,.39f,.155f,.30f,.14f,coat,s*-16);B(m,s*.174f,1.015f,.473f,.077f,.19f,.025f,Cyan,s*-16);}else{m.Crystal(new Vector3(s*.24f,1.10f,.46f),.10f,.66f,Cyan,new Vector3(-20,0,s*-24));B(m,s*.20f,.84f,.45f,.23f,.14f,.20f,Rock*1.2f);}
    for(int j=-1;j<=1;j+=2){B(m,s*.18f,.265f,j*.27f,.14f,.42f,.18f,coat*.85f);B(m,s*.18f,.095f,j*.27f+.045f,.18f,.14f,.25f,fox?Hex("476873"):Rock*.65f);}
   }
   if(fox){
    B(m,0,.53f,.44f,.31f,.24f,.09f,Hex("F5F4DF"));for(int i=0;i<6;i++){m.Crystal(new Vector3(i%2==0?-.08f:.08f,.79f,-.29f+i*.105f),.065f,.22f+(i%3)*.035f,Cyan);B(m,.07f+i*.07f,.48f+i*.055f,-.48f-i*.09f,.23f-i*.015f,.22f-i*.017f,.22f,Cyan*(.75f+i*.04f),i*-8);}
    m.Crystal(new Vector3(.46f,.84f,-1.04f),.08f,.24f,Hex("CDF8F6"),new Vector3(45,0,-35));
   }else{
    for(int i=0;i<18;i++)B(m,(i%3-1)*.16f,.75f+(i%2)*.06f,(i/3-2)*.14f,.23f,.12f,.19f,Rock*(.85f+i%4*.12f),(i%3-1)*9);
    for(int s=-1;s<=1;s+=2)for(int i=0;i<4;i++)B(m,s*.26f,.56f,-.31f+i*.19f,.12f,.19f,.17f,i%2==0?Moss:Rock);
    B(m,0,.865f,-.04f,.50f,.055f,.48f,Hex("925A3B"));B(m,0,.912f,-.06f,.33f,.05f,.35f,Hex("534133"));
    for(int s=-1;s<=1;s+=2){B(m,s*.28f,.67f,-.22f,.18f,.29f,.29f,Hex("987442"));B(m,s*.29f,.70f,-.39f,.15f,.055f,.025f,Hex("DEAB59"));B(m,s*.30f,.72f,.13f,.13f,.24f,.18f,Hex("5B6D60"));}
    B(m,0,1.02f,-.30f,.47f,.15f,.16f,Hex("B5A383"));m.Bar(new Vector3(0,.48f,-.4f),new Vector3(.08f,.40f,-.75f),.08f,Rock);m.Crystal(new Vector3(.08f,.42f,-.78f),.08f,.22f,Cyan,new Vector3(45,0,0));root.localScale=Vector3.one*1.65f;
   }
   m.Build(root,fox?"Crystal coat, ears and tail":"Stone plates, horns and loaded saddle");
  }
 }
}
