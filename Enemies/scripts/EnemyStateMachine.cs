using System.Collections.Generic;
using Godot;

public class EnemyStateMachine : Node
{
	// private
	private List<EnemyState> states;

	// public
	public EnemyState CurrentState { get; private set; }

	// methods
	public override void _Ready()
	{
		PauseMode = PauseModeEnum.Stop;
	}

	public override void _Process(float delta)
	{
		ChangeState(CurrentState.Process(delta));
	}

	public override void _PhysicsProcess(float delta)
	{
		ChangeState(CurrentState.PhysicsProcess(delta));
	}

	public void Initialize(Enemy enemy)
	{
		states = new List<EnemyState>();

		foreach (Node child in GetChildren())
		{
			if (child is EnemyState state)
			{
				state.Enemy = enemy;
				state.StateMachine = this;
				state.Init();
				states.Add(state);
			}
		}

		if (states.Count > 0)
		{
			ChangeState(states[0]);
			PauseMode = PauseModeEnum.Inherit; // Set pause mode to inherit so it pauses when the game is paused
		}
	}

	public void ChangeState(EnemyState newState)
	{
		if (newState == null || newState == CurrentState)
			return;

		CurrentState?.Exit();
		CurrentState = newState;
		CurrentState.Enter();
	}
}
