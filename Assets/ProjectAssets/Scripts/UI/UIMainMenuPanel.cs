using UnityEngine;
using UnityEngine.UI;
using QFramework;
using DG.Tweening;

namespace QFramework.Example
{
    public class UIMainMenuPanelData : UIPanelData
    {
    }
    public partial class UIMainMenuPanel : UIPanel
    {
        private Vector2 mTitleOriginalPos;

        protected override void OnInit(IUIData uiData = null)
        {
            mData = uiData as UIMainMenuPanelData ?? new UIMainMenuPanelData();
            // Record original position of Title to avoid drifting during loops
            mTitleOriginalPos = Title.rectTransform.anchoredPosition;
        }

        protected override void OnOpen(IUIData uiData = null)
        {
            // Kill existing tweens to prevent overlaps
            Showtime.rectTransform.DOKill();
            Title.rectTransform.DOKill();
            Btn_Exit.transform.DOKill();
            Btn_Start.GetComponent<RectTransform>().DOKill();

            // Showtime: 从上移动到默认位置
            Showtime.rectTransform.DOAnchorPosY(Showtime.rectTransform.anchoredPosition.y, 0.5f)
                .From(new Vector2(Showtime.rectTransform.anchoredPosition.x, 1000f))
                .SetEase(Ease.OutBack);

            // Title: 上下浮动 (使用原始位置确保不会漂移)
            Title.rectTransform.anchoredPosition = mTitleOriginalPos;
            Title.rectTransform.DOAnchorPosY(mTitleOriginalPos.y + 20f, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);

            // Btn_Exit: 从-180度旋转到0，缓冲刹车效果
            Btn_Exit.transform.DOLocalRotate(Vector3.zero, 0.6f)
                .From(new Vector3(0, 0, -180f))
                .SetEase(Ease.OutBack);

            // Btn_Start: 从左侧移动到中间位置
            Btn_Start.GetComponent<RectTransform>().DOAnchorPosX(Btn_Start.GetComponent<RectTransform>().anchoredPosition.x, 0.5f)
                .From(new Vector2(-3000f, Btn_Start.GetComponent<RectTransform>().anchoredPosition.y))
                .SetEase(Ease.OutBack);
        }

        protected override void OnShow()
        {
        }

        protected override void OnHide()
        {
        }

        protected override void OnClose()
        {
        }
    }
}
