# Animator Player

为 Unity Animator 提供状态播放、结束回调、顺序/随机/重复播放、运行时动画事件。
运行时命名空间为 `Yvsim.AnimatorPlayer`；随附原生 Animator 播放示例。

## 安装与依赖

当前源码版本为 **1.0.0**。Unity 最低版本为 **2021.3**。安装本地源码时：

1. 在 Package Manager 选择 **Add package from disk**，选择本包 `com.yvsim.animator-player/package.json`。
2. 等待 Unity 完成依赖解析和编译。也可将完整包目录放入工程的 `Packages/`，作为 Embedded Package。

核心包仅依赖 Unity Animation 模块，由 `package.json` 声明并由 UPM 解析；不依赖其他 Yvsim 包。
本地源码安装不要求 OpenUPM。仓库和注册表重新发布前，不应把 Git tag 或注册表中的 `1.0.0` 当作已可用版本。

## Animator 播放

在包含 Animator 的对象上添加 `AnimatorPlayer`，配置 Animator Controller 后调用。类型与命名空间同名时可使用别名：

```csharp
using AnimationPlayer = Yvsim.AnimatorPlayer.AnimatorPlayer;
using UnityEngine;

public sealed class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private AnimationPlayer player;

    public void PlayAction()
    {
        player.Play("Base Layer.Wave", () => Debug.Log("动作结束"));
    }
}
```

播放参数使用 Animator **状态名/完整路径**，`AddEvent` 等事件参数使用 **AnimationClip 名称**，两者不要混淆。

| 能力 | 入口 |
| --- | --- |
| 指定状态/起始进度 | `Play(stateName, ...)`、`Play(stateName, normalizedTime, ...)` |
| 随机一个 | `PlayRandom(stateNames, ...)`，返回输入列表中选中项的索引，无有效项时为 `-1` |
| 随机遍历列表 | `PlayRandomList(stateNames, playEnd, allFinish, layerIndex)` |
| 顺序播放 | `PlayList(startIndex, stateNames, playEnd, allFinish, layerIndex)`；另有指定首项进度的重载 |
| 重复次数 | `PlayWhile(stateName, whileCount, playEnd, playStartOne, playEndOnce, layerIndex)` |
| 固定帧 | `FixedToFirstFrame`、`FixedToNormalizedTime`、`FixedToLastFrame` |
| 速度/暂停 | `PlaySpeed`、`IsPause` |
| 当前状态 | `AnimatorComponent`、`GetCurrentState(layerIndex)`、`GetCurrentClip(layerIndex)` |

回调和序列依赖 Animator 的实际状态与归一化时间；请为业务动画配置正确的 Controller、Layer 和 State。
自动过渡、Blend Tree、循环与反向播放会影响完成时机，应使用实际控制器验收。组件停用会取消正在等待的播放流程。
需要改变行为时可继承并重写现有 `virtual` 方法。

## 动画事件

```csharp
player.AddEvent("Wave", "wave-sound", () => Debug.Log("事件触发"), 0.3f);
player.ManuallyCallEvent("Wave", "wave-sound");
player.RemoveEvent("Wave", "wave-sound");
player.ClearEvent("Wave");
```

事件键用于区分同一 Clip 的回调；`forceReplace: true` 可替换同键注册。
`ClearEvent()` 清理全部由本组件接口管理的事件。事件写入 Animator Controller 使用的 Clip，多个对象共享 Clip 时应检查事件共享带来的影响。

## 示例：Animator Playback

在 Package Manager 导入 **Animator Playback**，打开其中的 `Generated/Animator Playback.unity` 并进入 Play Mode。
选中 `Animator Playback Example`，通过组件右键菜单中的“示例”操作单个、顺序、随机、随机遍历与三次重复播放，以及暂停/恢复、1/2 倍速、首/中/末帧和动画事件。

示例包含原生 Animator Controller、Move/Turn/Scale 三段非循环动画、可见立方体、Prefab 和完整场景。运行时复制 Clip，避免事件影响共享动画资源；不使用其他业务包或插件，也不需要 UI 包。

可从 `Yvsim > 示例 > Animator Player > 创建播放示例` 在唯一目录重新生成素材，不覆盖现有资源或切换当前场景。操作步骤、预期结果与资源说明见 [示例 README](Samples~/Animator%20Playback/README.md)。

## 许可证

代码与本包生成的示例网格、材质和动画使用 MIT License。

## 从 GitHub 安装

仓库：[com.yvsim.animator-player](https://github.com/liuyan7669/com.yvsim.animator-player)。仓库名与 UPM 包名一致。

在 Unity Package Manager 选择 Add package from git URL，输入：

```text
https://github.com/liuyan7669/com.yvsim.animator-player.git#main
```

此地址指向 main 分支，当前包版本为 1.0.0。本次上传不创建版本 tag 或 GitHub Release，也不代表已在 OpenUPM 登记；Unity 官方依赖仍按 package.json 解析。
