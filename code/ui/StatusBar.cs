using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using naval.Teams;


namespace naval.Teams
{

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
			Team = Add.Label( "TeamName", "team" );
		}

		public override void Tick()
		{
			var player = (Local.Pawn as NavalPlayer);
			if ( player == null ) return;





			//CurrentTeam.Lenght
			var StaminaTemp = 100;

			var HealthRemainder = 100 - player.Health.CeilToInt();
			var StaminaRemainder = 100 - StaminaTemp;
			Health.Text = "🩸 " + $"{player.Health.CeilToInt()}" + "%";
			Stamina.Text = "🏃 " + $"{StaminaTemp}" + "%";
			Money.Text = "💰 " + "3371";


			Health.Style.Set( $"background: linear-gradient(to left, red " + HealthRemainder + 3 + "%, rgba( #222, 0.3 )" + HealthRemainder + "%);" );
			Stamina.Style.Set( $"background: linear-gradient(to left, blue " + StaminaRemainder + 3 + "%, rgba( #222, 0.3 )" + StaminaRemainder + "%);" );


			Team.Text = player.Team.TeamName;
			Team.Style.Set( $"color: blue;" );


		}
	}


}
