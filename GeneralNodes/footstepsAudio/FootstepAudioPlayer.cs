using Godot;

namespace Rpg
{
    public partial class FootstepAudioPlayer : AudioStreamPlayer2D
    {
        // private
        [Export]
        private AudioStream[] footstepVariations;

        // private
        // private LevelTileMap tileMap;
        private AudioStreamPlayer2D audioStreamPlayer2D;

        // methods
        public override void _Ready()
        {
            // audioStreamRandomPitch = (AudioStreamRandomizer)Stream;
            GlobalLevelManager.Instance.Connect(GlobalLevelManager.SignalName.LevelLoaded, new(this, MethodName.OnLevelLoaded));
            OnLevelLoaded();
        }

        private void OnLevelLoaded()
        {
            // for (Node p = GetParent(); p != null; p = p.GetParent())
            // {
            //     if (p is LevelTileMap level)
            //     {
            //         tileMap = level;
            //         break;
            //     }
            // }
        }

        // called in animationplayer function call track of player
        private void PlayFootsteps()
        {
            // audioStreamRandomPitch.ad = (object)tileMap.TileSet.TileGetName(tileMap.GetCellAtlasCoords(tileMap.ToLocal(GlobalPosition) / tileMap.TileSet.TileSize)) switch
            // {
            //     "grass.png" => footstepVariations[0],
            //     "pathway.png" => footstepVariations[1],
            //     "floor.png" => footstepVariations[2],
            //     _ => footstepVariations[1],
            // };
            Play();
        }
    }
}
