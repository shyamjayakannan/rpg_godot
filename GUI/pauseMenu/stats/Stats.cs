using Godot;

public class Stats : PanelContainer
{
    // private
    private Label level;
    private Label xp;
    private Label attack;
    private Label attackPlus;
    private Label defence;
    private Label defencePlus;

    // methods
    public override void _Ready()
    {
        level = GetNode<Label>("VBoxContainer/HBoxContainer/Level");
        xp = GetNode<Label>("VBoxContainer/HBoxContainer2/Xp");
        attack = GetNode<Label>("VBoxContainer/HBoxContainer3/Attack");
        attackPlus = GetNode<Label>("VBoxContainer/HBoxContainer3/Attack2");
        defence = GetNode<Label>("VBoxContainer/HBoxContainer4/Defence");
        defencePlus = GetNode<Label>("VBoxContainer/HBoxContainer4/Defence2");
    }

    public void ModifyStats(int attackModifier, int defenceModifier)
    {
        Display();

        if (attackModifier >= 0)
        {
            attackPlus.Text = $"+{attackModifier}";
            attackPlus.Modulate = Colors.LightGreen;
        }
        else
        {
            attackPlus.Text = attackModifier.ToString();
            attackPlus.Modulate = Colors.Red;
        }

        if (defenceModifier >= 0)
        {
            defencePlus.Text = $"+{defenceModifier}";
            defencePlus.Modulate = Colors.LightGreen;
        }
        else
        {
            defencePlus.Text = defenceModifier.ToString();
            defencePlus.Modulate = Colors.Red;
        }
    }

    public void UpdateStats(int attackModifier, int defenceModifier)
    {
        GlobalPlayerManager.Instance.Player.Attack += attackModifier;
        GlobalPlayerManager.Instance.Player.Defence += defenceModifier;
        Display();
        attackPlus.Text = "";
        defencePlus.Text = "";
    }

    public void Display()
    {
        level.Text = GlobalPlayerManager.Instance.Player.Level.ToString();
        xp.Text = $"{GlobalPlayerManager.Instance.Player.Xp}/{GlobalPlayerManager.Instance.Player.LevelUpXpRequirements[GlobalPlayerManager.Instance.Player.Level]}";
        attack.Text = $"{GlobalPlayerManager.Instance.Player.Attack}";
        defence.Text = $"{GlobalPlayerManager.Instance.Player.Defence}";
    }
}
