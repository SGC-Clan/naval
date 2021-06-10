using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class StatusBar : Panel
{
	public Label Health;
	public Label Stamina;
	public Label Money;
	public Label Team;

	public StatusBar()
	{
		//Add.Label( "🩸", "icon" );
		Health = Add.Label( "100", "health" );
		Stamina = Add.Label( "100", "stamina" );
		Money = Add.Label( "3371", "money" );
		Team = Add.Label( "Royal Navy", "team" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		Health.Text = "🩸 " + $"{player.Health.CeilToInt()}";
		Stamina.Text = "🏃 "+"100";
		Money.Text = "💰 " + "3371";
		Team.Text = "Royal Navy";
	}
}
