using System;
using UnityEngine;
using System.Collections.Generic;
using QFramework;
using UniRx;

namespace FrogRE
{
    /// <summary>
    /// 敌人生成器，使用 QFramework 单例和控制器套件
    /// </summary>
    public class EnemyGenerator : MonoSingleton<EnemyGenerator>, IController
    {
        public IArchitecture GetArchitecture() => FrogRE.Interface;

        [Header("Settings")]
        [SerializeField, Tooltip("敌人预制体")]
        private GameObject enemyPrefab;

        [SerializeField, Tooltip("生成敌人的时间间隔")]
        private float generateInterval = 3f;

        [SerializeField, Tooltip("生成敌人的数量上限")]
        private int generateCount = 10;

        [SerializeField, Tooltip("生成敌人的半径")]
        private Vector2 generateRadius = new Vector2(5f, 10f);
        
        [Header("Target")]
        [SerializeField, Tooltip("玩家位置（可留空，运行时自动查找）")]
        private Transform target;

        private int _currentCount;
        public int CurrentCount
        {
            get => _currentCount;
            set => _currentCount = value;
        }

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            _currentCount = 0;
        }

        private void Start()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[EnemyGenerator] 未配置 enemyPrefab，已停止生成。");
                enabled = false;
                return;
            }

            // 使用 UniRx 处理周期性生成
            var interval = Mathf.Max(0.1f, generateInterval);
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(interval))
                .Subscribe(_ => GenerateEnemy())
                .AddTo(this);
        }

        private void GenerateEnemy()
        {
            if (_currentCount >= generateCount) return;

            if (target == null)
            {
                target = ResolveTarget();
                if (target == null) return;
            }

            Vector3 frogPosition = target.position;

            float radius = UnityEngine.Random.Range(generateRadius.x, generateRadius.y);
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
            
            Vector3 spawnPosition = new Vector3(x, 0f, z) + frogPosition;
            spawnPosition.y = frogPosition.y;

            var enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            var enemyBehaviour = enemyObj.GetComponent<EnemyBehaviour>();
            if (enemyBehaviour != null)
            {
                enemyBehaviour.SetTarget(target);
            }

            _currentCount++;
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
    }
}
