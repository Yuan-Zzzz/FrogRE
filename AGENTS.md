# AGENTS.md - 智能体开发指南

本文档为在此工作的智能体提供开发规范和操作指南。

---

## 1. 项目概况

| 项目属性 | 值 |
|---|---|
| 引擎版本 | Unity 2022.3.62f2c1 |
| 项目类型 | Unity 游戏项目 (C#) |
| 主框架 | QFramework (UIKit, ResKit, AudioKit) |
| 源代码命名空间 | `QFramework.Example` |

### 目录结构

```
Assets/
├── ProjectAssets/Scripts/    # 游戏业务代码
├── QFramework/               # 框架代码 (勿轻易修改)
├── BOXOPHOBIC/               # 第三方工具 (Utils, Skybox)
├── TextMesh Pro/             # TMP 示例代码
└── Scenes/                  # 场景文件
```

---

## 2. 构建与测试命令

### 2.1 本地构建 (通过 Unity Editor)

Unity 项目无传统意义上的"命令行测试"，所有验证通过 Editor 完成：

```bash
python -c "
from unity_skills import call_skill
call_skill('scene_build', targetPlatform='WebGL')
call_skill('console_clear')
"
```

### 2.2 CI/CD 自动构建 (GitHub Actions)

当代码合并至 `master` 分支时自动触发构建：

```yaml
matrix:
  - WebGL
  - StandaloneWindows64
  - StandaloneLinux64
```

**构建结果**: PR 提交时自动构建，失败则阻止合并。合并后自动部署至 GitHub Pages。

---

## 3. 开发规范

### 3.1 分支管理 (GitHub Flow)

| 分支前缀 | 用途 | 示例 |
|---|---|---|
| `master` | 主分支 (受保护) | - |
| `feat/` | 新功能 | `feat/login-system` |
| `fix/` | Bug 修复 | `fix/collision-bug` |
| `refactor/` | 重构 | `refactor/audio-manager` |

**开发流程**: checkout → commit → push → PR → merge → delete

### 3.2 代码风格

#### 命名约定

- **类/接口**: PascalCase (`FrogController`, `IUIPanel`)
- **方法**: PascalCase (`Update`, `HandleJumpInput`)
- **私有字段**: camelCase 加下划线前缀 (`_rb`, `_targetScale`)
- **SerializeField**: PascalCase (`JumpForce`, `ViewTransform`)
- **常量**: 全大写加下划线 (`MAX_JUMP_FORCE`)

#### Unity 特有规范

```csharp
// ✅ 推荐: 使用 SerializeField 暴露私有变量
[Header("Jump Settings")]
[SerializeField] private float jumpForce = 10f;

// ✅ 推荐: Unity 生命周期方法使用单行空方法体
void Start() { }
void Update() { HandleInput(); }

// ❌ 避免: 公有字段直接暴露
public float Speed;
public float Speed { get; private set; }  // 推荐
```

#### 导入规范

```csharp
// 导入顺序: Unity → System → 项目内部 → 第三方
using UnityEngine;
using System.Collections.Generic;
using QFramework;
using TMPro;
```

### 3.3 错误处理

```csharp
// ✅ 推荐: Try-Catch 与日志
try {
    LoadScene("Game");
} catch (System.Exception e) {
    Debug.LogError($"加载场景失败: {e.Message}");
}

// ❌ 避免: 空 Catch 块
catch { }
```

### 3.4 注释规范

- 公开 API 必须文档注释
- TODO 使用统一格式: `// TODO: [描述]`

```csharp
/// <summary>
/// 执行青蛙跳跃
/// </summary>
public void Jump(Vector3 direction) { }
```

---

## 4. Unity 操作指南

### 4.1 使用 unity-skills 遥控 Editor

项目已配置 unity-skills，可通过 REST API 操作 Unity Editor：

```python
from unity_skills import is_unity_running, call_skill

if is_unity_running():
    call_skill('gameobject_create', name='Player', primitiveType='Capsule')
    call_skill('console_get_logs', type='Error')
```

**前提条件**: Unity Editor 必须运行且启动 UnitySkills 服务器

### 4.2 脚本创建后处理

```python
result = call_skill('script_create', name='MyScript', template='MonoBehaviour')
if result.get('success'):
    wait_for_unity(timeout=10)  # 等待编译完成
```

---

## 5. 部署与发布

| 触发条件 | 动作 |
|---|---|
| PR 合并至 master | 自动构建并部署至 GitHub Pages |
| 推送 Tag | 创建 GitHub Release |

**在线演示地址**: https://yuan-zzzz.github.io/FrogRE/

---

## 6. 常见问题

**Q: PR 合并按钮灰色不可点击?**
A: 检查: 1) 构建是否通过; 2) 是否已获 Approve; 3) 是否有冲突.

**Q: 如何运行单个测试?**
A: Unity 项目无单元测试框架，使用 Play Mode 或手动验证场景功能.

---

## 7. 敏感操作提醒

- **禁止直接提交至 master**: 所有变更通过 PR
- **框架代码谨慎修改**: QFramework 位于 Assets/QFramework/
- **资源文件使用 LFS**: 大型二进制文件已配置 Git LFS

---

*本文件由 Agent 生成，最后更新: 2024*