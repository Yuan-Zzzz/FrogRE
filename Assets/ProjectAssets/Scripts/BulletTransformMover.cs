using UnityEngine;

namespace FrogRE
{
    public class BulletTransformMover : MonoBehaviour
    {
        private Vector3 moveDirection = Vector3.forward;
        private float moveSpeed;

        public void Initialize(Vector3 direction, float speed)
        {
            moveDirection = direction.normalized;
            moveSpeed = speed;
        }

        private void Update()
        {
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }
}
