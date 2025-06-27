using System.Collections.Generic;
using Godot;

public class PlayerStateMachine : Node
{
	// private
	private List<State> states;
	private State currentState;

	// properties
	public State NextState { get; private set; }

	// methods
	public override void _Ready()
	{
		PauseMode = PauseModeEnum.Stop;
	}

	public override void _Process(float delta)
	{
		ChangeState(currentState.Process(delta));
	}

	public override void _PhysicsProcess(float delta)
	{
		ChangeState(currentState.PhysicsProcess(delta));
	}

	// using event here because 'event' is a reserved keyword in C#
	public override void _UnhandledInput(InputEvent _event)
	{
		ChangeState(currentState.HandleInput(_event));
	}

	public void Initialize(Player player)
	{
		states = new List<State>();

		foreach (Node child in GetChildren())
			if (child is State state)
				states.Add(state);

		if (states.Count > 0)
		{
			State.StateMachine = this;
			State.Player = player;

			foreach (State state in states)
				state.Init();

			ChangeState(states[0]);
			PauseMode = PauseModeEnum.Inherit;
		}
	}

	public void ChangeState(State newState)
	{
		if (newState == null || newState == currentState)
			return;

		NextState = newState;
		currentState?.Exit();

		currentState = newState;
		currentState.Enter();
	}
}
