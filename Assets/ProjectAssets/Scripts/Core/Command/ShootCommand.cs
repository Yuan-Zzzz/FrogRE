using QFramework;

namespace FrogRE
{
    /// <summary>
    /// 射击命令：每射出一发子弹，消耗1点饱食度。
    /// </summary>
    public class ShootCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var model = this.GetModel<IFrogDataModel>();
            
            // 只有在饱食度大于0时才允许扣除
            if (model.Hunger.Value > 0)
            {
                model.Hunger.Value--;
                // UnityEngine.Debug.Log($"[ShootCommand] 消耗1点饱食度，当前剩余: {model.Hunger.Value}");
            }
            else
            {
                // UnityEngine.Debug.LogWarning("[ShootCommand] 饱食度不足，无法射击！");
            }
        }
    }
}
