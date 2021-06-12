using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using naval.teams;
using naval;

public class StatusBar : Panel
{
	public Label Health;
	public Label Stamina;
	public Label Money;
	public Label Team;

	public StatusBar()
	{

		Health = Add.Label( "100", "health" );
		Stamina = Add.Label( "100", "stamina" );
		Money = Add.Label( "3371", "money" );
		Team = Add.Label( "Team Name", "team" );
	}

	public override void Tick()
	{
		var player = Local.Pawn;
		if ( player == null ) return;

		BaseTeam CurrentTeam = (Local.Pawn as NavalPlayer).Team;

		

		var StaminaTemp = 100;

		var HealthRemainder = 100 - player.Health.CeilToInt();
		var StaminaRemainder = 100 - StaminaTemp;
		Health.Text = "🩸 " + $"{player.Health.CeilToInt()}" + "%";
		Stamina.Text = "🏃 "+ $"{StaminaTemp}" + "%";
		Money.Text = "💰 " + "3371";
		Team.Text = CurrentTeam.TeamName;



		Health.Style.Set( $"background: linear-gradient(to right, red " + player.Health.CeilToInt() + "%, 0%, rgba( #222, 0.3 )" + HealthRemainder + "%);" );
		Stamina.Style.Set( $"background: linear-gradient(to right, blue " + StaminaTemp + "%, 0%, rgba( #222, 0.3 )" + StaminaRemainder + "%);" );
		Team.Style.Set( $"color: rgb({CurrentTeam.TeamColor.r},{CurrentTeam.TeamColor.g},{CurrentTeam.TeamColor.b});" );


	}
}
