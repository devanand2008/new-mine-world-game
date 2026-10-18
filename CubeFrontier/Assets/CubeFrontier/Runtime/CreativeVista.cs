using UnityEngine;

namespace CubeFrontier
{
    // A small original, real-time voxel model for the start screen.
    // Its own unlit material keeps the preview independent of world time and weather.
    public sealed class CreativeVista : MonoBehaviour
    {
        public RenderTexture Image { get; private set; }
        GameObject stage;
        Transform island;
        Material material;

        public void Show(bool visible)
        {
            if (!stage && visible) Build();
            if (stage) stage.SetActive(visible);
        }
        void Build()
        {
            stage = new GameObject("Creative village preview");
            stage.layer = 29;
            stage.transform.position = new Vector3(12000, 1200, 12000);
            material = new Material(Resources.Load<Material>("FrontierVista"));
            Image = new RenderTexture(900, 690, 24, RenderTextureFormat.ARGB32);
            Image.antiAliasing = 2; Image.Create();
            var cameraObject = new GameObject("Village preview camera");
            cameraObject.transform.SetParent(stage.transform, false);
            var camera = cameraObject.AddComponent<Camera>();
            camera.cullingMask = 1 << 29; camera.targetTexture = Image;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.56f, .82f, .88f);
            camera.orthographic = true; camera.orthographicSize = 9.3f;
            camera.nearClipPlane = .1f; camera.farClipPlane = 80;
            camera.transform.localPosition = new Vector3(19, 17, 23);
            camera.transform.LookAt(stage.transform.position + Vector3.up * 1.3f);
            var mesh = new ArtMesh();
            Color grass = new Color(.49f, .76f, .33f), earth = new Color(.68f, .46f, .30f);
            for (int x = -6; x <= 6; x++) for (int z = -5; z <= 5; z++) {
                if (Mathf.Abs(x) + Mathf.Abs(z) > 9) continue;
                float depth = 1.5f + ((x * x + z * z) % 3) * .35f;
                mesh.Box(new Vector3(x, -depth / 2, z), new Vector3(1, depth, 1), earth);
                mesh.Box(new Vector3(x, .12f, z), new Vector3(1, .24f, 1), grass * (1 - ((x + z + 24) % 4) * .025f));
                if (x == 2 || (z == 3 && x < 3)) mesh.Box(new Vector3(x, .26f, z), new Vector3(.96f, .06f, .96f), new Color(.93f, .81f, .57f));
                if (x < -3 && z > 0 && z < 4) mesh.Box(new Vector3(x, .28f, z), new Vector3(1, .08f, 1), new Color(.27f, .72f, .83f));
            }
            Cottage(mesh, new Vector3(-1, .25f, -1), new Color(.95f, .43f, .32f));
            Tree(mesh, new Vector3(-4, .25f, -3), new Color(.31f, .66f, .39f));
            Tree(mesh, new Vector3(4, .25f, -2), new Color(.95f, .69f, .28f));
            Tree(mesh, new Vector3(4, .25f, 2), new Color(.44f, .72f, .37f));
            // Little bridge, crystal outcrop and a timber supply stack complete the settlement.
            for(int i=0;i<5;i++)mesh.Box(new Vector3(-4.5f,.48f,.8f+i*.55f),new Vector3(2,.16f,.46f),new Color(.73f,.49f,.28f));
            for(int i=0;i<3;i++) {mesh.Box(new Vector3(2.7f+i*.43f,.48f,-4),new Vector3(.38f,.42f,1.4f),new Color(.56f,.35f,.22f));}
            mesh.Crystal(new Vector3(-4,.35f,4),.35f,1.4f,new Color(.29f,.91f,.84f));
            mesh.Crystal(new Vector3(-4.7f,.35f,3.9f),.2f,.8f,new Color(.49f,.76f,.98f));
            for(int i=0;i<4;i++){mesh.Box(new Vector3(2.9f,.32f,3+i*.36f),new Vector3(1.4f,.1f,.22f),new Color(.46f,.30f,.19f));mesh.Box(new Vector3(2.9f,.48f,3+i*.36f),new Vector3(1.1f,.22f,.13f),new Color(.78f,.84f,.35f));}
            for (int i = 0; i < 8; i++) {
                float x = -3 + i * .65f;
                mesh.Box(new Vector3(x, .65f, 4), new Vector3(.12f, .85f, .12f), new Color(.99f, .91f, .72f));
            }
            mesh.Box(new Vector3(-.7f, .8f, 4), new Vector3(4.9f, .12f, .12f), new Color(.99f, .91f, .72f));
            for (int i = 0; i < 13; i++) {
                float x = -3.5f + (i * 7 % 9) * .8f, z = 2.3f + (i % 3) * .4f;
                mesh.Box(new Vector3(x, .47f, z), new Vector3(.08f, .4f, .08f), new Color(.23f, .53f, .3f));
                mesh.Box(new Vector3(x, .69f, z), new Vector3(.28f, .19f, .28f), i % 2 == 0 ? new Color(1, .79f, .25f) : new Color(1, .47f, .55f));
            }
            island = mesh.Build(stage.transform, "Little village", Vector3.zero, material);
            var clouds = new ArtMesh();
            clouds.Box(new Vector3(-7, 6, -5), new Vector3(4, .55f, 1.8f), Color.white);
            clouds.Box(new Vector3(-6.7f, 6.4f, -5), new Vector3(2, .5f, 1.5f), Color.white);
            clouds.Box(new Vector3(5, 7, -6), new Vector3(3, .55f, 1.8f), Color.white);
            clouds.Build(stage.transform, "Little clouds", Vector3.zero, material);
        }
        static void Cottage(ArtMesh m, Vector3 p, Color roof)
        {
            m.Box(p + new Vector3(0, 1.3f, 0), new Vector3(3.6f, 2.6f, 3.2f), new Color(1, .91f, .68f));
            m.Box(p + new Vector3(.65f, .95f, 1.63f), new Vector3(.8f, 1.9f, .15f), new Color(.23f, .53f, .53f));
            m.Box(p + new Vector3(-.9f, 1.5f, 1.65f), new Vector3(.85f, .85f, .16f), new Color(.39f, .79f, .88f));
            m.Box(p + new Vector3(1.83f, 1.5f, 0), new Vector3(.16f, .9f, 1.2f), new Color(.39f, .79f, .88f));
            for (int i = 0; i < 5; i++) m.Box(p + new Vector3(0, 2.7f + i * .32f, 0), new Vector3(4.3f - i * .72f, .34f, 4), roof);
            m.Box(p + new Vector3(-1, 3.8f, -.7f), new Vector3(.5f, 1.3f, .6f), new Color(.94f, .82f, .63f));
        }
        static void Tree(ArtMesh m, Vector3 p, Color leaf)
        {
            m.Box(p + Vector3.up * 1.25f, new Vector3(.45f, 2.5f, .45f), new Color(.58f, .37f, .25f));
            m.Box(p + Vector3.up * 2.8f, new Vector3(2.2f, 1.8f, 2), leaf);
            m.Box(p + new Vector3(.25f, 3.9f, 0), new Vector3(1.5f, .6f, 1.4f), leaf * 1.06f);
        }
        void Update()
        {
            if (stage && stage.activeSelf && island) island.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.unscaledTime * .16f) * 7, 0);
        }
        void OnDestroy()
        {
            if (stage) Destroy(stage);
            if (Image) { Image.Release(); Destroy(Image); }
            if (material) Destroy(material);
        }
    }
}
