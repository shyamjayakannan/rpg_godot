using Godot;

public class FootstepAudioPlayer : AudioStreamPlayer2D
{
    // private
    [Export]
    private AudioStream[] footstepVariations;

    // private
    private LevelTileMap tileMap;
    private AudioStreamRandomPitch audioStreamRandomPitch;

    // methods
    public override void _Ready()
    {
        audioStreamRandomPitch = (AudioStreamRandomPitch)Stream;
        GlobalLevelManager.Instance.Connect(nameof(GlobalLevelManager.LevelLoaded), this, nameof(OnLevelLoaded));
        OnLevelLoaded();
    }

    private void OnLevelLoaded()
    {
        for (Node p = GetParent(); p != null; p = p.GetParent())
        {
            if (p is LevelTileMap level)
            {
                tileMap = level;
                break;
            }
        }
    }

    // called in animationplayer function call track of player
    private void PlayFootsteps()
    {
        switch (tileMap.TileSet.TileGetName(tileMap.GetCellv(tileMap.ToLocal(GlobalPosition) / tileMap.CellQuadrantSize)))
        {
            case "grass.png":
                audioStreamRandomPitch.AudioStream = footstepVariations[0];
                break;

            case "pathway.png":
                audioStreamRandomPitch.AudioStream = footstepVariations[1];
                break;

            case "floor.png":
                audioStreamRandomPitch.AudioStream = footstepVariations[2];
                break;

            default:
                audioStreamRandomPitch.AudioStream = footstepVariations[1];
                break;
        }

        Play();
    }
}
