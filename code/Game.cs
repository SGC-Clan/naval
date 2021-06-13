using naval.teams;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;



namespace naval
{
	[Library( "naval", Title = "Naval" )]
	public partial class NavalGame : Game
	{
		public static NavalGame Instance => (Current as NavalGame);

		public RoyalNavyTeam RoyalNavy = new();
		public FrenchNavyTeam FrenchNavy = new();
		public MerchantsTeam Merchants = new();
		public PiratesTeam Pirates = new();

		public List<BaseTeam> Teams { get; set; } = new();

		public NavalGame()
		{
			if ( IsServer )
			{
				//Team Code Provided by sbox-hideandseek - Oppossome (In the Style of Stack Overflow <3)
				registerTeam( RoyalNavy );
				registerTeam( FrenchNavy );
				registerTeam( Merchants );
				registerTeam( Pirates );

				// Create the HUD
				new NavalHud();
			}
			else
			{
				return;
			}
		}

		public BaseTeam GetTeamById( int id )
		{
			if ( id == 0 ) return null;
			//return Teams[id - 1];
			return null;
		}

		private void registerTeam( BaseTeam team )
		{
			Teams.Add( team );
			team.TeamIndex = Teams.Count;
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );
			var player = new NavalPlayer();
			
			player.Respawn();

			cl.Pawn = player;
			player.Team = RoyalNavy;
		}

		public override void ClientDisconnect( Client client, NetworkDisconnectionReason reason )
		{
			var nvlPlayer = (client.Pawn as NavalPlayer);
			nvlPlayer.Team.OnPlayerExited( nvlPlayer );

			nvlPlayer.Team = RoyalNavy;
			
			base.ClientDisconnect( client, reason );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}

		[ServerCmd( "spawn" )]
		public static void Spawn( string modelname )
		{
			var owner = ConsoleSystem.Caller?.Pawn;

			if ( ConsoleSystem.Caller == null )
				return;

			var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 500 )
				.UseHitboxes()
				.Ignore( owner )
				.Size( 2 )
				.Run();

			var ent = new Prop();
			ent.Position = tr.EndPos;
			ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );
			ent.SetModel( modelname );

			// Drop to floor
			if ( ent.PhysicsBody != null && ent.PhysicsGroup.BodyCount == 1 )
			{
				var p = ent.PhysicsBody.FindClosestPoint( tr.EndPos );

				var delta = p - tr.EndPos;
				ent.PhysicsBody.Position -= delta;
				//DebugOverlay.Line( p, tr.EndPos, 10, false );
			}

		}

		[ServerCmd( "spawn_entity" )]
		public static void SpawnEntity( string entName )
		{
			var owner = ConsoleSystem.Caller.Pawn;

			if ( owner == null )
				return;

			var attribute = Library.GetAttribute( entName );

			if ( attribute == null || !attribute.Spawnable )
				return;

			var tr = Trace.Ray( owner.EyePos, owner.EyePos + owner.EyeRot.Forward * 200 )
				.UseHitboxes()
				.Ignore( owner )
				.Size( 2 )
				.Run();

			var ent = Library.Create<Entity>( entName );
			if ( ent is BaseCarriable && owner.Inventory != null )
			{
				if ( owner.Inventory.Add( ent, true ) )
					return;
			}

			ent.Position = tr.EndPos;
			ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRot.Angles().yaw, 0 ) );

			//Log.Info( $"ent: {ent}" );
		}
	}
}
