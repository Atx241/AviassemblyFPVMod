using BepInEx;
using UnityEngine;
namespace AviassemblyMod
{
    public class FPVCamera : MonoBehaviour
    {
        public CameraController ccontroller;
        //I put this code in Update in case some other game behaviour tries to override the camera position. It's probably not necessary, so if performance becomes a problem I can put it in Start()
        public void Update()
        {
            if (!ccontroller.enabled)
            {
                transform.localPosition = Vector3.forward * 0.05f;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            if (UnityInput.Current.GetKeyDown("l"))
            {
                ccontroller.enabled = !ccontroller.enabled;
            }
        }
    }
}
