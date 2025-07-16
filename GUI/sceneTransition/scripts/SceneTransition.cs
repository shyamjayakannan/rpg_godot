using Godot;

namespace Rpg
{
	public partial class SceneTransition : CanvasLayer
	{
		// private
		private AnimationPlayer animationPlayer;

		// properties
		public static SceneTransition Instance { get; private set; }

		// methods
		public override void _Ready()
		{
			Instance = this;
			animationPlayer = GetNode<AnimationPlayer>("Control/AnimationPlayer");
		}

		public float FadeIn()
		{
			animationPlayer.Play("fadeIn");
			return (float)animationPlayer.CurrentAnimationLength;
		}

		public float FadeOut()
		{
			animationPlayer.Play("fadeOut");
			return (float)animationPlayer.CurrentAnimationLength;
		}
	}
}
