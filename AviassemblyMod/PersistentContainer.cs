using UnityEngine;

namespace AviassemblyMod
{
    public class PersistentContainer : MonoBehaviour
    {
        public void Update()
        {
            foreach (Transform child in transform)
            {
                child.localPosition = Vector3.zero;
            }
        }
    }
}
