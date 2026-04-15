using QFramework;

namespace FrogRE
{
    /// <summary>
    /// 玩家吃到敌人后的统一业务命令：增加饱食度并增加分数。
    /// </summary>
    public class EnemyEatenCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            var frogDataModel = this.GetModel<IFrogDataModel>();
            var scoreModel = this.GetModel<IScoreModel>();

            if (frogDataModel.Hunger.Value < frogDataModel.MaxHunger)
            {
                frogDataModel.Hunger.Value++;
            }

            scoreModel.Score.Value++;
        }
    }
}
