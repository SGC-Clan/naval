using naval.Teams;
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sandbox
{
	[Library( "naval", Title = "Naval" )]
	public partial class NavalGame : Game
	{
		public static NavalGame Instance => (Current as NavalGame);

		public static List<BaseTeam> NavalTeams { get => teams; set => teams = value; }
		public RoyalNavyTeam RoyalNavy = new();
		public FrenchNavyTeam FrenchNavy = new();
		public MerchantsTeam Merchants = new();
		public PiratesTeam Pirates = new();
		private static List<BaseTeam> teams = new();

		public NavalGame()
		{

			void registerTeam( BaseTeam team )
			{
				NavalTeams.Add( team );
				Log.Info( "Team Registered:"+(team).ToString() );
				team.TeamIndex = NavalTeams.Count;
			}

			//Team Code Provided by sbox-hideandseek - Oppossome (In the Style of Stack Overflow <3)
			registerTeam( RoyalNavy );
			registerTeam( FrenchNavy );
			registerTeam( Merchants );
			registerTeam( Pirates );

			// Create the HUD
			new NavalHud();

		}

		public BaseTeam GetTeamById( int id )
		{

			if ( id == 0 ) { return null; }


			return NavalTeams[id - 1];

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

		[ConCmd.Server( "spawn" )]
		public static async Task Spawn( string modelname )
		{
			var owner = ConsoleSystem.Caller?.Pawn;

			if ( ConsoleSystem.Caller == null )
				return;

			var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500 )
				.UseHitboxes()
				.Ignore( owner )
				.Run();

			var modelRotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );

			//
			// Does this look like a package?
			//
			if ( modelname.Count( x => x == '.' ) == 1 && !modelname.EndsWith( ".vmdl", System.StringComparison.OrdinalIgnoreCase ) && !modelname.EndsWith( ".vmdl_c", System.StringComparison.OrdinalIgnoreCase ) )
			{
				modelname = await SpawnPackageModel( modelname, tr.EndPosition, modelRotation, owner );
				if ( modelname == null )
					return;
			}

			var model = Model.Load( modelname );
			if ( model == null || model.IsError )
				return;

			var ent = new Prop
			{
				Position = tr.EndPosition + Vector3.Down * model.PhysicsBounds.Mins.z,
				Rotation = modelRotation,
				Model = model
			};

			// Let's make sure physics are ready to go instead of waiting
			ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			// If there's no physics model, create a simple OBB
			if ( !ent.PhysicsBody.IsValid() )
			{
				ent.SetupPhysicsFromOBB( PhysicsMotionType.Dynamic, ent.CollisionBounds.Mins, ent.CollisionBounds.Maxs );
			}
		}

		static async Task<string> SpawnPackageModel( string packageName, Vector3 pos, Rotation rotation, Entity source )
		{
			var package = await Package.Fetch( packageName, false );
			if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
			{
				// spawn error particles
				return null;
			}

			if ( !source.IsValid ) return null; // source entity died or disconnected or something

			var model = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );
			var mins = package.GetMeta( "RenderMins", Vector3.Zero );
			var maxs = package.GetMeta( "RenderMaxs", Vector3.Zero );

			// downloads if not downloads, mounts if not mounted
			await package.MountAsync();

			return model;
		}

		[ConCmd.Server( "spawn_entity" )]
		public static void SpawnEntity( string entName )
		{
			var owner = ConsoleSystem.Caller.Pawn as Player;

			if ( owner == null )
				return;

			var entityType = TypeLibrary.GetTypeByName<Entity>( entName );
			if ( entityType == null )

				if ( !TypeLibrary.Has<SpawnableAttribute>( entityType ) )
					return;

			var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 200 )
				.UseHitboxes()
				.Ignore( owner )
				.Size( 2 )
				.Run();

			var ent = TypeLibrary.Create<Entity>( entityType );
			if ( ent is BaseCarriable && owner.Inventory != null )
			{
				if ( owner.Inventory.Add( ent, true ) )
					return;
			}

			ent.Position = tr.EndPosition;
			ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) );

			//Log.Info( $"ent: {ent}" );
		}

	}
}
