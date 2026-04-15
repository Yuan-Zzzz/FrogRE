using System;
using UnityEngine;
using QFramework;
using UniRx;
using DG.Tweening;

namespace FrogRE
{
    /// <summary>
    /// 敌人的行为控制器
    /// </summary>
    public class EnemyBehaviour : MonoBehaviour, IController
    {
        public IArchitecture GetArchitecture() => FrogRE.Interface;

        [Header("Movement")]
        [SerializeField, Tooltip("敌人的移动速度")]
        private float moveSpeed = 3f;
        
        [SerializeField, Tooltip("敌人的停止距离")]
        private float stopDistance = 5f;

        [Header("Combat")]
        [SerializeField, Tooltip("射击间隔")]
        private float shootInterval = 2f;

        [Header("Jump")]
        [SerializeField, Tooltip("跳跃间隔")]
        private float jumpInterval = 4f;
        
        [SerializeField, Tooltip("跳跃力度")]
        private float jumpForce = 15f;
        
        [SerializeField, Tooltip("地面检测射线长度")]
        private float groundCheckLength = 1.1f;

        [Header("Physics")]
        [SerializeField, Tooltip("额外重力倍率，>1 会让敌人下落更快")]
        private float fallGravityMultiplier = 2.5f;
        
        [Header("Target")]
        [SerializeField, Tooltip("玩家位置（可留空，运行时自动查找）")]
        private Transform target;

        [Header("Eaten Effect")]
        [SerializeField, Tooltip("敌人被吃掉时播放的特效预制体")]
        private GameObject eatenVfxPrefab;

        [SerializeField, Tooltip("被吃特效持续时间（秒）")]
        private float vfxLifeTime = 3f;

        private Rigidbody _rb;
        
        private float _timer;
        private float _distance;
        private bool _isGrounded;
        private bool _isEaten;

        private void Awake()
        {
            // TODO: _shooter = GetComponent<Shooter>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            Observable.Interval(TimeSpan.FromSeconds(jumpInterval))
                .Subscribe(_ => PerformJump())
                .AddTo(this);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (target == null)
            {
                target = ResolveTarget();
                if (target == null) return;
            }

            Transform frogTarget = target;
            _distance = Vector3.Distance(frogTarget.position, transform.position);
            
            Vector3 lookPosition = frogTarget.position;
            lookPosition.y = transform.position.y;
            transform.LookAt(lookPosition);

            CheckGround();

            if (_distance > stopDistance && _isGrounded)
            {
                EnemyMove();
            }

            if (_timer >= shootInterval && _isGrounded)
            {
                _timer = 0f;
                Shoot();
            }
        }

        private void FixedUpdate()
        {
            // 给敌人施加额外重力，让下落更利落；地面时不叠加，避免贴地抖动。
            if (_rb != null && !_isGrounded && _rb.velocity.y < 0f && fallGravityMultiplier > 1f)
            {
                Vector3 extraGravity = Physics.gravity * (fallGravityMultiplier - 1f);
                _rb.AddForce(extraGravity, ForceMode.Acceleration);
            }
        }

        private void EnemyMove()
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                target.position,
                moveSpeed * Time.deltaTime
            );
        }

        private void PerformJump()
        {
            transform.DOScaleY(0.5f, 0.2f).OnComplete(() =>
            {
                Vector3 forward = transform.forward;
                forward.y = 1f;
                
                float actualJumpForce = UnityEngine.Random.Range(jumpForce - 5f, jumpForce + 5f);
                if (_rb != null)
                {
                    _rb.velocity = forward * actualJumpForce;
                }
                
                transform.DOScaleY(1f, 0.5f);
            });
        }

        private void Shoot()
        {
            // TODO: 请在此处填写调用射击命令或事件的逻辑
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isEaten || other == null)
            {
                return;
            }

            if (IsPlayerHit(other))
            {
                HandleEaten(null, grantHunger: true);
                return;
            }

            if (TryGetBulletRoot(other, out var bulletRoot))
            {
                HandleEaten(
                    bulletRoot,
                    grantHunger: false,
                    destroyBullet: !CanBulletPenetrate(bulletRoot)
                );
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private Transform ResolveTarget()
        {
            var playerByTag = GameObject.FindGameObjectWithTag("Player");
            if (playerByTag != null) return playerByTag.transform;

            var shooter = FindObjectOfType<FrogShooter>();
            if (shooter != null) return shooter.transform;

            var controller = FindObjectOfType<FrogController>();
            if (controller != null) return controller.transform;

            return null;
        }

        private void CheckGround()
        {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckLength, LayerMask.GetMask("Ground"));
        }

        private bool IsPlayerHit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return true;
            }

            var targetTransform = other.transform;
            return targetTransform.GetComponentInParent<FrogShooter>() != null
                   || targetTransform.GetComponentInParent<FrogController>() != null;
        }

        private bool TryGetBulletRoot(Collider other, out GameObject bulletRoot)
        {
            bulletRoot = null;

            var bulletMover = other.GetComponentInParent<BulletTransformMover>();
            if (bulletMover == null)
            {
                return false;
            }

            bulletRoot = bulletMover.gameObject;
            return true;
        }

        private void HandleEaten(GameObject bulletRoot, bool grantHunger, bool destroyBullet = true)
        {
            _isEaten = true;
            if (grantHunger)
            {
                this.SendCommand<EnemyEatenCommand>();
            }
            else
            {
                this.SendCommand<EnemyShotCommand>();
            }
            SpawnEatenVfx();

            if (bulletRoot != null && destroyBullet)
            {
                Destroy(bulletRoot);
            }

            Destroy(gameObject);
        }

        private bool CanBulletPenetrate(GameObject bulletRoot)
        {
            if (bulletRoot == null)
            {
                return false;
            }

            var profile = bulletRoot.GetComponent<BulletHitProfile>();
            return profile != null && profile.CanPenetrate;
        }

        private void SpawnEatenVfx()
        {
            if (eatenVfxPrefab == null)
            {
                return;
            }

            var vfx = Instantiate(eatenVfxPrefab, transform.position, Quaternion.identity);
            if (vfxLifeTime > 0f)
            {
                Destroy(vfx, vfxLifeTime);
            }
        }

        private void OnDestroy()
        {
            if (EnemyGenerator.Instance != null && EnemyGenerator.Instance.CurrentCount > 0)
            {
                EnemyGenerator.Instance.CurrentCount--;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 position = transform.position;
            position.y += 0.5f;
            Gizmos.DrawRay(position, Vector3.down * groundCheckLength);
        }
    }
}