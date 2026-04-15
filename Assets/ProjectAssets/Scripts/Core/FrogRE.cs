using QFramework;

namespace FrogRE
{
    public class FrogRE : Architecture<FrogRE>
    {
        protected override void Init()
        {
            // 注册 Model
            this.RegisterModel<IFrogDataModel>(new FrogDataModel());
            this.RegisterModel<IScoreModel>(new ScoreModel());
        }
    }
}
