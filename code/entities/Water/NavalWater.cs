using System;

namespace Sandbox
{
	[Library( "water" )]
	public partial class NavalWater : ModelEntity
	{
		/// <summary>
		/// Handles water level and buoyancy.
		/// </summary>
		public NavalWaterController WaterController = new NavalWaterController();

		/// <summary>
		/// Handles water ripples and rendering.
		/// </summary>
		public NavalWaterSceneObject WaterSceneObject;

		/// <summary>
		/// Material to use for water.
		/// </summary>
		[Property, ResourceType( "vmat" )]
		[Net] public string WaterMaterial { get; set; } = "materials/shadertest/test_water.vmat";

		/// <summary>
		/// Whether the water should have reflections.
		/// </summary>
		[Property]
		public bool EnableReflection { get; set; } = true;

		/// <summary>
		/// Whether the water surface should have ripples from objects moving about.
		/// </summary>
		[Property]
		public bool EnableRipples { get; set; } = true;

		/*[Property, Obsolete( "Does nothing" )]
		public bool EnableShadows { get; set; } = true;

		[Property, Obsolete( "Does nothing" )]
		public bool EnableFog { get; set; } = true;

		[Property, Obsolete( "Does nothing" )]
		public bool EnableRefraction { get; set; } = true;*/

		public NavalWater()
		{
			Tags.Add( "water" );
			WaterController.WaterEntity = this;

			EnableTouch = true;
			EnableTouchPersists = true;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			WaterSceneObject?.Delete();
			WaterController.WaterEntity = null;
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

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			CreateWaterSceneObject();
		}

		void CreateWaterSceneObject()
		{
			// Assume no scene object if we didn't pass a material
			if ( WaterMaterial is null || WaterMaterial.Length == 0 )
				return;

			Log.Info( "Scale:" );
			Log.Info(Scale);

			Vector3 NewMins = CollisionBounds.Mins * Scale;
			Vector3 NewMaxs = CollisionBounds.Maxs * Scale;
			BBox ScaledCollisionBounds = new BBox( NewMins, NewMaxs );

			WaterSceneObject = new NavalWaterSceneObject( Game.SceneWorld, this, Material.Load( WaterMaterial ), ScaledCollisionBounds + Position ) //this.CollisionBounds + this.Position
			{
				Transform = this.Transform,
				Position = this.Position,
			};
		}


		[GameEvent.Client.Frame]
		void Think()
		{
			UpdateSceneObject( WaterSceneObject );
		}

		/// <summary>
		/// Keep the scene object updated. By default this moves the transform to match this entity's transform
		/// and updates the bounds to the new position.
		/// </summary>
		public virtual void UpdateSceneObject( SceneObject obj )
		{
			if ( WaterSceneObject == null )
				return;
			WaterSceneObject.Update();
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( Game.IsClient )
				return;

			base.TakeDamage( info );

			if ( info.HasTag( "bullet" ) )
				AddRipple( info.Position, Vector3.Down, Sandbox.Game.Random.Float( info.Damage, info.Damage * 4 ) );
		}

		[ClientRpc]
		private void AddRipple( Vector3 position, Vector3 velocity, float strength )
		{
			WaterSceneObject?.AddRipple( position, velocity, strength );
		}
	}
}
