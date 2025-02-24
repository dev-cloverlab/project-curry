using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Extensions
{
    public static class AnimatorExtensions
    {
        /// <summary>
        /// 指定のハッシュのアニメーションが再生されているか
        /// FIXME : 再生と同じフレームでは正常に検知できない
        /// </summary>
        public static bool IsCurrentAnimationHash(this Animator self, int hash)
        {
            return self.GetCurrentAnimatorStateInfo(0).shortNameHash == hash;
        }

        /// <summary>
        /// アニメーションが終了しているか
        /// FIXME : 再生と同じフレームでは正常に検知できない
        /// </summary>
        public static bool IsAnimationEnd(this Animator self, int hash)
        {
            // 別のアニメーションが再生されていたら終了したことにする
            if (!self.IsCurrentAnimationHash(hash))
            {
                return true;
            }

            var currentAnimatorState = self.GetCurrentAnimatorStateInfo(0);
            return currentAnimatorState.normalizedTime >= 1;
        }

        /// <summary>
        /// アニメーションを同期させた状態で再生させる
        /// </summary>
        public static void PlaySyncAnimation(this Animator self, string clipName)
        {
            var length = GetAnimationClipLength(self, clipName);
            self.Play(clipName, 0, Time.time / length);
        }

        /// <summary>
        /// アニメーションクリップの時間を取得
        /// </summary>
        private static float GetAnimationClipLength(this Animator self, string clipName)
        {
            var animationClips = self.runtimeAnimatorController.animationClips;

            return (from animationClip in animationClips
                where animationClip.name == clipName
                select animationClip.length).FirstOrDefault();
        }

        /// <summary>
        /// 再生中のアニメーションの長さ(秒) を取得
        /// </summary>
        public static float GetCurrentAnimationLength(this Animator self)
        {
            var currentAnimatorState = self.GetCurrentAnimatorStateInfo(0);
            return currentAnimatorState.length;
        }

        /// <summary>
        /// アニメーションを即再生する
        /// 同フレームで終了検知が可能
        /// </summary>
        public static void PlayImmediate(this Animator self, int hash, float normalizedTime=0.0f)
        {
            self.Play(hash,0,normalizedTime);
            self.Update(0);
        }
    }
}
