using Godot;

namespace Rpg
{
    public partial class EnemyState : Node
    {
        // Exports
        [Export]
        protected string NextStatePath { get; private set; }

        // methods
        public Enemy Enemy { get; set; }
        public EnemyStateMachine StateMachine { get; set; }

        // properties
        protected EnemyState NextState { get; private set; }

        // methods
        public override void _Ready()
        {
            NextState = GetNode<EnemyState>(NextStatePath);
        }

        public virtual void Init()
        {

        }

        public virtual void Enter()
        {

        }

        public virtual void Exit()
        {

        }

        public virtual EnemyState Process(double delta)
        {
            return null;
        }

        public virtual EnemyState PhysicsProcess(double delta)
        {
            return null;
        }
    }
}
