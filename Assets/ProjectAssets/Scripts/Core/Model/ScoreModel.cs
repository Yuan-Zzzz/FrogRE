using QFramework;

namespace FrogRE
{
    public interface IScoreModel : IModel
    {
        BindableProperty<int> Score { get; }
    }

    public class ScoreModel : AbstractModel, IScoreModel
    {
        public BindableProperty<int> Score { get; } = new BindableProperty<int>(0);

        protected override void OnInit()
        {
        }
    }
}
