namespace UITweens.Services
{
    /// <summary>
    /// Wraps a TweenBuilder to implement ITweenDeclaration.
    /// Single Responsibility: Adapter for declarative tween API only.
    /// </summary>
    public class DeclarationWrapper : ITweenDeclaration
    {
        private readonly TweenBuilder builder;

        public DeclarationWrapper(TweenBuilder builder)
        {
            this.builder = builder;
        }

        public TweenBuilder Spawn(TweenSpawner spawner)
        {
            return builder;
        }
    }
}

