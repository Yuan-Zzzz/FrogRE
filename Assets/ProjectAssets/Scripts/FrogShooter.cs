using UnityEngine;
using QFramework;
using DG.Tweening;

namespace FrogRE
{
    public class FrogShooter : MonoBehaviour, IController
    {
        [Header("Bullet Settings")]
        [SerializeField] private GameObject bulletPrefab1;
        [SerializeField] private GameObject bulletPrefab2;
        [SerializeField] private GameObject bulletPrefab3;

        [Header("Shooting Settings")]
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float shootInterval = 1f;
        [SerializeField] private int minimumHungerToShoot = 2;

        [Header("Recoil Settings")]
        [SerializeField] private float recoilForce = 5f;
        [SerializeField] private bool enableRecoil = true;

        [Header("Scale Settings")]
        [SerializeField] private float scaleDuration = 0.3f;
        
        [Header("Body Feedback")]
        [SerializeField] private MeshRenderer[] bodyRenderers;
        [SerializeField] private Color readyColor = Color.red;
        [SerializeField] private Color normalColor = Color.green;
        [SerializeField] private float bodyColorTweenDuration = 0.2f;

        [Header("Fire Point Offset")]
        [SerializeField] private Vector3 firePointOffset = new Vector3(0, 0.5f, 0.5f);

        private Rigidbody rb;
        private IFrogDataModel frogDataModel;
        private float shootTimer;
        private bool isReadyToShoot;

        public IArchitecture GetArchitecture()
        {
            return FrogRE.Interface;
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            frogDataModel = this.GetModel<IFrogDataModel>();
            shootTimer = shootInterval;

            if (firePoint == null)
            {
                GameObject firePointObj = new GameObject("FirePoint");
                firePointObj.transform.SetParent(transform);
                firePointObj.transform.localPosition = firePointOffset;
                firePoint = firePointObj.transform;
            }

            frogDataModel.Hunger.RegisterWithInitValue(OnHungerChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnHungerChanged(int currentHunger)
        {
            float targetScaleValue = Mathf.Max(1f, currentHunger + 1f);
            transform.DOScale(Vector3.one * targetScaleValue, scaleDuration)
                .SetEase(Ease.OutBack);
        }

        private void Update()
        {
            UpdateShootCooldown();
            UpdateReadyVisual();
            HandleShootInput();
        }

        public void ApplyJumpShootBonus(float bonusTime)
        {
            if (bonusTime <= 0f)
            {
                return;
            }

            shootTimer += bonusTime;
            if (shootTimer > shootInterval)
            {
                shootTimer = shootInterval;
            }
        }

        private void UpdateShootCooldown()
        {
            shootTimer += Time.deltaTime;
            if (shootTimer > shootInterval)
            {
                shootTimer = shootInterval;
            }
        }

        private void UpdateReadyVisual()
        {
            bool shouldBeReady = CanShootByHunger() && Mathf.Approximately(shootTimer, shootInterval);
            if (shouldBeReady == isReadyToShoot)
            {
                return;
            }

            isReadyToShoot = shouldBeReady;
            var color = isReadyToShoot ? readyColor : normalColor;
            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                if (bodyRenderers[i] == null)
                {
                    continue;
                }

                bodyRenderers[i].material.DOColor(color, bodyColorTweenDuration);
            }
        }

        private void HandleShootInput()
        {
            if (!Input.GetMouseButtonUp(0))
            {
                return;
            }

            if (!Mathf.Approximately(shootTimer, shootInterval))
            {
                return;
            }

            if (!CanShootByHunger())
            {
                transform.DOShakePosition(0.2f, 0.1f);
                return;
            }

            Shoot();
        }

        private void Shoot()
        {
            int hungerBeforeShot = frogDataModel.Hunger.Value;
            var bulletIndex = ResolveBulletIndexFromHunger(hungerBeforeShot);
            GameObject bulletPrefab = GetSelectedBullet(bulletIndex);
            if (bulletPrefab == null)
            {
                return;
            }

            this.SendCommand<ShootCommand>();

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            var hitProfile = bullet.GetComponent<BulletHitProfile>();
            if (hitProfile == null)
            {
                hitProfile = bullet.AddComponent<BulletHitProfile>();
            }
            hitProfile.SetLevel(bulletIndex);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.isKinematic = true;
                bulletRb.useGravity = false;
            }

            var bulletMover = bullet.GetComponent<BulletTransformMover>();
            if (bulletMover == null)
            {
                bulletMover = bullet.AddComponent<BulletTransformMover>();
            }
            bulletMover.Initialize(firePoint.forward, bulletSpeed);

            if (enableRecoil && rb != null)
            {
                ApplyRecoil();
            }

            shootTimer = 0f;
            Destroy(bullet, 5f);
        }
        
        private bool CanShootByHunger()
        {
            return frogDataModel != null && frogDataModel.Hunger.Value >= minimumHungerToShoot;
        }

        private int ResolveBulletIndexFromHunger(int hunger)
        {
            if (hunger >= 5) return 3;
            if (hunger >= 3) return 2;
            return 1;
        }

        private GameObject GetSelectedBullet(int bulletIndex)
        {
            switch (bulletIndex)
            {
                case 1: return bulletPrefab1;
                case 2: return bulletPrefab2;
                case 3: return bulletPrefab3;
                default: return bulletPrefab1;
            }
        }

        private void ApplyRecoil()
        {
            Vector3 recoilDirection = -transform.forward;
            rb.AddForce(recoilDirection * recoilForce, ForceMode.Impulse);
        }
    }
}
