using System;
using System.Collections.Generic;

namespace Sandbox
{
	public class NavalWaterRipple : SceneCustomObject
	{
		internal NavalWater Water;
		internal ComputeShader compute;
		internal const int MaxConcurrentSplahes = 10;

		public class RippleCascade
		{
			public Texture Texture;
			internal Texture PrevTexture;

			/// <summary>
			/// How far is the simulation radius of the splash from camera view
			/// </summary>
			public float Radius;
			public RippleCascade( float radius = 512 )
			{
				Radius = radius;

				ImageFormat format = ImageFormat.RG1616F;
				Texture = Texture.Create( 256, 256 ).WithUAVBinding().WithFormat( format ).Finish();
				PrevTexture = Texture.Create( 256, 256 ).WithUAVBinding().WithFormat( format ).Finish();
			}
			public void Swap()
			{
				Texture temp = Texture;
				Texture = PrevTexture;
				PrevTexture = temp;
			}
		}
		public RippleCascade Cascade;

		/// <summary>
		/// How far is the simulation radius of the splash from camera view
		/// </summary>
		public float Radius = 512;

		public struct SplashInformation
		{
			public Vector3 Position;
			public float Radius;
			public float Strength;
			public Vector3 Velocity;
		};
		public struct RippleConstants
		{
			public Vector2 ViewPosition;
			public Vector2 ViewPositionLast;
			public float Radius;
			public float TimeDelta;
			public List<SplashInformation> Splashes;
		};

		public RippleConstants SplashConstants;
		/*public WaterRipple( Water parent )
		{
			this.Water = parent;
			compute = new ComputeShader( "ripplecompute_cs" );

			Cascade = new RippleCascade();

			SplashConstants = new RippleConstants();
			SplashConstants.Splashes = new List<SplashInformation>();
		}*/
		public NavalWaterRipple( SceneWorld world, NavalWater parent ) : base( world )
		{
			this.Water = parent;
			this.Transform = parent.Transform;
			this.Bounds = parent.CollisionBounds + parent.Position;
			
			compute = new ComputeShader( "ripplecompute_cs" );

			Cascade = new RippleCascade();

			SplashConstants = new RippleConstants();
			SplashConstants.Splashes = new List<SplashInformation>();
		}

		internal float GetSmallestRadiusFromVolume( Vector3 volume )
		{
			return Math.Min( volume.x, Math.Min( volume.y, volume.z ) ) * 0.5f;
		}

		/// <summary>
		/// Updates per frame data for the splash to render on the GPU next
		/// </summary>
		public void Update()
		{
			float Radius = Math.Abs( Cascade.Radius );

			float fHeight = Math.Max( Water.CollisionBounds.Mins.z + Water.Position.z, Water.CollisionBounds.Maxs.z + Water.Position.z );
			Vector3 vPos = new Vector3( Camera.Position.x + (Camera.Rotation.Forward.x * Radius * 0.85f), Camera.Position.y + (Camera.Rotation.Forward.y * Radius * 0.85f), fHeight );

			// Floor so that it snaps
			vPos = new Vector3( MathF.Floor( vPos.x / 128 ) * 128, MathF.Floor( vPos.y / 128 ) * 128, vPos.z );

			// Clamp to AABB so we can better optimize the used space
			/*if( Water.CollisionBounds.Size.x > Radius * 2 && Water.CollisionBounds.Size.y > Radius * 2 )
            {
                vPos.x = Math.Clamp( vPos.x, Water.CollisionBounds.Mins.x + Radius, Water.CollisionBounds.Maxs.x - Radius );
                vPos.y = Math.Clamp( vPos.y, Water.CollisionBounds.Mins.y + Radius, Water.CollisionBounds.Maxs.y - Radius );
            }*/

			// Encode the data
			SplashConstants.ViewPositionLast = SplashConstants.ViewPosition;
			SplashConstants.ViewPosition = vPos;
			SplashConstants.Radius = Radius;
			SplashConstants.TimeDelta = Math.Min( Time.Delta, 1 / 15f ); // Minimum of 15fps	

			BBox box = new BBox( vPos + new Vector3( -Radius, -Radius, 8 ), vPos + new Vector3( Radius, Radius, -8 ) );


			foreach ( Entity ent in Entity.FindInBox( box ) )
			{
				Vector3 debugPos = new Vector3( ent.Position.x, ent.Position.y, fHeight );
				Vector3 velocityDelta = new Vector3( ent.Velocity * Time.Delta );

				float radius = GetSmallestRadiusFromVolume( ent.WorldSpaceBounds.Size );

				if ( velocityDelta.Length < 0.1f )
					continue;

				// Add splash to list
				AddRipple(
					position: new Vector2( ent.Position.x, ent.Position.y ),
					velocity: velocityDelta,
					force: (velocityDelta * new Vector3( 0.003f, 0.003f, 0.015f )).Length,
					radius: radius );

			}

			// If debug view is on
			DebugView( box, fHeight );
		}

		public override void RenderSceneObject()
		{
			Update();
			OnRender();
		}

		internal void DebugView( BBox box, float height )
		{
			//Splash texture bounds
			DebugOverlay.Box( box.Mins, box.Maxs, Color.Cyan, 0, false );

			foreach ( var splash in SplashConstants.Splashes )
			{
				DebugOverlay.Box(
					new Vector3( splash.Position, height ) + new Vector3( splash.Radius, splash.Radius, splash.Strength ),
					new Vector3( splash.Position, height ) + new Vector3( -splash.Radius, -splash.Radius, -splash.Strength ),
					Color.Red,
					0,
					false
				);
			}
		}

		internal void SendAttributes()
		{
			if ( !Cascade.Texture.IsLoaded || !EngineConVars.r_water_ripples )
				return;

			//Send the constants to the compute shader
			compute.Attributes.Set( "ViewPosition", SplashConstants.ViewPosition );
			compute.Attributes.Set( "ViewPositionLast", SplashConstants.ViewPositionLast );
			compute.Attributes.Set( "Radius", SplashConstants.Radius * 2.0f );
			compute.Attributes.Set( "TimeDelta", SplashConstants.TimeDelta );
			compute.Attributes.Set( "Time", Time.Now );
			compute.Attributes.Set( "Splashes", SplashConstants.Splashes.Count );
			compute.Attributes.Set( "WaterHeight", Math.Max( Water.CollisionBounds.Mins.z, Water.CollisionBounds.Maxs.z ) + Water.Position.z );
			compute.Attributes.Set( "WaterVelocity", Water.Velocity );

			compute.Attributes.SetData( "SplashInformationBuffer_t", SplashConstants.Splashes );

			// Send the textures
			compute.Attributes.Set( "SplashTexture", Cascade.Texture );
			compute.Attributes.Set( "SplashTextureLast", Cascade.PrevTexture );
			compute.Attributes.Set( "SplashTextureSize", Cascade.Texture.Width );

			SplashConstants.Splashes.Clear();
		}

		/// <summary>
		/// Renders the splash texture on the compute shader on the GPU
		/// </summary>
		public void OnRender()
		{
			if ( !Cascade.Texture.IsLoaded )
				return;

			// Swap textures
			Cascade.Swap();

			// Update the compute shader attributes
			SendAttributes();

			// And send it for the GPU to do the work
			compute.Dispatch( Cascade.Texture.Width, Cascade.Texture.Height, 1 );

		}

		public void AddRipple( Vector2 position, Vector2 velocity, float force, float radius = 2.0f )
		{
			if ( SplashConstants.Splashes.Count >= MaxConcurrentSplahes ) return;

			SplashConstants.Splashes.Add( new SplashInformation()
			{
				Position = position,
				Radius = radius,
				Strength = force,
				Velocity = velocity
			} );
		}

	};
}
