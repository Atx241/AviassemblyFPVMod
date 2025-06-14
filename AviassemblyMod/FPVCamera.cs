using BepInEx;
using UnityEngine;
namespace AviassemblyMod
{
    public class FPVCamera : MonoBehaviour
    {
        const float camReturnSpeed = 2f;
        const float camSensitivity = 100;
        public CameraController ccontroller;
        public float yRot = 0;
        public Vector3 offset;
        //I put this code in Update in case some other game behaviour tries to override the camera position. It's probably not necessary, so if performance becomes a problem I can put it in Start()
        public void Update()
        {
            if (!ccontroller.enabled)
            {
                gameObject.GetComponent<Camera>().nearClipPlane = 0.01f;
                transform.localPosition = offset;
                transform.localRotation = Quaternion.Euler(0, yRot, 0);
                if (Input.GetMouseButton(1))
                {
                    yRot += Input.GetAxis("Mouse X") * Time.deltaTime * camSensitivity;
                }
                else
                {
                    yRot = Mathf.Lerp(yRot, 0, Time.deltaTime * camReturnSpeed);
                }
            } else
            {
                gameObject.GetComponent<Camera>().nearClipPlane = 0.3f;
            }
            if (UnityInput.Current.GetKeyDown("l"))
            {
                ccontroller.enabled = !ccontroller.enabled;
            }
        }
    }
}
