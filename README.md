
# 🚀 团队开发工作流与 CI/CD 规范

为了保证代码质量和主分支（`master`）的稳定性，本项目采用了基于 **GitHub Flow** 的开发模式，并集成了 **Game-CI** 自动化构建与部署。

## 1. 核心规则
*   **禁止直接推送**：`master` 分支已开启分支保护，任何人（包括管理员）无法直接执行 `git push origin main`。
*   **PR 合并制**：所有代码变更必须通过 **Pull Request (PR)** 进行
*   **自动化门槛**：所有 PR 必须通过 GitHub Actions 的编译检查（Build Check），否则不允许合并。

## 2. 开发步骤

当你开始一个新任务（功能或修复 Bug）时，请遵循以下流程：

### 第一步：同步主分支
在开始前，确保你的本地 `main` 分支是最新的：
```bash
git checkout main
git pull origin main
```

### 第二步：创建特性分支
从 `master` 切出一个新分支。命名规范：`类型/功能描述`（例如 `feat/login-system` 或 `fix/collision-bug`）。
```bash
git checkout -b feat/your-feature-name
```

### 第三步：开发与提交
在你的分支上进行开发。建议多次提交小步快跑，并编写清晰的 Commit Message。
```bash
git add .
git commit -m "feat: 增加用户登录 UI 界面"
```

### 第四步：推送分支并开启 PR
将分支推送到远程仓库：
```bash
git push origin feat/your-feature-name
```
推送后，访问 GitHub 仓库页面，点击显眼的 **"Compare & pull request"** 按钮。

### 第五步：合并与清理
一旦 PR 获得批准且通过了所有检查，点击 **Merge pull request**。合并后，删除远程和本地的特性分支：
```bash
git branch -d feat/your-feature-name
git push origin --delete feat/your-feature-name
```

## 3. 自动化部署 (CI/CD)

本项目配置了强大的 GitHub Actions 工作流：

*   **构建检查**：每当你提交 PR 时，系统会自动尝试构建 **WebGL**, **Windows**, **Linux** 版本。如果代码导致 Unity 编译报错，PR 会显示红叉，禁止合并。
*   **自动部署**：当 PR 被合并到 `main` 分支后，系统会自动触发 WebGL 构建，并将其部署到 **GitHub Pages**。
*   **在线演示地址**：[https://yuan-zzzz.github.io/FrogRE/]

## 4. 分支命名规范

| 分支前缀 | 适用场景 |
| :--- | :--- |
| `feat/` | 新功能开发 |
| `fix/` | 修复已知 Bug |
| `refactor/` | 代码重构（无功能变化） |
| `docs/` | 仅文档修改 |
| `chore/` | 配置更新、依赖包更新等 |

---

## 💡 常见问题解答 (FAQ)

**Q: 为什么我的 PR 按钮是灰色的，不能点击合并？**
A: 请检查：1. 自动化构建是否还在运行或已报错；2. 是否已经有人 Approve 了你的代码；3. 是否有合并冲突。

**Q: 网页版本什么时候更新？**
A: 只要 `master` 分支有更新，Actions 就会启动部署任务。通常在合并后 3-5 分钟内，网页会自动同步。

---
