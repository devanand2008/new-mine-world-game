using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using CubeFrontier.Core;

namespace CubeFrontier.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string Root = "Assets/CubeFrontier/Resources";
        static ProjectSetup() { EditorApplication.delayCall += EnsureAssets; }

        private static void EnsureAssets()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall += EnsureAssets; return; }
            if (!File.Exists(Root + "/FrontierVoxel.mat") || !File.Exists(Root + "/FrontierArt.mat")) Prepare();
        }
        [MenuItem("Cube Frontier/Prepare Project")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root); Directory.CreateDirectory("Assets/CubeFrontier/Settings"); AssetDatabase.Refresh();
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/CubeFrontier/Settings/FrontierURP.asset");
            if (!pipeline) {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, "Assets/CubeFrontier/Settings/FrontierRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer); pipeline.msaaSampleCount = 2; pipeline.shadowDistance = 45;
                AssetDatabase.CreateAsset(pipeline, "Assets/CubeFrontier/Settings/FrontierURP.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline; QualitySettings.renderPipeline = pipeline;
            foreach (string concept in new[] { "ExplorerConcept", "WardenConcept" }) {
                string conceptPath = Root + "/" + concept + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(conceptPath);
                if (importer) {
                    importer.textureType = TextureImporterType.Default;
                    importer.textureShape = TextureImporterShape.Texture2D;
                    importer.maxTextureSize = 2048; importer.mipmapEnabled = false;
                    importer.alphaSource = TextureImporterAlphaSource.None;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
                if (!AssetDatabase.LoadAssetAtPath<Texture2D>(conceptPath)) throw new System.InvalidOperationException("Concept must import as Texture2D: " + conceptPath);
            }
            var atlas = BlockAtlas.Generate(Root + "/FrontierAtlas.png");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/FrontierVoxel.mat");
            if (!material) {
                var shader = Shader.Find("CubeFrontier/Voxel"); if (!shader) throw new System.InvalidOperationException("Frontier voxel shader was not imported.");
                material = new Material(shader); material.SetTexture("_BaseMap", atlas); AssetDatabase.CreateAsset(material, Root + "/FrontierVoxel.mat");
            }
            material.SetTexture("_BaseMap", atlas); EditorUtility.SetDirty(material);
            foreach (string name in new[] { "Art", "Water", "Sky", "Cloud", "Glass", "Blueprint", "Weather", "Vista" }) {
                string path = Root + "/Frontier" + name + ".mat";
                var art = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!art) { art = new Material(Shader.Find("CubeFrontier/" + (name == "Cloud" ? "Art" : name))); AssetDatabase.CreateAsset(art, path); }
                if (name == "Cloud") art.SetFloat("_FogWeight", 0);
                if (name == "Glass") art.SetTexture("_BaseMap", atlas);
                EditorUtility.SetDirty(art);
            }
            // Explicitly retain shaders used by generated creature and outline materials in player builds.
            if (!AssetDatabase.LoadAssetAtPath<Material>(Root + "/CreatureShader.mat")) {
                var creatureMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")); creatureMaterial.EnableKeyword("_EMISSION"); creatureMaterial.SetColor("_EmissionColor", Color.cyan);
                AssetDatabase.CreateAsset(creatureMaterial, Root + "/CreatureShader.mat");
            }
            if (!AssetDatabase.LoadAssetAtPath<Material>(Root + "/OutlineShader.mat")) AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Unlit")), Root + "/OutlineShader.mat");
            PlayerSettings.companyName = "Cube Frontier"; PlayerSettings.productName = "Cube Frontier Eclipse"; PlayerSettings.bundleVersion = "0.9.0";
            PlayerSettings.colorSpace = ColorSpace.Linear; PlayerSettings.runInBackground = false;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.template = "PROJECT:Frontier";
            PlayerSettings.defaultWebScreenWidth = 1280; PlayerSettings.defaultWebScreenHeight = 800;
            var settingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (settingsAssets.Length > 0) {
                var settings = new SerializedObject(settingsAssets[0]); var input = settings.FindProperty("activeInputHandler");
                if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/CubeFrontier/Scenes/Frontier.unity", true) };
            AssetDatabase.SaveAssets(); Debug.Log("Cube Frontier assets and Web settings prepared. Open Frontier scene and enter Play mode.");
        }
        [MenuItem("Cube Frontier/Build Web")]
        public static void BuildWeb()
        {
            Prepare();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL)) throw new System.InvalidOperationException("Install Web Build Support for this Unity Editor version.");
            Directory.CreateDirectory("Builds/Web");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/CubeFrontier/Scenes/Frontier.unity" }, locationPathName = "Builds/Web", target = BuildTarget.WebGL, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new System.InvalidOperationException("Web build failed: " + report.summary.result);
            Debug.Log("Web build completed at Builds/Web. Browser verification is still required.");
        }
        [MenuItem("Cube Frontier/Build Windows")]
        public static void BuildWindows()
        {
            Prepare();PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.defaultScreenWidth=1024;PlayerSettings.defaultScreenHeight=640;
            Directory.CreateDirectory("Builds/Windows");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/CubeFrontier/Scenes/Frontier.unity"},locationPathName="Builds/Windows/CubeFrontier.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new System.InvalidOperationException("Windows build failed: "+report.summary.result);
            Debug.Log("Windows build completed at Builds/Windows.");
        }
    }
}

