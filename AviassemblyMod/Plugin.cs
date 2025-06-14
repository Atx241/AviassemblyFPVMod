using System;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using UnityEngine.SceneManagement;

namespace AviassemblyMod
{
    [BepInPlugin("atxmedia.aviassembly.mod", "AtxMedia's Aviassembly Mod", "0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        //Shortening of Logger.LogInfo
        public void Log(object data)
        {
            Logger.LogInfo(data);
        }
        //Bepinex calls this function at the start of running the game
        public void Awake()
        {
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
                    /*Canvas[] canvi = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                    Log("Canvases: ");
                    foreach (var c in canvi)
                    {
                        Log(c.name);
                    }*/
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
                var target = player.transform.GetChild(0);
                cameraObject.transform.SetParent(target, false);
                cameraObject.transform.position = Vector3.zero;
                cameraObject.AddComponent<FPVCamera>();
                cameraObject.GetComponent<FPVCamera>().ccontroller = cameraController;
                Log("Successfully added FPV camera monobehaviour");
                PrintObjectTree(target.GetChild(0));
            }
        }
        //A utility function for viewing the root objects of scene trees
        void PrintSceneTree(Scene scene)
        {
            Log("Scene loaded: " + scene.name);
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                Log(root.name);
            }
        }
        //A utility function for viewing the direct children of objects (can be used in conjunction with PrintSceneTree)
        void PrintObjectTree(GameObject obj, int level = 0)
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
        void PrintObjectTree(Transform obj, int level = 0)
        {
            PrintObjectTree(obj.gameObject, level);
        }
    }
}
