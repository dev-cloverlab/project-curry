using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace curry.UI
{
    /// <summary>
    /// ScrollViewに配置した特定のオブジェクトの位置までスクロールを自動で移動するコンポーネント
    /// DOTweenが必要です
    /// GridLayoutGroupを使っているため、縦一列の場合もVerticalLayoutGroupではなくGridLayoutGroupを使ってください
    /// FIXME : 縦スクロールにしか対応していません
    /// </summary>
    public class ScrollViewAutoScroll : MonoBehaviour
    {
        [SerializeField]
        private RectTransform m_ViewportRectTransform;

        [SerializeField]
        private Transform m_ViewportTransform;

        [SerializeField]
        private RectTransform m_ContentRectTransform;

        [SerializeField]
        private GridLayoutGroup m_GridLayoutGroup;

        [SerializeField]
        private float m_TransitionDuration = 0.2f;

        private Transform dummy;

        private Sequence m_Sequence;

        private void OnDisable()
        {
            KillSequence();
        }

        private void KillSequence()
        {
            if (m_Sequence != null)
            {
                m_Sequence.Kill();
                m_Sequence = null;
            }
        }

        /// <summary>
        /// 自動スクロールを行う
        /// </summary>
        /// <param name="cellTarget">選択中のオブジェクトのTransform</param>
        /// <param name="tweenFlg">Tweenを行うか、デフォルト表示などでTweenせず一気に移動させたい場合はfalseを渡す</param>
        public void AutoScroll(Transform cellTarget, bool tweenFlg = true)
        {
            if (cellTarget == null)
            {
                Debug.LogAssertion("cellTarget is null");
                return;
            }

            // リストにオブジェクトを追加した状態を即レイアウトに反映する必要があるため明示的に更新を呼び出す
            // これをしない場合、リスト内のオブジェクトの個数の変化から1フレーム待つ必要がある
            // リストの要素数が変化しない場合最初だけ呼び出すだけでもいいかも
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_ContentRectTransform);

            // Tween中の場合は一度Killしておく
            KillSequence();

            const float halfAdjustValue = 2.0f;

            // セルサイズの半分の高さを取得
            var halfCellSizeY = m_GridLayoutGroup.cellSize.y / halfAdjustValue;
            // spacingの半分の高さを取得
            var halfSpacingY = m_GridLayoutGroup.spacing.y / halfAdjustValue;

            // Content内のCellのLocalPositionではなくViewport上でのCellのLocalPositionを取得したいため、ダミーオブジェクトを用意する
            // FIXME : 計算で出せそうならダミーを用意せずに行いたい
            if (dummy == null)
            {
                dummy = new GameObject("dummy").transform;
                dummy.SetParent(m_ViewportTransform);
            }

            // Content内の選択CellとPositionを合わせる
            dummy.position = cellTarget.position;
            // Viewportでの選択オブジェクトのLocalPositionを取得
            var cellPosition = dummy.localPosition;

            // Viewportでの選択オブジェクトのTopの位置とBottomの位置を取得、Spacingによる量も考慮
            var topCellPositionY = cellPosition.y + halfCellSizeY + halfSpacingY;
            var bottomCellPositionY = cellPosition.y - halfCellSizeY - halfSpacingY;

            // ViewportのTopの位置とBottomの位置を取得、Paddingによる量も考慮
            var viewportTop = m_ViewportRectTransform.rect.yMax - m_GridLayoutGroup.padding.top;
            var viewportBottom = m_ViewportRectTransform.rect.yMin + m_GridLayoutGroup.padding.bottom;

            // CellのBottomがViewportのBottomよりも下にある場合
            if (bottomCellPositionY < viewportBottom)
            {
                // どれだけはみ出ているかを取得
                var diff = Mathf.Abs(viewportBottom - bottomCellPositionY);
                // 移動先のpositionを取得
                var contentPos = m_ContentRectTransform.localPosition;
                contentPos.y += diff;

                if (tweenFlg)
                {
                    // はみ出ている分だけContentの位置を移動させる
                    m_Sequence = DOTween.Sequence();
                    m_Sequence.Append(m_ContentRectTransform.DOLocalMove(contentPos, m_TransitionDuration));
                }
                else
                {
                    m_ContentRectTransform.localPosition = contentPos;
                }
            }

            // CellのTopがViewportのTopよりも上にある場合
            if (topCellPositionY > viewportTop)
            {
                // どれだけはみ出ているかを取得
                var diff = Mathf.Abs(viewportTop - topCellPositionY);
                // 移動先のpositionを取得
                var contentPos = m_ContentRectTransform.localPosition;
                contentPos.y -= diff;

                if (tweenFlg)
                {
                    // はみ出ている分だけContentの位置を移動させる
                    m_Sequence = DOTween.Sequence();
                    m_Sequence.Append(m_ContentRectTransform.DOLocalMove(contentPos, m_TransitionDuration));
                }
                else
                {
                    m_ContentRectTransform.localPosition = contentPos;
                }
            }
        }
    }
}
