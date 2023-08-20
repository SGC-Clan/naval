using System;

namespace Sandbox
{
	[Library( "water" )]
	public partial class NavalWater : ModelEntity
	{
		private NavalWaterController WaterController = new NavalWaterController();
		private NavalWaterRipple WaterRipple;

		[Net] public string WaterMaterial { get; set; } = "materials/water/naval_water_ocean01.vmat";

		[Net] public float WaterBoundsSize { get; set; } = 100;
		[Net] public Vector3 WaterBoundsCenter { get; set; } = new Vector3();

		public NavalWater()
		{
			Tags.Add( "water" );
			WaterController.WaterEntity = this;

			EnableTouch = true;
			EnableTouchPersists = true;

			CreateWaterModel();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			CreateWaterModel();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			WaterController.WaterEntity = null;

			WaterRipple?.Delete();
			WaterRipple = null;
		}

		public override void Touch( Entity other )
		{
			base.Touch( other );

			WaterController.Touch( other );
		}

		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			WaterController.EndTouch( other );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			WaterController.StartTouch( other );
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );
		}


		[GameEvent.Client.Frame]
		void OnFrame()
		{
			//WaterRipple.Update();
			//WaterRipple.OnRender();

			if ( !WaterRipple.IsValid() )
				return;

			if ( SceneObject == null )
				Log.Info( SceneObject );

			if ( SceneObject == null )
				return;

			//material MUST be set to water.shader or this will explode
			SceneObject.Attributes.Set( "RippleTexture", WaterRipple.Cascade.Texture );
			SceneObject.Attributes.Set( "SplashRadius", WaterRipple.SplashConstants.Radius );
			SceneObject.Attributes.Set( "SplashViewPosition", WaterRipple.SplashConstants.ViewPosition );

			SceneObject.Attributes.SetCombo( "D_RIPPLES", true );
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( Game.IsClient )
				return;

			base.TakeDamage( info );

			if ( info.HasTag( "bullet" ) )
				AddRipple( info.Position, Vector3.Down, Game.Random.Float( info.Damage, info.Damage * 4 ) );
		}

		[ClientRpc]
		private void AddRipple( Vector3 position, Vector3 velocity, float strength )
		{
			WaterRipple?.AddRipple( position, velocity, strength );
		}

		public async void CreateWaterModel() 
		{
			//pls find a better way to do this pls
			await GameTask.Delay( 50 );

			var mesh = new Mesh
			{
				Material = Material.Load( WaterMaterial ),
				PrimitiveType = MeshPrimitiveType.Triangles,
			};
			var meshVB = new VertexBuffer();
			meshVB.AddCube( WaterBoundsCenter, WaterBoundsSize, new Rotation(), default );
			mesh.CreateBuffers( meshVB, true );
			var meshModelBuilder = new ModelBuilder();
			meshModelBuilder.AddMesh( mesh );
			meshModelBuilder.AddCollisionBox( new Vector3( WaterBoundsSize/2, WaterBoundsSize / 2, WaterBoundsSize / 2 ), WaterBoundsCenter );
			Model =  meshModelBuilder.Create();
			SetupPhysicsFromModel( PhysicsMotionType.Static );
			Tags.Add( "water" );


			WaterRipple = new NavalWaterRipple( Game.SceneWorld, this );
		}
	}
}
