using System;
using System.IO;
using System.Runtime.InteropServices;
using CubeFrontier.Core;
using UnityEngine;

namespace CubeFrontier
{
    public sealed class LocalSaves : MonoBehaviour
    {
        public bool Busy { get; private set; }
        public string Status { get; private set; }
        private string directory;
        private void Awake() { directory = Path.Combine(Application.persistentDataPath, "CubeFrontier"); Directory.CreateDirectory(directory); }
        public bool Exists(int slot) { return File.Exists(PathFor(slot)) || File.Exists(PathFor(slot) + ".bak"); }
        private string PathFor(int slot) { if (slot < 0 || slot > 2) throw new ArgumentOutOfRangeException("slot"); return Path.Combine(directory, "world-" + slot + ".cfe"); }
        public WorldSave Read(int slot)
        {
            string path = PathFor(slot);
            try { return SaveCodec.Decode(File.ReadAllBytes(path)); }
            catch (Exception original) {
                if (!File.Exists(path + ".bak")) throw new IOException("Save could not be loaded: " + original.Message, original);
                var recovered = SaveCodec.Decode(File.ReadAllBytes(path + ".bak")); Status = "Recovered previous save snapshot."; return recovered;
            }
        }
        public void Write(int slot, WorldSave save)
        {
            if (Busy) return;
            try {
                byte[] bytes = SaveCodec.Encode(save); string path = PathFor(slot), temp = path + ".tmp";
                File.WriteAllBytes(temp, bytes);
                // Keep a verified previous version. A failed write can be recovered on next load.
                if (File.Exists(path)) {
                    bool valid = true;
                    try { SaveCodec.Decode(File.ReadAllBytes(path)); }
                    catch (InvalidDataException) { valid = false; }
                    catch (ArgumentException) { valid = false; }
                    if (valid) File.Copy(path, path + ".bak", true);
                }
                File.Copy(temp, path, true); File.Delete(temp);
                Busy = true; Status = "Writing save...";
#if UNITY_WEBGL && !UNITY_EDITOR
                FrontierSync(gameObject.name);
#else
                SaveSynced("");
#endif
            } catch (Exception e) { Busy = false; Status = "Save failed: " + e.Message; Debug.LogException(e); }
        }
        public void SaveSynced(string error) { Busy = false; Status = string.IsNullOrEmpty(error) ? "Saved to this device." : "Browser save failed: " + error; }
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void FrontierSync(string objectName);
#endif
    }
}
