using Godot;

namespace Rpg
{
    public partial class FootstepAudioPlayer : AudioStreamPlayer2D
    {
        // Exports
        [Export]
        private AudioStream[] footstepVariations;

        // private
        Godot.Collections.Array<Node> array;

        // methods
        // called in animationplayer function call track of player
        public override void _Ready()
        {
            array = GetTree().GetNodesInGroup("GroundTileMapLayers");
            array.Reverse();
        }

        private void PlayFootsteps()
        {
            foreach (Node node in array)
            {
                if (node is not TileMapLayer tileMapLayer)
                    continue;

                TileData tileData = tileMapLayer.GetCellTileData(tileMapLayer.LocalToMap(tileMapLayer.ToLocal(GlobalPosition)));

                if (tileData == null)
                    continue;

                Stream = footstepVariations[(int)tileData.GetCustomData("footstepType")];
                break;
            }

            Play();
        }
    }
}
