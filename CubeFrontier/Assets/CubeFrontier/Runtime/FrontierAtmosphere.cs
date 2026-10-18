using UnityEngine;
namespace CubeFrontier
{
 public sealed class FrontierAtmosphere:MonoBehaviour
 {
  public Transform Target;
  Transform clouds;
  void Start(){
   var mesh=new ArtMesh();var rng=new System.Random(1543);
   for(int i=0;i<28;i++){
    float x=(float)rng.NextDouble()*220-110,z=(float)rng.NextDouble()*220-110;
    for(int j=0;j<3;j++)mesh.Box(new Vector3(x+j*4,0+j%2*.5f,z+(j%2)*3),new Vector3(8+j*2,1.2f,5+j*2),new Color(.88f,.94f,.97f));
   }
   clouds=mesh.Build(transform,"Drifting block clouds",Vector3.zero,Resources.Load<Material>("FrontierCloud"));
   clouds.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  }
  void Update(){if(Target&&clouds)clouds.position=new Vector3(Target.position.x+Mathf.Sin(Time.time*.004f)*14,65,Target.position.z);}
 }
}
