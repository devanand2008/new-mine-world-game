using System.Collections.Generic;
using CubeFrontier.Core;
using UnityEngine;
namespace CubeFrontier
{
    public sealed partial class FrontierUI
    {
        string catalogueSearch="";int catalogueCategory,cataloguePage,recipePage;
        public bool SearchFocused { get; private set; }
        Texture2D blockAtlas;
        void DrawItem(Rect rect,int id)
        {
            if(!blockAtlas)blockAtlas=Resources.Load<Texture2D>("FrontierAtlas");
            if(Blocks.Placeable(id)&&blockAtlas){int tile=Blocks.Tile((Block)id,4);float sx=1f/Blocks.AtlasColumns,sy=1f/Blocks.AtlasRows,ix=.5f/blockAtlas.width,iy=.5f/blockAtlas.height;GUI.DrawTextureWithTexCoords(rect,blockAtlas,new Rect(tile%Blocks.AtlasColumns*sx+ix,tile/Blocks.AtlasColumns*sy+iy,sx-2*ix,sy-2*iy));}
            else if(Items.Valid(id))GUI.DrawTexture(rect,ItemIllustrations.Get(id),ScaleMode.ScaleToFit);
        }
        void SlotButton(Inventory inv,int i,float x,float y,float size)
        {
            Slot slot=inv.At(i);
            RectFill(new Rect(x,y,size,size),swapping==i?cyan:i==Session.Player.Selected?orange:muted*.35f);
            RectFill(new Rect(x+2,y+2,size-4,size-4),ink);
            if(GUI.Button(new Rect(x,y,size,size),new GUIContent("",slot.Count>0?Items.Name(slot.Item):"Empty slot"),GUIStyle.none)){if(swapping<0)swapping=i;else{inv.Swap(swapping,i);swapping=-1;}}
            if(slot.Count>0){DrawItem(new Rect(x+10,y+7,size-20,size-20),slot.Item);Label(x+5,y+size-20,size-8,19,Session.Player.Creative?"INF":slot.Count.ToString(),small,textInk);}
            if(i<9)Label(x+4,y+3,15,20,(i+1).ToString(),small,muted);
        }
        void Pack()
        {
            bool creative=Session.Player.Creative;Inventory inv=Session.Player.Inventory;
            Card(new Rect(40,100,1200,647),panel);
            RectFill(new Rect(776,218,1,431),new Color(.35f,.6f,.65f,.25f));
            Label(70,125,720,42,creative?"THE BUILDER'S COLLECTION":"YOUR EXPEDITION PACK",heading,cyan);
            Label(70,172,1090,30,creative?"Choose a material to replace the selected hotbar slot. All materials are unlimited in Creative.":"Gather materials in the world, then craft building blocks and tools here.",body,muted);
            Label(70,225,720,29,creative?"MATERIAL LIBRARY  /  "+Blocks.PlaceableCount+" BLOCKS":"FIELD RECIPES",body,textInk);
            Label(798,225,370,29,"YOUR PACK",body,cyan);
            if(creative)Catalogue();else Crafting();
            // Compact 9-column bag keeps all twenty-seven slots visible without scrolling.
            for(int i=0;i<Inventory.Capacity;i++)SlotButton(inv,i,798+i%9*45,267+i/9*48,42);
            Label(800,423,380,47,"Click two pack slots to swap. Select your hotbar slot below before choosing a material.",small,muted);
            for(int i=0;i<9;i++)if(Button(798+i%5*78,481+i/5*39,70,33,(i+1).ToString(),Session.Player.Selected==i?cyan:muted))Session.Player.Selected=i;
            Label(800,575,370,55,GUI.tooltip,body,textInk);
            if(Button(70,680,270,43,"Return to expedition",cyan)){swapping=-1;Session.Resume();}
            if(Button(358,680,245,43,"House blueprints  [B]",orange)){InventoryOpen=false;BuildingsOpen=true;}
            Label(799,672,390,51,"RMB Place   LMB Mine   E Close\nCreative: hold RMB to build a wall",small,muted);
        }
        void Catalogue()
        {
            Label(70,265,95,28,"SEARCH",small,muted);
            GUI.SetNextControlName("material-search");
            string previous=catalogueSearch;catalogueSearch=GUI.TextField(new Rect(145,262,306,29),catalogueSearch,35,field);if(previous!=catalogueSearch)cataloguePage=0;
            SearchFocused=GUI.GetNameOfFocusedControl()=="material-search";
            string[] categories={"All","Nature","Building","Decor"};
            for(int i=0;i<4;i++)if(Button(70+i*172,307,164,34,categories[i],i==catalogueCategory?cyan:muted)){catalogueCategory=i;cataloguePage=0;}
            var items=new List<int>();for(int i=1;i<Blocks.Count;i++)if(Blocks.Placeable(i)&&(catalogueCategory==0||Blocks.Category((Block)i)==categories[catalogueCategory])&&(catalogueSearch.Length==0||Items.Name(i).IndexOf(catalogueSearch,System.StringComparison.OrdinalIgnoreCase)>=0))items.Add(i);
            int pages=Mathf.Max(1,Mathf.CeilToInt(items.Count/18f));cataloguePage=Mathf.Clamp(cataloguePage,0,pages-1);
            for(int n=0;n<18&&n+cataloguePage*18<items.Count;n++){
                int id=items[n+cataloguePage*18];float x=70+n%6*116,y=354+n/6*88;
                Card(new Rect(x,y,107,80),new Rect(x,y,107,80).Contains(Event.current.mousePosition)?new Color(.16f,.32f,.36f):ink);
                if(GUI.Button(new Rect(x,y,107,80),new GUIContent("",Items.Name(id)),GUIStyle.none)) {GUI.FocusControl(null);SearchFocused=false;Session.Player.Inventory.SelectCreative(Session.Player.Selected,id);Session.Tone(480);}
                DrawItem(new Rect(x+35,y+6,35,35),id);Label(x+6,y+44,100,36,Items.Name(id),small,textInk);
            }
            if(items.Count==0)Label(85,390,600,80,"No materials match this search.",body,muted);
            if(Button(70,625,145,34,"< Previous",muted))cataloguePage=Mathf.Max(0,cataloguePage-1);
            Label(250,633,260,25,(cataloguePage+1)+" / "+pages+"     "+items.Count+" materials",small,cyan);
            if(Button(550,625,195,34,"Next >",muted))cataloguePage=Mathf.Min(pages-1,cataloguePage+1);
        }
        void Crafting()
        {
            int pages=Mathf.CeilToInt(Recipes.All.Length/6f);recipePage=Mathf.Clamp(recipePage,0,pages-1);
            for(int i=0;i<6&&recipePage*6+i<Recipes.All.Length;i++){
                Recipe r=Recipes.All[recipePage*6+i];float y=268+i*58;DrawItem(new Rect(73,y+3,38,38),r.Output);
                bool ready=true;string ingredients="";
                foreach(var input in r.Inputs){int have=Session.Player.Inventory.Count(input.Item);if(have<input.Count)ready=false;ingredients+=Items.Name(input.Item)+" "+have+"/"+input.Count+"  ";}
                if(Button(124,y,620,31,"Craft "+r.Count+" "+Items.Name(r.Output),ready?cyan:muted))Session.Craft(r);
                Label(130,y+33,615,25,ingredients,small,muted);
            }
            if(Button(70,625,145,34,"< Previous",muted))recipePage=Mathf.Max(0,recipePage-1);
            Label(250,633,250,25,"Recipes  "+(recipePage+1)+" / "+pages,small,cyan);
            if(Button(550,625,195,34,"Next >",muted))recipePage=Mathf.Min(pages-1,recipePage+1);
        }
        void BuildingMenu()
        {
            Card(new Rect(55,110,1170,610),panel);Label(87,139,1050,45,"A LITTLE INSPIRATION FOR YOUR NEXT BUILD",heading,cyan);
            Label(88,195,1050,54,Session.Player.Creative?"Choose a blueprint, aim at a clear site, rotate with R and click to build. Every wall and window is editable.":"Ready-made houses can be placed in Creative. In Survival, craft these materials and build your own house block by block.",body,muted);
            for(int i=0;i<BuildingPlan.All.Length;i++){
                var plan=BuildingPlan.All[i];float x=87+i*279;
                Card(new Rect(x,266,263,333),ink);
                Card(new Rect(x+10,276,243,135),i%2==0?new Color(.80f,.91f,.89f):new Color(.96f,.85f,.70f));
                BlueprintIcon(new Rect(x+20,283,222,110),i);
                Label(x+16,410,237,64,plan.Name,heading,textInk);
                Label(x+16,482,237,64,plan.Description,small,muted);
                Label(x+16,560,240,25,plan.Width+" x "+plan.Depth+" blocks  /  "+plan.Height+" high",small,cyan);
                if(Button(x,614,263,42,Session.Player.Creative?"Preview & place":"Creative blueprint",Session.Player.Creative?cyan:muted)&&Session.Player.Creative)Session.Builder.Select(i);
            }
            if(Button(87,674,245,32,"Back to expedition",cyan))Session.Resume();
            Label(355,675,840,32,"A red preview means the site is occupied or uneven. Z undoes an unchanged house.",small,muted);
        }
        void BlueprintIcon(Rect r,int kind)
        {
            // Block diagrams communicate the roof and wall palette without loading extra textures.
            Block wall=kind==0?Block.WhitePlaster:kind==1?Block.StoneBricks:kind==2?Block.Glass:Block.CarvedStone;
            Block roof=kind==0?Block.RedRoof:kind==1?Block.SlateTiles:kind==2?Block.Copper:Block.StoneBricks;
            int columns=kind==3?5:9,rows=kind==3?6:4;float unit=kind==3?13:17,x0=r.center.x-columns*unit*.5f;
            for(int y=0;y<rows;y++)for(int x=0;x<columns;x++)DrawItem(new Rect(x0+x*unit,r.y+36+y*unit,unit,unit),x==columns/2&&y>rows-3?(int)Block.DarkTimber:(int)wall);
            for(int x=0;x<columns+2;x++)DrawItem(new Rect(x0+(x-1)*unit,r.y+20-Mathf.Min(x,columns+1-x)*4,unit,unit),(int)roof);
            if(kind<3){DrawItem(new Rect(x0+unit,r.y+55,unit,unit),(int)Block.Glass);DrawItem(new Rect(x0+(columns-2)*unit,r.y+55,unit,unit),(int)Block.Glass);}
        }
    }
}
