using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace AviassemblyMod
{
    public class AtxManager : MonoBehaviour
    {
        public static Mesh testMesh;
        public static string modelsPath;
        public void Awake()
        {
            StartCoroutine(IEStart());
        }
        public IEnumerator IEStart()
        {
            Log("Initialized AtxManager");
            modelsPath = Environment.CurrentDirectory + "/BepInEx/plugins/AviFPV";
            Directory.CreateDirectory(modelsPath);
            yield return null;
            CustomParts.fuelPartBar = Plugin.Instance.fuelPartBar;
            //Add a dummy plane container to prevent errors with prefabs
            gameObject.AddComponent<DummyPlaneContainer>();
            CustomParts.AddParts();
            Destroy(gameObject.GetComponent<DummyPlaneContainer>());
            Singleton<PlaneContainer>.InitSingleton();

        }
        public static void Log(object msg)
        {
            Plugin.Instance.Log("ATXMAN: " + msg);
        }
        public static void Err(object msg)
        {
            Plugin.Instance.Err("ATXMAN: " + msg);
        }
        public static void PrintComponents(GameObject obj)
        {
            Log(obj.name);
            foreach (var c in obj.GetComponents<Component>())
            {
                var o = "";
                o += c.ToString();
                o = o.Substring(obj.name.Length + 2);
                o = o.Substring(0, o.Length - 1);
                //If the script can be enabled/disabled, then show that
                if (c is Behaviour)
                {
                    o += " (enabled: ";
                    o += (c as Behaviour).enabled;
                    o += ")";
                }
                Log(o);
            }
        }
    }
}
