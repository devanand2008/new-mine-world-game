using System.Collections.Generic;
using UnityEngine;
using CubeFrontier.Core;
namespace CubeFrontier
{
    // Original pixel silhouettes, shared by recipes, trading, the pack and the hotbar.
    public static class ItemIllustrations
    {
        static readonly Dictionary<int,Texture2D> icons=new Dictionary<int,Texture2D>();
        public static Texture2D Get(int id) {
            Texture2D icon;if(icons.TryGetValue(id,out icon)&&icon)return icon;
            var pixels=new Color[256];Color dark=new Color(.10f,.17f,.22f),steel=new Color(.66f,.85f,.90f),wood=new Color(.64f,.38f,.20f),gold=new Color(1,.75f,.29f),mint=new Color(.36f,.91f,.78f);
            Color metal=id>=Items.IronPickaxe&&id<=Items.IronSword?steel:mint;
            System.Action<int,int,int,int,Color> box=(x,y,w,h,c)=>{for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)if(xx>=0&&xx<16&&yy>=0&&yy<16)pixels[xx+yy*16]=c;};
            if(Items.Armor(id)) {
                Color cloth=id>=Items.CrystalHelm?mint:id>=Items.IronHelm?steel:wood;int type=Items.ArmorSlotIndex(id);
                if(type==0){box(3,5,10,7,cloth);box(5,12,6,2,cloth);box(5,5,6,4,dark);box(4,11,8,1,Color.white);}
                else if(type==1){box(4,3,8,10,cloth);box(2,9,12,4,cloth);box(6,11,4,3,dark);box(5,4,1,7,Color.white);box(5,3,6,2,gold);}
                else {box(4,3,3,9,cloth);box(9,3,3,9,cloth);box(4,11,8,3,gold);box(3,2,4,2,dark);box(9,2,4,2,dark);}
            } else if(id==Items.Shield) {box(3,7,10,7,steel);box(4,4,8,4,steel);box(6,2,4,3,steel);box(5,7,6,5,dark);box(7,4,2,9,gold);}
            else if(id==Items.Ration||id==Items.CookedMeat) {box(2,5,12,6,wood);box(4,4,8,8,gold);box(3,7,10,3,new Color(.93f,.53f,.22f));for(int x=5;x<12;x+=3)box(x,9,1,3,new Color(1,.9f,.6f));}
            else if(id==Items.GoldenApple) {box(4,3,8,8,gold);box(3,5,10,5,gold);box(7,11,2,3,wood);box(9,12,3,2,mint);box(5,8,2,2,Color.white);}
            else if(id==Items.Bow){for(int y=2;y<14;y++){int x=10-Mathf.Abs(y-8)/2;box(x,y,2,1,wood);}box(5,2,1,12,steel);box(2,7,11,1,gold);box(12,6,2,3,mint);}
            else if(id==Items.Arrow){for(int i=3;i<12;i++)box(i,i,2,2,wood);box(10,10,4,4,steel);box(2,2,3,3,Color.white);}
            else if(id==Items.Torch){box(7,2,3,8,wood);box(5,9,7,4,new Color(1,.38f,.14f));box(7,10,3,5,gold);}
            else {
                for(int i=2;i<12;i++)box(i,i,2,2,wood);
                if(id==Items.Pickaxe||id==Items.IronPickaxe){box(3,11,10,2,metal);box(11,8,3,4,metal);box(3,12,8,1,Color.white);}
                else if(id==Items.Axe||id==Items.IronAxe){box(3,9,7,5,metal);box(2,10,2,3,metal);box(3,12,6,1,Color.white);}
                else if(id==Items.Shovel){box(9,9,5,5,steel);box(10,8,3,1,steel);}
                else {for(int i=6;i<14;i++)box(i,i,2,2,metal);box(4,6,6,2,gold);box(6,4,2,6,gold);box(12,13,2,2,Color.white);}
            }
            // One-pixel dark outline keeps silhouettes readable over any UI surface.
            var outlined=(Color[])pixels.Clone();for(int y=1;y<15;y++)for(int x=1;x<15;x++)if(pixels[x+y*16].a==0&&(pixels[x-1+y*16].a>0||pixels[x+1+y*16].a>0||pixels[x+(y-1)*16].a>0||pixels[x+(y+1)*16].a>0))outlined[x+y*16]=dark;
            icon=new Texture2D(16,16,TextureFormat.RGBA32,false);icon.name=Items.Name(id)+" icon";icon.filterMode=FilterMode.Point;icon.wrapMode=TextureWrapMode.Clamp;icon.SetPixels(outlined);icon.Apply();icons[id]=icon;return icon;
        }
    }
}
