using QFramework;

namespace FrogRE
{
    public interface IFrogDataModel : IModel
    {
        BindableProperty<int> Hunger { get; }
        int MaxHunger { get; }
    }

    public class FrogDataModel : AbstractModel, IFrogDataModel
    {
        public int MaxHunger => 5;
        
        // 使用 BindableProperty 维护响应式数据，初始值为 1
        public BindableProperty<int> Hunger { get; } = new BindableProperty<int>(1);

        protected override void OnInit()
        {
            // 这里可以处理数据初始化，如读取存档
        }
    }
}
