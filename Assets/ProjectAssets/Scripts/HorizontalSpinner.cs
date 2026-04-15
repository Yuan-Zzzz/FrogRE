using UnityEngine;

namespace FrogRE
{
    public class HorizontalSpinner : MonoBehaviour
    {
        [SerializeField] private float spinSpeed;

        private void Update()
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }
    }
}
