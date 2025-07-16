using Godot;

namespace Rpg
{
    public partial class GlobalAudioManager : Node
    {
        // private
        private AudioStreamPlayer musicPlayer;
        private string musicBus = "Music";
        private float musicFadeDuration = 0.5f;
        private AudioStream stream;
        private Timer timer;

        // properties
        public static GlobalAudioManager Instance { get; private set; }

        // methods
        public override void _Ready()
        {
            Instance = this;

            // dont pause this if game pauses
            ProcessMode = ProcessModeEnum.Always;

            AudioStreamPlayer audioStreamPlayer = new()
            {
                Bus = musicBus,
                VolumeDb = -40
            };
            musicPlayer = audioStreamPlayer;
            AddChild(audioStreamPlayer);
            timer = new Timer
            {
                WaitTime = musicFadeDuration,
                OneShot = true
            };
            timer.Connect(Timer.SignalName.Timeout, new(this, MethodName.OnTimerTimeout));
            AddChild(timer);
        }

        private void OnTimerTimeout()
        {
            musicPlayer.Stop();
            FadeIn();
        }

        public void PlayAudio(AudioStream audioStream = null)
        {
            if (audioStream != stream)
            {
                FadeOut();
                timer.Start();
                stream = audioStream;
            }
        }

        private void FadeOut()
        {
            Tween tween = CreateTween();
            tween.TweenProperty(musicPlayer, "volume_db", -40, musicFadeDuration);
        }

        private void FadeIn()
        {
            if (stream == null)
                return;

            musicPlayer.Stream = stream;
            musicPlayer.Play();
            CreateTween().TweenProperty(musicPlayer, "volume_db", 0, musicFadeDuration);
        }
    }
}
