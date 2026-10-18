using UnityEngine;
namespace CubeFrontier
{
    public sealed class CharacterGallery : MonoBehaviour
    {
        public RenderTexture Image { get; private set; }
        public int Index { get; private set; }
        public bool Bestiary { get; private set; }
        public bool Rotate = true;
        GameObject stage, model; Camera view;
        public void Show(int index,bool bestiary)
        {
            Index=index; Bestiary=bestiary;
            if(!stage) {
                stage=new GameObject("Character studio");stage.layer=30;stage.transform.position=new Vector3(10000,1000,10000);
                Image=new RenderTexture(640,640,24,RenderTextureFormat.ARGB32);Image.antiAliasing=2;Image.Create();
                var cameraObject=new GameObject("Turnaround camera");cameraObject.transform.SetParent(stage.transform,false);view=cameraObject.AddComponent<Camera>();
                view.cullingMask=1<<30;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.77f,.91f,.94f);view.targetTexture=Image;view.orthographic=true;view.nearClipPlane=.1f;view.farClipPlane=20;
                var lamp=new GameObject("Studio key light");lamp.transform.SetParent(stage.transform,false);lamp.transform.localRotation=Quaternion.Euler(35,-30,0);
                var light=lamp.AddComponent<Light>();light.type=LightType.Directional;light.color=new Color(1,.91f,.79f);light.intensity=1.3f;light.cullingMask=1<<30;
                var platform=new ArtMesh();platform.Box(new Vector3(0,-.07f,0),new Vector3(2.4f,.12f,2.4f),new Color(.62f,.77f,.70f));platform.Build(stage.transform,"Display plinth");
            }
            stage.SetActive(true);
            if(model){model.SetActive(false);Destroy(model);}
            model=bestiary?ReferenceCreatures.Create(stage.transform,index):ReferenceCharacters.Create(stage.transform,index);
            foreach(var rig in model.GetComponentsInChildren<ArtRig>())rig.Preview=true;
            model.transform.localRotation=Quaternion.Euler(0,-18,0);
            float center=bestiary?(index==2?.65f:index==7?.65f:index==8?1.0f:1.48f):1.02f;
            view.orthographicSize=bestiary?(index==2?1.4f:index==7?1.1f:index==8?1.65f:1.93f):1.25f;
            view.transform.localPosition=new Vector3(3.1f,center+1.3f,6.7f);view.transform.LookAt(stage.transform.position+Vector3.up*center);
        }
        public void Hide(){if(stage)stage.SetActive(false);}
        void Update(){if(stage&&stage.activeSelf&&model&&Rotate)model.transform.Rotate(0,Time.unscaledDeltaTime*12,0);}
        public void Turn(float degrees){if(model)model.transform.Rotate(0,degrees,0);}
        void OnDestroy(){if(stage)Destroy(stage);if(Image){Image.Release();Destroy(Image);}}
    }
}
