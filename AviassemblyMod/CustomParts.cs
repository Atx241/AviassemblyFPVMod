using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Object;

namespace AviassemblyMod
{
    public static class CustomParts
    {
        public static List<GameObject> customParts = new List<GameObject>();
        public static PartBar fuelPartBar = null;
        public static void AddParts()
        {
            AddEngine("John", "Engine", 200, "ramjet");
            AddEngine("Steve", "JetEngine", 400, "ramjet", true);
        }
        public static void ReloadBuilder()
        {
            AtxManager.Log("Reloaded part builder");
            var partsList = new List<GameObject>(fuelPartBar.prefabs);
            if (fuelPartBar != null)
            {
                foreach (var part in customParts)
                {
                    if (!partsList.Contains(part))
                    {
                        partsList.Add(part);
                        AddPartButton(part);
                    }
                }
            }
            fuelPartBar.prefabs = partsList.ToArray();
        }
        public static void AddEngine(string name, string rootPrefab, int thrust, string model, bool electric = false)
        {
            var instantiator = PartPrefabs.GetPartPrefab(rootPrefab);
            var obj = Instantiate(instantiator, Plugin.Instance.customPrefabsContainer.transform);
            obj.name = name;
            obj.GetComponent<Engine>().thrust = thrust;
            obj.GetComponent<Engine>().electricEngine = electric;
            var meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = obj.GetComponentInChildren<MeshFilter>();
            }
            var objResult = OBJLoader.Load(AtxManager.modelsPath + "/" + model + ".obj");
            if (objResult.mesh == null)
            {
                AtxManager.Err("Model \"" + model + "\" does not exist.");
                return;
            }
            meshFilter.sharedMesh = objResult.mesh;
            PartPrefabs.GetAllPrefabs().Add(obj);
            customParts.Add(obj);
        }
        public static void AddPartButton(GameObject prefab)
        {
            GameObject obj = Instantiate(fuelPartBar.buttonPrefab);
            obj.transform.SetParent(fuelPartBar.transform);
            obj.transform.localScale = Vector3.one;
            obj.GetComponent<PartButton>().Init(fuelPartBar.placer, prefab, Singleton<IconGenerator>.Instance);
        }
    }
}
