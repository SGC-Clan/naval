using naval.teams;
using Sandbox;
using naval;

public partial class NavalPlayer : Player
	{
		[Net] public int TeamIndex { get; set; }
		private int lastTeamIndex { get; set; }

		public BaseTeam Team
		{
			get => NavalGame.Instance.GetTeamById( TeamIndex );
			set
			{
				BaseTeam lastTeam = NavalGame.Instance.GetTeamById( lastTeamIndex );
				if ( lastTeam == value ) return;

				TeamIndex = (value != null ? value.TeamIndex : 0);
				lastTeamIndex = TeamIndex;

				lastTeam?.OnPlayerExited( this );
				value?.OnPlayerEntered( this );
			}
		}

		public void TickTeamCheck()
		{
			if ( TeamIndex != lastTeamIndex )
				this.Team = NavalGame.Instance.GetTeamById( TeamIndex );
		}
	}
