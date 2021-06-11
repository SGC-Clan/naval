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

		var StaminaTemp = 100;

		var HealthRemainder = 100 - player.Health.CeilToInt();
		var StaminaRemainder = 100 - StaminaTemp;
		Health.Text = "🩸 " + $"{player.Health.CeilToInt()}" + "%";
		Stamina.Text = "🏃 "+ $"{StaminaTemp}" + "%";
		Money.Text = "💰 " + "3371";
		Team.Text = "Royal Navy";

		
		Health.Style.Set( $"background: linear-gradient(to right, red " + player.Health.CeilToInt() + "%, 0%, rgba( #222, 0.3 )" + HealthRemainder + "%);" );
		Stamina.Style.Set( $"background: linear-gradient(to right, blue " + StaminaTemp + "%, 0%, rgba( #222, 0.3 )" + StaminaRemainder + "%);" );
		Team.Style.Set( $"color: blue;" );


	}
}
