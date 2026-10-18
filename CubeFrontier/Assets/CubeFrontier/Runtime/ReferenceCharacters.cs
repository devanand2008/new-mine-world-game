using UnityEngine;

namespace CubeFrontier
{
    public static class ReferenceCharacters
    {
        public static readonly string[] Names = { "Wilderness Explorer", "Forest Ranger", "Shadow Assassin", "Royal Knight", "Arc-Tech Engineer", "Desert Nomad", "Frost Mage", "Verdant Guardian", "Sky Explorer", "Wandering Merchant", "Crimson Arena Fighter", "Lunar Explorer", "Eclipse Sentinel" };
        public static readonly string[] Details = {
            "Teal trail coat / orange expedition pack / crystal pickaxe",
            "Moss hood / layered woodland cloak / emerald bow",
            "Charcoal hood and face wrap / violet twin daggers",
            "Navy plate / gold borders / cyan visor, sword and shield",
            "Copper workwear / goggles / power pack and mechanical pick",
            "Sand hood / rust scarf / water flask and crescent blade",
            "White hair / ice-blue robes / crystal crown and frost staff",
            "Cream and sage robes / living vines / branching emerald staff",
            "Burgundy hood / cream fur / segmented glider and gear pack",
            "Broad hat / white beard / laden pack, potions and lantern",
            "Crimson hair and scarf / steel armour / axe and round shield",
            "Midnight-blue robes / crescent crown / lunar staff and orbs",
            "Obsidian plate / violet core / horned helm and halberd"
        };
        public static int Selected { get { return Mathf.Clamp(PlayerPrefs.GetInt("character-appearance",0),0,Names.Length-1); } }
        static readonly Color Skin=Hex("EAB083"), Leather=Hex("67452E"), Metal=Hex("87969A"), Dark=Hex("272C35"), Cyan=Hex("69E9F8"), Gold=Hex("D8A24F");
        internal static Color Hex(string value) { Color c; ColorUtility.TryParseHtmlString("#"+value,out c); return c; }
        internal static void B(ArtMesh m,float x,float y,float z,float w,float h,float d,Color c,float rz=0,bool glow=false)
        { m.Box(new Vector3(x,y,z),new Vector3(w,h,d),c,new Vector3(0,0,rz),glow); }
        static void Buckle(ArtMesh m,float x,float y,float z,float size=.07f)
        { B(m,x,y,z,size,size,.025f,Metal); B(m,x,y,z+.017f,size*.5f,size*.48f,.015f,Leather); }
        public static GameObject Create(Transform parent,int style)
        {
            style=Mathf.Clamp(style,0,Names.Length-1);
            var root=new GameObject(Names[style]); root.layer=parent.gameObject.layer; root.transform.SetParent(parent,false);
            var rig=root.AddComponent<ArtRig>();
            Color[] coats={Hex("235963"),Hex("52613A"),Hex("282735"),Hex("283F66"),Hex("D17A30"),Hex("B6A078"),Hex("78B7D7"),Hex("CBC8A2"),Hex("813D48"),Hex("8A6748"),Hex("8C303A"),Hex("273D69"),Hex("292B41")};
            Color coat=coats[style], accent=style==2||style==12?Hex("B879EF"):style==1||style==7?Hex("9CDD5E"):style==10?Hex("F1A04F"):Cyan;
            Color hair=style==6||style==7||style==8||style==9?Hex("E4DDD2"):style==10?Hex("A6382C"):Hex("4D2D20");
            bool armor=style==3||style==10||style==12, hood=style==1||style==2||style==5||style==6||style==7||style==8||style==11;
            bool dress=style==6||style==7||style==11;
            var body=new ArtMesh();
            B(body,0,1.035f,0,.48f,.55f,.29f,coat);
            B(body,0,1.04f,.157f,.24f,.48f,.035f,armor?Dark:Hex("D7CFB5"));
            // Open lapels, quilted coat panels, seams, belt and pouches.
            for(int s=-1;s<=1;s+=2) {
                B(body,s*.175f,1.10f,.179f,.12f,.45f,.06f,coat*1.16f,s*8);
                B(body,s*.21f,.80f,.035f,.16f,.24f,.34f,coat*.86f,s*7);
                B(body,s*.17f,1.38f,.05f,.135f,.17f,.34f,coat*1.17f,s*23);
                B(body,s*.16f,1.07f,.224f,.045f,.48f,.025f,style==0?Hex("D58A35"):Leather,s*-5);
                Buckle(body,s*.16f,1.19f,.243f);
                B(body,s*.24f,.84f,.12f,.13f,.16f,.12f,Leather); Buckle(body,s*.24f,.88f,.187f,.05f);
                for(int j=0;j<4;j++) B(body,s*.20f,.95f+j*.065f,.22f,.045f,.019f,.018f,Metal*.8f);
            }
            B(body,0,.84f,.016f,.52f,.06f,.35f,Leather); Buckle(body,0,.84f,.211f,.10f);
            B(body,0,1.365f,0,.16f,.16f,.16f,armor?Dark:Skin);
            B(body,0,1.58f,.018f,.40f,.40f,.35f,Skin);
            B(body,0,1.475f,.191f,.25f,.13f,.04f,Skin*1.035f);
            B(body,0,1.55f,.213f,.065f,.075f,.05f,Skin*.96f);
            B(body,0,1.465f,.223f,.10f,.018f,.014f,Hex("9F5E42"));
            for(int s=-1;s<=1;s+=2) {
                B(body,s*.211f,1.56f,.015f,.075f,.115f,.10f,Skin);
                B(body,s*.096f,1.609f,.203f,.092f,.087f,.025f,Hex("FFF3DA"));
                B(body,s*.084f,1.60f,.22f,.047f,.066f,.018f,style==2?accent:Hex("285458"));
                B(body,s*.08f,1.608f,.232f,.023f,.052f,.009f,Dark);
                B(body,s*.071f,1.628f,.239f,.016f,.021f,.007f,Color.white);
                B(body,s*.097f,1.674f,.211f,.105f,.033f,.023f,hair,s*9);
            }
            if(style!=3&&style!=12) {
            B(body,0,1.795f,-.012f,.44f,.115f,.39f,hair);
            for(int i=0;i<16;i++) {
                float x=(i%5-2)*.082f, y=hood?1.77f:1.79f+(i/5)*.045f, z=-.13f+(i%3)*.12f;
                B(body,x,y,z,.14f,.095f,.16f,hair*Mathf.Lerp(.76f,1.25f,(i%4)/3f),(i%3-1)*19);
            }
            for(int s=-1;s<=1;s+=2) B(body,s*.196f,1.704f,.075f,.09f,.20f,.25f,hair,s*7);
            B(body,-.06f,1.742f,.219f,.13f,.13f,.08f,hair,27);
            }
            if(hood) {
                B(body,0,1.86f,-.02f,.53f,.105f,.48f,coat);
                B(body,0,1.63f,-.205f,.52f,.42f,.10f,coat);
                for(int s=-1;s<=1;s+=2) {
                    B(body,s*.243f,1.66f,-.01f,.09f,.38f,.44f,coat,s*5);
                    B(body,s*.23f,1.70f,.226f,.049f,.31f,.04f,style==8?Hex("DED3BB"):coat*1.3f,s*6);
                }
                B(body,0,1.862f,.231f,.48f,.06f,.04f,style==8?Hex("DED3BB"):coat*1.3f);
                // Overlapping cloak strips keep the silhouette visibly stepped.
                for(int i=0;i<7;i++) B(body,(i-3)*.078f,.965f,-.226f,.087f,.70f+(.08f*(i%2)),.06f,coat*(.83f+(i%3)*.075f));
            }
            if(style==2) { B(body,0,1.493f,.213f,.38f,.13f,.035f,Dark); B(body,0,1.38f,.08f,.47f,.12f,.39f,coat); }
            if(dress) {
                for(int i=0;i<8;i++) {
                    float a=i*Mathf.PI/4;
                    body.Box(new Vector3(Mathf.Sin(a)*.21f,.55f,Mathf.Cos(a)*.16f),new Vector3(.19f,.59f,.11f),coat*(.83f+(i%3)*.10f),new Vector3(0,i*45,-Mathf.Sin(a)*9));
                    B(body,Mathf.Sin(a)*.24f,.28f,Mathf.Cos(a)*.20f,.13f,.065f,.10f,style==7?Hex("658245"):Hex("DBE9E6"));
                }
                for(int s=-1;s<=1;s+=2) B(body,s*.16f,1.46f,-.12f,.10f,.46f,.15f,hair,s*8);
            }
            if(armor) {
                for(int row=0;row<3;row++) for(int s=-1;s<=1;s+=2) {
                    B(body,s*.122f,1.24f-row*.12f,.217f,.225f,.14f,.075f,coat*(1.22f-row*.12f),s*(8-row*3));
                    B(body,s*.127f,1.30f-row*.12f,.258f,.20f,.025f,.025f,style==10?Metal:Gold,s*5);
                }
                body.Crystal(new Vector3(0,1.15f,.29f),.065f,.17f,accent);
                for(int s=-1;s<=1;s+=2) B(body,s*.12f,.65f,.15f,.21f,.27f,.09f,coat,s*8);
                if(style!=10) {
                    B(body,0,1.64f,.027f,.51f,.48f,.43f,coat);
                    B(body,0,1.67f,.238f,.35f,.068f,.025f,Dark);
                    B(body,0,1.67f,.255f,.31f,.027f,.018f,accent,0,true);
                    B(body,0,1.57f,.247f,.038f,.18f,.03f,accent,0,true);
                    B(body,0,1.805f,.04f,.065f,.16f,.46f,style==3?Gold:Metal);
                    for(int s=-1;s<=1;s+=2) { B(body,s*.184f,1.535f,.229f,.076f,.17f,.07f,coat*1.3f,s*12); if(style==12) body.Crystal(new Vector3(s*.235f,1.92f,-.08f),.073f,.45f,coat*1.6f,new Vector3(0,0,s*-25)); }
                }
                for(int i=0;i<7;i++) B(body,(i-3)*.08f,.91f,-.237f,.082f,.88f+(i%2)*.06f,.055f,coat*.85f);
                B(body,0,.47f,-.272f,.48f,.045f,.035f,Gold);
            }
            if(style==4||style==5||style==8) {
                B(body,0,1.805f,.247f,.42f,.065f,.035f,Leather);
                for(int s=-1;s<=1;s+=2) { B(body,s*.112f,1.81f,.27f,.155f,.125f,.07f,Gold); B(body,s*.112f,1.81f,.313f,.111f,.081f,.025f,accent,0,true); }
            }
            if(style==5||style==9||style==10) {
                B(body,0,1.37f,.055f,.47f,.115f,.38f,Hex("A64732"));
                B(body,-.12f,1.17f,.248f,.12f,.32f,.04f,Hex("A64732"),-12);
            }
            if(style==5) { for(int s=-1;s<=1;s+=2) B(body,s*.155f,.60f,0,.27f,.49f,.40f,coat,s*7); B(body,.32f,.94f,-.06f,.14f,.24f,.14f,Cyan*.58f); }
            if(style==7) for(int i=0;i<12;i++) B(body,Mathf.Sin(i*2.1f)*.23f,.58f+i*.092f,.245f,.095f,.035f,.05f,Hex("698748"),i*39);
            if(style==9) {
                B(body,0,1.872f,.03f,.76f,.065f,.65f,Leather); B(body,0,1.982f,-.01f,.43f,.19f,.39f,coat);
                B(body,0,1.92f,-.005f,.45f,.06f,.41f,Leather);
                for(int i=0;i<5;i++) B(body,(i-2)*.055f,1.425f+Mathf.Abs(i-2)*.034f,.233f,.064f,.18f,.075f,hair);
            }
            if(style==11) Crescent(body,new Vector3(0,1.925f,.16f),.13f,Gold);
            if(style==0||style==1||style==4||style==8||style==9) Pack(body,style,coat,accent);
            if(style==8) for(int s=-1;s<=1;s+=2) for(int j=0;j<5;j++) {
                float x=s*(.32f+j*.20f), y=1.30f+j*.065f;
                B(body,x,y,-.39f,.235f,.36f-j*.035f,.05f,j%2==0?Hex("DBCFAE"):coat,s*14);
                B(body,x,y+.16f,-.35f,.245f,.045f,.07f,Leather,s*14);
            }
            body.Build(root.transform,"Layered clothing, face and equipment");
            for(int s=-1;s<=1;s+=2) {
                var arm=new ArtMesh();
                B(arm,s*.327f,1.13f,0,.20f,.39f,.25f,coat,s*5);
                B(arm,s*.34f,.902f,.025f,.165f,.20f,.185f,armor?Metal:Skin);
                B(arm,s*.35f,.851f,.03f,.20f,.085f,.21f,Dark);
                B(arm,s*.35f,.765f,.045f,.18f,.14f,.20f,Dark);
                B(arm,s*.35f,.764f,.152f,.115f,.065f,.035f,armor?Dark:Skin);
                for(int j=0;j<3;j++) B(arm,s*.34f,1.28f-j*.093f,.023f,.26f-j*.014f,.105f,.31f,coat*(1.12f-j*.09f),s*7);
                B(arm,s*.345f,.913f,.144f,.16f,.07f,.045f,armor?Metal:coat*1.2f); Buckle(arm,s*.35f,.848f,.151f,.05f);
                if(armor) for(int j=0;j<3;j++) B(arm,s*(.34f+j*.013f),1.34f-j*.055f,0,.31f,.08f,.35f, j==0?Gold:coat*1.2f,s*14);
                Weapon(arm,style,s,accent,coat);
                Transform at=arm.Build(root.transform,s<0?"Left shoulder":"Right shoulder",new Vector3(s*.30f,1.31f,0));
                if(s<0) rig.LeftArm=at; else rig.RightArm=at;
                var leg=new ArtMesh();
                B(leg,s*.135f,.465f,0,.215f,.53f,.255f,Dark);
                for(int j=0;j<3;j++) B(leg,s*.14f,.65f-j*.14f,.126f,.205f,.12f,.055f,armor?coat:Dark*(1.12f+j*.07f));
                B(leg,s*.14f,.15f,.036f,.245f,.25f,.33f,Leather*.65f);
                B(leg,s*.14f,.072f,.075f,.27f,.10f,.38f,Hex("A39A84"));
                B(leg,s*.14f,.039f,.082f,.28f,.042f,.39f,Dark);
                B(leg,s*.14f,.234f,.14f,.235f,.05f,.06f,style==0?Gold:Metal); Buckle(leg,s*.14f,.234f,.181f,.052f);
                for(int j=0;j<3;j++) B(leg,s*.14f,.125f+j*.032f,.21f,.12f,.014f,.015f,Gold*.8f);
                Transform lt=leg.Build(root.transform,s<0?"Left hip":"Right hip",new Vector3(s*.135f,.75f,0));
                if(s<0) rig.LeftLeg=lt; else rig.RightLeg=lt;
            }
            return root;
        }
        static void Pack(ArtMesh m,int style,Color coat,Color accent)
        {
            Color fabric=style==0?Hex("D17D26"):style==4?Hex("9A653C"):coat;
            B(m,0,1.09f,-.285f,.46f,.53f,.25f,Leather);
            B(m,0,1.09f,-.423f,.44f,.48f,.08f,fabric);
            B(m,0,1.285f,-.447f,.46f,.15f,.10f,fabric*1.15f);
            for(int s=-1;s<=1;s+=2) {
                B(m,s*.148f,1.10f,-.48f,.047f,.47f,.025f,Leather); Buckle(m,s*.148f,1.18f,-.50f);
                B(m,s*.264f,.99f,-.32f,.115f,.23f,.19f,fabric); B(m,s*.264f,1.075f,-.326f,.127f,.05f,.20f,Leather);
            }
            B(m,0,.93f,-.488f,.21f,.17f,.065f,fabric*1.15f); Buckle(m,0,.95f,-.53f);
            B(m,0,1.45f,-.31f,.51f,.16f,.19f,Hex("837B69"));
            for(int s=-1;s<=1;s+=2) B(m,s*.17f,1.45f,-.31f,.045f,.175f,.205f,Leather);
            B(m,.30f,1.16f,-.34f,.10f,.20f,.11f,accent,0,true);
            B(m,.30f,1.275f,-.34f,.13f,.035f,.14f,Metal);
            B(m,.30f,1.045f,-.34f,.13f,.035f,.14f,Metal);
            if(style==4||style==8) for(int s=-1;s<=1;s+=2) {
                B(m,s*.18f,1.30f,-.48f,.115f,.33f,.13f,Dark);
                B(m,s*.18f,1.31f,-.557f,.06f,.22f,.033f,Cyan,0,true);
            }
            if(style==1) for(int i=0;i<5;i++) {
                m.Bar(new Vector3(.19f+i*.025f,1.12f,-.38f),new Vector3(.32f+i*.025f,1.68f,-.38f),.02f,Leather);
                B(m,.32f+i*.025f,1.67f,-.38f,.045f,.10f,.05f,accent,-15);
            }
            if(style==9) for(int i=0;i<5;i++) {
                B(m,(i-2)*.085f,1.66f,-.34f,.08f,.27f,.21f,i%2==0?Hex("874643"):Hex("467073"));
                B(m,(i-2)*.10f,.86f,-.54f,.067f,.12f,.055f,i%2==0?accent:Hex("E29354"),0,true);
            }
        }
        static void Weapon(ArtMesh m,int style,int s,Color accent,Color coat)
        {
            float x=s*.43f;
            if(style==2) { m.Bar(new Vector3(x,.77f,.17f),new Vector3(x,.43f,.38f),.05f,Leather); m.Crystal(new Vector3(x,.42f,.39f),.055f,.38f,accent,new Vector3(-28,0,0)); return; }
            if((style==3||style==10)&&s<0) {
                if(style==3) { B(m,x,.77f,.32f,.39f,.61f,.075f,Gold); B(m,x,.79f,.366f,.32f,.48f,.035f,coat); m.Crystal(new Vector3(x,.80f,.397f),.075f,.23f,accent); }
                else for(int i=0;i<8;i++) B(m,x+Mathf.Sin(i*Mathf.PI/4)*.16f,.78f+Mathf.Cos(i*Mathf.PI/4)*.16f,.31f,.16f,.16f,.065f,i%2==0?coat:Metal,i*45);
                return;
            }
            if(s<0) return;
            if(style==0||style==4) {
                m.Bar(new Vector3(x,.46f,.10f),new Vector3(x,1.13f,.37f),.065f,Leather);
                B(m,x,1.11f,.36f,.13f,.14f,.14f,Dark);
                for(int k=-1;k<=1;k+=2) for(int j=0;j<4;j++)
                    B(m,x+k*(.10f+j*.07f),1.15f-j*j*.022f,.38f,.12f,.085f,.08f,style==0?Cyan:Metal,-k*j*16,style==0);
                B(m,x,1.13f,.444f,.06f,.055f,.025f,accent,0,true);
            } else if(style==1) {
                Vector3 last=new Vector3(x,.38f,.25f);
                for(int i=1;i<=8;i++) { float a=i/8f*Mathf.PI; Vector3 p=new Vector3(x+Mathf.Sin(a)*.23f,.38f+i*.095f,.25f); m.Bar(last,p,.045f,i%3==0?accent:Leather,i%3==0); last=p; }
                m.Bar(new Vector3(x,.38f,.25f),new Vector3(x,1.14f,.25f),.008f,Hex("E7D8A7"));
            } else if(style==6||style==7||style==11||style==12) {
                m.Bar(new Vector3(x,.15f,.22f),new Vector3(x,1.64f,.22f),.045f,style==7?Leather:Metal);
                if(style==11) Crescent(m,new Vector3(x,1.75f,.22f),.20f,Gold);
                else if(style==12) { B(m,x,1.64f,.22f,.51f,.08f,.08f,Gold); m.Crystal(new Vector3(x+.19f,1.65f,.22f),.11f,.49f,accent); }
                else { m.Crystal(new Vector3(x,1.78f,.22f),.10f,.35f,accent); for(int k=-1;k<=1;k+=2) m.Bar(new Vector3(x,1.43f,.22f),new Vector3(x+k*.17f,1.78f,.22f),.05f,style==7?Leather:Metal); }
            } else if(style==9) {
                B(m,x,.47f,.23f,.20f,.22f,.20f,Gold); B(m,x,.48f,.23f,.145f,.16f,.21f,Hex("F7B453"),0,true);
                B(m,x,.63f,.23f,.06f,.09f,.06f,Metal);
            } else if(style==8) { m.Bar(new Vector3(x,.72f,.20f),new Vector3(x,.37f,.25f),.06f,Leather); B(m,x,.39f,.25f,.20f,.055f,.09f,Metal); }
            else { m.Bar(new Vector3(x,.47f,.20f),new Vector3(x,1.04f,.20f),.055f,Leather); B(m,x,.90f,.20f,.26f,.045f,.075f,Gold); if(style==10) B(m,x+.10f,1.08f,.20f,.28f,.25f,.05f,Metal,-20); else m.Crystal(new Vector3(x,1.17f,.20f),.065f,.53f,accent,new Vector3(0,0,style==5?-16:0)); }
        }
        static void Crescent(ArtMesh m,Vector3 p,float radius,Color color)
        {
            for(int i=0;i<9;i++) { float a=(i*23+45)*Mathf.Deg2Rad; B(m,p.x+Mathf.Cos(a)*radius,p.y+Mathf.Sin(a)*radius,p.z,.055f,.08f,.04f,color,i*23-45,true); }
        }
    }
}
