using System.Collections.Generic;
using UnityEngine;
using Player = Yvsim.AnimatorPlayer.AnimatorPlayer;

namespace Yvsim.AnimatorPlayer.Samples
{
    /// <summary>直接使用本包 API 的播放示例；运行后通过组件上下文菜单操作。</summary>
    public sealed class AnimatorPlaybackExample : MonoBehaviour
    {
        [Tooltip("演示使用的 AnimatorPlayer，引用同物体上已配置 Controller 的组件。")]
        [SerializeField] private Player player;
        [Tooltip("最近一次操作、动画事件或完成回调的说明；运行时由示例更新。")]
        [SerializeField] private string status = "进入播放模式后，右键本组件标题选择“示例”菜单。";
        [Tooltip("本次播放操作已完成的动画数量；启动新的播放操作时归零。")]
        [SerializeField] private int completedCount;
        [Tooltip("本次播放操作触发的 Move 动画事件数量；启动新的播放操作时归零。")]
        [SerializeField] private int eventCount;

        private readonly List<string> states = new List<string> { "Base Layer.Move", "Base Layer.Turn", "Base Layer.Scale" };
        private readonly List<AnimationClip> ownedClips = new List<AnimationClip>();
        private Animator animator;
        private RuntimeAnimatorController originalController;
        private AnimatorOverrideController ownedController;

        /// <summary>当前演示使用的播放器。</summary>
        public Player Player => player;
        /// <summary>最近操作与回调说明。</summary>
        public string Status => status;
        /// <summary>本次操作已完成的动画数。</summary>
        public int CompletedCount => completedCount;
        /// <summary>本次操作收到的动画事件数。</summary>
        public int EventCount => eventCount;

        private void Awake()
        {
            animator = player != null ? player.GetComponent<Animator>() : null;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            originalController = animator.runtimeAnimatorController;
            ownedController = new AnimatorOverrideController(originalController);
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            ownedController.GetOverrides(overrides);
            for (int i = 0; i < overrides.Count; i++)
            {
                var clip = Instantiate(overrides[i].Key);
                clip.name = overrides[i].Key.name;
                ownedClips.Add(clip);
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, clip);
            }
            ownedController.ApplyOverrides(overrides);
            animator.runtimeAnimatorController = ownedController;
            RegisterEvent();
        }

        private void Start() => PlaySingle();

        private bool Prepare(string message, bool resetCounts = true)
        {
            if (!Application.isPlaying || player == null || ownedController == null)
            {
                Debug.LogWarning("请打开随附的 Animator Playback 场景并进入播放模式。", this);
                return false;
            }
            if (resetCounts) { completedCount = 0; eventCount = 0; }
            status = message;
            return true;
        }

        /// <summary>从头播放 Move，观察移动和自然结束回调。</summary>
        [ContextMenu("示例/播放单个 Move")]
        public void PlaySingle()
        {
            if (!Prepare("播放 Move")) return;
            player.Play(states[0], () => { completedCount++; status = "Move 播放完成"; });
        }

        /// <summary>依次播放 Move、Turn、Scale，并分别记录完成。</summary>
        [ContextMenu("示例/顺序播放全部")]
        public void PlaySequence()
        {
            if (!Prepare("顺序播放 Move → Turn → Scale")) return;
            player.PlayList(0, states, index => { completedCount++; status = "完成 " + states[index]; }, () => status = "全部顺序播放完成");
        }

        /// <summary>随机播放一个状态，并显示原列表索引。</summary>
        [ContextMenu("示例/随机播放一个")]
        public void PlayRandom()
        {
            if (!Prepare("随机播放")) return;
            int index = player.PlayRandom(states, () => { completedCount++; status = "随机动画播放完成"; });
            status = "随机选中 " + states[index];
        }

        /// <summary>随机且不重复地播放三个状态。</summary>
        [ContextMenu("示例/随机遍历全部")]
        public void PlayRandomSequence()
        {
            if (!Prepare("随机遍历全部动画")) return;
            player.PlayRandomList(states, state => { completedCount++; status = "完成 " + state; }, () => status = "全部随机播放完成");
        }

        /// <summary>重复播放 Move 三次，分别记录每次结束。</summary>
        [ContextMenu("示例/重复播放 Move 三次")]
        public void PlayRepeated()
        {
            if (!Prepare("重复播放 Move 三次")) return;
            player.PlayWhile(states[0], 3, () => status = "三次重复播放完成", null, remaining => { completedCount++; status = "本轮完成，剩余 " + (remaining - 1); });
        }

        /// <summary>切换 AnimatorPlayer 的暂停状态。</summary>
        [ContextMenu("示例/暂停或恢复")]
        public void TogglePause()
        {
            if (!Prepare("暂停/恢复", false)) return;
            player.IsPause = !player.IsPause;
            status = player.IsPause ? "已暂停" : "已恢复";
        }

        /// <summary>在正常速度与两倍速度之间切换。</summary>
        [ContextMenu("示例/正常或两倍速度")]
        public void ToggleSpeed()
        {
            if (!Prepare("切换速度", false)) return;
            player.PlaySpeed = player.PlaySpeed == 1f ? 2f : 1f;
            status = "播放速度 " + player.PlaySpeed;
        }

        /// <summary>调用本包接口固定在 Move 首帧。</summary>
        [ContextMenu("示例/固定 Move 首帧")]
        public void FirstFrame() { if (Prepare("固定首帧")) player.FixedToFirstFrame(states[0]); }
        /// <summary>调用本包接口固定在 Move 的 50% 进度。</summary>
        [ContextMenu("示例/固定 Move 中间帧")]
        public void MiddleFrame() { if (Prepare("固定 50% 进度")) player.FixedToNormalizedTime(states[0], 0.5f); }
        /// <summary>调用本包接口固定在 Move 末帧附近。</summary>
        [ContextMenu("示例/固定 Move 末帧")]
        public void LastFrame() { if (Prepare("固定末帧")) player.FixedToLastFrame(states[0]); }

        /// <summary>为实例独占的 Move Clip 注册或替换 0.5 秒事件。</summary>
        [ContextMenu("示例/注册或替换 Move 事件")]
        public void RegisterEvent()
        {
            if (!Prepare("已注册 Move 事件", false)) return;
            player.AddEvent("Move", "sample-midpoint", () => { eventCount++; status = "收到 Move 0.5 秒事件"; }, 0.5f, true);
        }
        /// <summary>移除本示例注册的动画事件。</summary>
        [ContextMenu("示例/移除 Move 事件")]
        public void RemoveEvent()
        {
            if (Prepare("已移除 Move 事件", false)) player.RemoveEvent("Move", "sample-midpoint");
        }
        /// <summary>手动触发本示例注册的动画事件。</summary>
        [ContextMenu("示例/手动触发 Move 事件")]
        public void InvokeEvent()
        {
            if (Prepare("手动触发事件", false)) player.ManuallyCallEvent("Move", "sample-midpoint");
        }

        private void OnDestroy()
        {
            if (ownedController == null) return;
            if (player != null) { player.StopAllCoroutines(); player.ClearEvent(); }
            if (animator != null) animator.runtimeAnimatorController = originalController;
            foreach (var clip in ownedClips) Destroy(clip);
            Destroy(ownedController);
        }
    }
}
