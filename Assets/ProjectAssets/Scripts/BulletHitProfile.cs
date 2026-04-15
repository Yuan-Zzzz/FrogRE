using UnityEngine;

namespace FrogRE
{
    public class BulletHitProfile : MonoBehaviour
    {
        [SerializeField] private int bulletLevel = 1;
        [SerializeField] private bool canPenetrate;

        public bool CanPenetrate => canPenetrate;

        public void SetLevel(int level)
        {
            bulletLevel = Mathf.Max(1, level);
            canPenetrate = bulletLevel >= 2;
        }
    }
}
