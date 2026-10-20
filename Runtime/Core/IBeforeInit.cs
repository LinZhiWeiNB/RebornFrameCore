namespace RebornFrame.Core
{
    public interface IBeforeInit
    {
        
        
        /// <summary>
        /// 在Awake之前，还是在Awake之后，Awake之前将在热更和配置加载之前
        /// </summary>
        public bool BeforeAwake { get; }
        
        /// <summary>
        /// 优先度，越大越优先，同级不保证优先度
        /// </summary>
        public int Priority { get; }
        
        /// <summary>
        /// 是否已经执行完毕
        /// </summary>
        public bool IsFinished { get; }
        
        /// <summary>
        /// 启动
        /// </summary>
        public void Start();
        
        /// <summary>
        /// 更新
        /// </summary>
        public void Update();
    }
}
