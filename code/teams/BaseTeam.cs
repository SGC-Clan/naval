using naval; 
using Sandbox;
using System;
using System.Collections.Generic;

namespace naval.teams
{
	public abstract partial class BaseTeam
	{

		public List<NavalPlayer> Players { get; } = new();
		public abstract string TeamName { get;  }
		public abstract Color TeamColor { get; }
		public int TeamIndex { get; set; }

		public virtual bool CanMove => true;

		public virtual void OnPlayerEntered(NavalPlayer player )
		{
			if ( !Players.Contains( player ) )
			{
				Log.Info( $"Player `{player.GetClientOwner().Name}` joined team `{TeamName}` IsServer: {Host.IsServer}" );
				Players.Add( player );
			}

		}

		public virtual void OnPlayerExited( NavalPlayer player )
		{
			if ( !Players.Contains( player ) )
			{
				Log.Info( $"Player `{player.GetClientOwner().Name}` left team `{TeamName}` IsServer: {Host.IsServer}" );
				Players.Remove( player );
			}
		}
	}
}
