# Animator Playback

本示例直接使用 Animator Player 的公开 API，只依赖本包及 Unity Animation 模块。提供原生 Animator Controller、Move/Turn/Scale 三个非循环 Clip、立方体网格与材质、Prefab 和可直接运行的场景。

## 使用步骤

1. 在 Package Manager 导入 Animator Playback。
2. 打开导入目录 `Generated/Animator Playback.unity` 并进入 Play Mode。
3. 选中 `Animator Playback Example`，观察立方体自动播放 Move，以及组件的 Status、Completed Count、Event Count。
4. 右键 `Animator Playback Example` 组件标题（或打开组件右上角菜单），展开“示例”，选择单个、顺序、随机、随机遍历、重复三次、暂停/恢复、调速、首/中/末帧及事件操作。

三个状态完整路径为 `Base Layer.Move`、`Base Layer.Turn`、`Base Layer.Scale`，Clip 名称分别为 `Move`、`Turn`、`Scale`。播放入口使用状态路径，事件入口使用 Clip 名称。

## 观察结果

- 顺序播放：移动、旋转、缩放依次完成，Completed Count 最终为 3。
- 随机遍历：三个状态各播放一次，顺序可变，最终完成数量仍为 3。
- 重复：Move 播放三次，每轮都产生完成回调。
- 暂停和调速：播放过程中切换暂停状态或 1/2 倍速；重新播放会退出定帧/暂停模式。
- 定帧：调用本包首帧、50% 进度、末帧接口，观察 Visual 的位置。
- 动画事件：默认在 Move 的 0.5 秒注册事件，Event Count 自动增加；可移除、重新注册或手动触发。

每个示例实例在运行时复制自己的 Clip 并通过 AnimatorOverrideController 使用，动画事件不会写回共享源 Clip；销毁实例时清理复制资源并恢复原 Controller。

## 重新生成

可执行 `Yvsim/示例/Animator Player/创建播放示例`，在新的唯一 `Assets/YvsimAnimatorPlayback*` 目录重新生成资源。生成器使用 Unity 资源和场景 API，不切换当前场景，不覆盖现有资源，不修改 Build Settings。

示例的操作入口位于 Editor 组件上下文菜单，没有运行时 GUI 或 UI 包依赖。生成材质面向 Built-in 渲染管线，其他管线可替换材质。核心组件行为由实际 Animator Controller 决定；业务控制器的循环、过渡和 Blend Tree 需按项目验证。
