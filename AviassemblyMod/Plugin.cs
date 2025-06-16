using System;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

namespace AviassemblyMod
{
    [BepInPlugin("atxmedia.aviassembly.mod", "AtxMedia's Aviassembly Mod", "0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        const string cockpitCamOffsetsFile = "C:\\AviFPV\\offsets.ccof";
        const string builtinCockpitCamOffsetsFile = "GliderCockpit,0,0,0.05\nBiplaneCockpit,0,0.6,2\nCockpit,0,0.7,1.7\nJetCockpit,0,0.6,0.8\nPrivateJetCockpit,0,0.3,0.4\nBigCockpit,0,0.4,0.1\nSmallLargeCockpit,0,0.6,0.75\nJetlineCockpit,0,0.33,1\nJetCockpit02,0,0.3,0.5";

        public static Plugin Instance = null;
        
        Dictionary<string, Vector3> cockpitCamOffsets = new Dictionary<string, Vector3>();
        
        public PartBar fuelPartBar = null;
        public GameObject customPrefabsContainer = null;
        //Shortening of Logger.LogInfo
        public void Log(object data)
        {
            Logger.LogInfo(data);
        }
        //Bepinex calls this function at the start of running the game
        public void Awake()
        {
            Instance = this;
            Log("Hello world from AtxMedia's Aviassembly Mod!");
            Log("Using .NET version " + Environment.Version);
            SceneManager.sceneLoaded += SceneLoad;
            SceneManager.sceneUnloaded += (Scene scene) =>
            {
                Log("Scene unloaded: " + scene.name);
            };
        }
        //This function runs when a new scene is loaded (this also calls GameAwake)
        void SceneLoad(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case "Menu":
                    var mainMenuCanvasObject = GameObject.Find("Menu Canvas");
                    if (mainMenuCanvasObject == null)
                    {
                        break;
                    }
                    var text = new GameObject("AtxMediaCreditText", typeof(Text)).GetComponent<Text>();
                    var textRect = text.rectTransform;
                    text.transform.SetParent(mainMenuCanvasObject.transform);
                    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                    text.fontSize = 30;
                    text.color = Color.black;
                    textRect.anchorMax = Vector2.one;
                    textRect.anchorMin = Vector2.one;
                    textRect.sizeDelta = new Vector2(600, 100);
                    textRect.anchoredPosition = new Vector2(-20, -20);
                    textRect.pivot = Vector2.one;
                    text.alignment = TextAnchor.UpperRight;
                    text.text = "Mods powered by AtxMedia";
                    Log("Created credit text");
                    break;
                case "Persistent":
                    //Create persistent containers
                    customPrefabsContainer = new GameObject("AtxPrefabs", typeof(PersistentContainer));
                    //Move these containers to the persistent scene
                    SceneManager.MoveGameObjectToScene(customPrefabsContainer, scene);
                    //Move these containers to arbitrary positions that will never be reached by the player (hopefully)
                    var nowhere = Vector3.one * -10000000000;
                    customPrefabsContainer.transform.position = nowhere;
                    Log("Created persistent containers");
                    //Inject custom runtime manager
                    var atxman = new GameObject("AtxManager", typeof(AtxManager));
                    //atxman.SetActive(false);
                    SceneManager.MoveGameObjectToScene(atxman, scene);
                    break;
                case "Building":
                    var partBars = FindObjectsByType<PartBar>(FindObjectsSortMode.None);
                    foreach (var partBar in partBars)
                    {
                        if (partBar.gameObject.name == "Fuel")
                        {
                            fuelPartBar = partBar;
                            break;
                        }
                    }
                    CustomParts.fuelPartBar = fuelPartBar;
                    CustomParts.ReloadBuilder();
                    break;
/*                case "Game":
                    foreach (var part in CustomParts.customParts)
                    {
                        part.SetActive(true);
                    }
                    break;*/
                default:
                    break;
            }
            PrintSceneTree(scene);
            var player = FindFirstObjectByType<PlaneContainer>();
            if (player != null)
            {
                GameAwake(player);
            }
        }
        //This function runs when a gameplay scene loads
        void GameAwake(PlaneContainer player)
        {
            PrintObjectTree(player.gameObject);
            //Find all of the camera controllers (used or not)
            var cameraControllers = FindObjectsByType<CameraController>(FindObjectsSortMode.None);
            //Destroy previously used camera controllers (the garbage collector doesn't work very well, so we have to do the destruction of these manually)
            foreach (var controller in cameraControllers)
            {
                //We've used the camera controller if it has a FPVCamera on it
                if (controller.gameObject.GetComponent<FPVCamera>() != null)
                {
                    //DestroyImmediate instead of Destroy so we can instantly refresh the camera controllers array instead of waiting a frame (which isn't super hard to do but this works)
                    DestroyImmediate(controller.gameObject);
                }
            }
            //Refresh the camera controllers array (since we got rid of all the unused controllers)
            cameraControllers = FindObjectsByType<CameraController>(FindObjectsSortMode.None);
            //Log(cameraControllers.Length);  //This line of code is used to double check that we got rid of all the unused camera controllers. I've commented it out for now since its not needed
            //Initialize variables
            CameraController cameraController = null;
            GameObject cameraObject = null;
            //Make sure that the cameraControllers array isn't empty to avoid errors
            if (cameraControllers.Length > 0)
            {
                //Assign the camera controller
                cameraController = cameraControllers[cameraControllers.Length - 1];
            }
            //If we have a singular, non-null camera controller that we can use, we can add an FPV monobehaviour to it
            if (cameraController != null && cameraControllers.Length < 2)
            {
                cameraObject = cameraController.gameObject;
                Log("Found a camera controller (" + cameraObject.name + ")");
                cameraController.enabled = false;
                var target = FindCockpit(player.transform);
                cameraObject.transform.SetParent(target, false);
                cameraObject.AddComponent<FPVCamera>();
                cameraObject.GetComponent<FPVCamera>().ccontroller = cameraController;
                Log("Successfully added FPV camera monobehaviour");
                var targetName = target.name.Replace("(Clone)", "").Trim();
                Log(targetName);
                {
                    var lines = builtinCockpitCamOffsetsFile.Split('\n');
                    cockpitCamOffsets.Clear();
                    foreach (var line in lines)
                    {
                        var segs = line.Split(',');
                        if (segs.Length != 4)
                        {
                            Logger.LogError("Invalid cockpit cam offset entry!");
                            continue;
                        }
                        var name = segs[0];
                        float dx;
                        float dy;
                        float dz;
                        if (float.TryParse(segs[1], out dx) && float.TryParse(segs[2], out dy) && float.TryParse(segs[3], out dz))
                        {
                            cockpitCamOffsets.Add(name, new Vector3(dx, dy, dz));
                        } else
                        {
                            Logger.LogError("Invalid cockpit cam offset numbers!");
                        }
                    }
                }
                if (cockpitCamOffsets.ContainsKey(targetName))
                {
                    cameraObject.GetComponent<FPVCamera>().offset = cockpitCamOffsets[targetName];
                } else
                {
                    Logger.LogError("Cockpit offset not found!");
                }
                //PrintComponents(player.transform.GetChild(13));
            }
        }
        //A utility function for viewing the root objects of scene trees
        public void PrintSceneTree(Scene scene)
        {
            Log("Scene loaded: " + scene.name);
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Log(root.name);
            }
        }
        //A utility function for viewing the direct children of objects (can be used in conjunction with PrintSceneTree)
        public void PrintObjectTree(GameObject obj, int level = 0)
        {
            Log(obj.name);
            foreach (Transform t in obj.transform)
            {
                string o = "";
                for (int i = 0; i < level; i++)
                {
                    o += "  ";
                }
                o += "|_" + t.name;
                Log(o);
            }
        }
        public void PrintObjectTree(Transform obj, int level = 0)
        {
            PrintObjectTree(obj.gameObject, level);
        }
        public void PrintComponents(GameObject obj)
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
        public void PrintComponents(Transform obj)
        {
            PrintComponents(obj.gameObject);
        }
        Transform FindCockpit(Transform parent)
        {
            foreach (var tr in parent)
            {
                var t = tr as Transform;
                if (t.name.Contains("Cockpit"))
                {
                    return t;
                }
            }
            return null;
        }
    }
}
