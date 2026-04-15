using QFramework;

namespace FrogRE
{
    /// <summary>
    /// 子弹击杀敌人：仅加分，不增加饱食度。
    /// </summary>
    public class EnemyShotCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var scoreModel = this.GetModel<IScoreModel>();
            scoreModel.Score.Value++;
        }
    }
}
