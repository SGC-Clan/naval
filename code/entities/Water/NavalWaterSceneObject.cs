using Sandbox.Internal;
using System;

namespace Sandbox
{
	public partial class NavalWaterSceneObject : SceneCustomObject
	{
		public BBox RenderBounds;
		public float WaterHeight;

		protected Material WaterMaterial;
		protected VertexBuffer vbWaterSurface;

		private NavalWater WaterParent;

		/// <summary>
		/// Controller used for planar reflections
		/// </summary>
		protected PlanarReflection WaterReflection;

		/// <summary>
		/// Controller used for water ripples using compute shaders
		/// </summary>
		protected RippleCompute WaterRipple;

		internal bool EnableRefraction;


		internal Vector3 _lastPosition;
		public Vector3 Velocity { get => _lastPosition - Position; }

		public NavalWaterSceneObject( SceneWorld world, NavalWater parent, Material material, BBox bounds ) : base( world )
		{
			WaterMaterial = material;
			WaterParent = parent;

			Flags.IsTranslucent = false;
			Flags.IsOpaque = true;

			Bounds = bounds;
			RenderBounds = bounds;

			CreateChildren();
		}

		internal void CreateChildren()
		{
			// Not until we unfuck the look of water

			/*if ( WaterParent.EnableReflection )
			{
				WaterReflection = new PlanarReflection( World, RenderBounds );
				AddChild( "WaterReflection", WaterReflection );
			}*/

			if ( WaterParent.EnableRipples )
				WaterRipple = new RippleCompute( this );

			EnableRefraction = WaterParent.EnableReflection;
		}

		/// <summary>
		///	Updates that are carried outside render thread
		///	</summary>
		public virtual void Update()
		{
			_lastPosition = Position;

			Transform = WaterParent.Transform;
			Position = WaterParent.Position;

			Vector3 NewMins = WaterParent.CollisionBounds.Mins * WaterParent.Scale;
			Vector3 NewMaxs = WaterParent.CollisionBounds.Maxs * WaterParent.Scale;
			BBox ScaledCollisionBounds = new BBox( NewMins, NewMaxs );

			Bounds = ScaledCollisionBounds + WaterParent.Position;        // Internal engine bounds
			RenderBounds = ScaledCollisionBounds + WaterParent.Position;  // Bounds accessible by C#


			if ( WaterReflection != null )
			{
				WaterReflection.Bounds = RenderBounds;
				WaterReflection?.Update( new Vector3( 0, 0, WaterHeight ), Rotation.Up, 2.0f );
			}

			WaterRipple?.Update();
		}

		/// <summary>
		///	Updates that are carried inside render thread
		///	</summary>
		public override void RenderSceneObject()
		{

			if ( Graphics.LayerType != SceneLayerType.Opaque )
				return;

			// Don't render if we are on the planar reflection's viewtarget itself
			if ( PlanarReflection.IsRenderingReflection() )
				return;


			//
			// Pass default parameters to the shader
			//
			Graphics.SetupLighting( this );
			
			Graphics.GrabDepthTexture( "DepthBufferCopyTexture" );
			Graphics.GrabFrameTexture( "FrameBufferCopyTexture" );

			//
			// Update the needed stuff for both reflection and splash
			//
			WaterReflection?.OnRender();
			WaterRipple?.OnRender();

			//
			// Check if camera is inside or touching water
			//
			bool bViewIntersectingWater = ViewIntersetingBBox( RenderBounds );

			// Move these away so we don't generate the vb every frame
			int[] res = ApproximatePlaneResolution( RenderBounds );
			vbUnderwaterStencil = MakeCuboid( RenderBounds.Mins, RenderBounds.Maxs );
			vbWaterSurface = MakeTesselatedPlane( RenderBounds.Mins, RenderBounds.Maxs, res[0], res[1] );
			
			//
			// Water constants
			//
			Graphics.Attributes.Set( "WaterHeight", WaterHeight );

			Graphics.Attributes.Set( "Reflection", WaterReflection is object );
			Graphics.Attributes.Set( "Refraction", true );
			Graphics.Attributes.Set( "Ripples", WaterRipple is object );


			if ( WaterRipple is not null )
			{
				//If splash is active, send constants
				Graphics.Attributes.Set( "SplashTexture", WaterRipple.Cascade.Texture );
				Graphics.Attributes.Set( "SplashRadius", WaterRipple.SplashConstants.Radius );
				Graphics.Attributes.Set( "SplashViewPosition", WaterRipple.SplashConstants.ViewPosition );
			}

			//
			// Draw our underwater fog stencil before the main pass
			// And also only if our vision is intersecting the water
			//
			if ( bViewIntersectingWater )
			{
				Graphics.Attributes.SetCombo( "D_UNDERWATER", true );
				vbUnderwaterStencil.Draw( WaterMaterial );
			}

			// If reflection is active, send reflection constants
			if ( WaterReflection is not null )
			{
				Graphics.Attributes.Set( "ReflectionTexture", WaterReflection.ColorTarget );
			}


			Graphics.Attributes.SetCombo( "D_UNDERWATER", false ); // Clear underwater flag when drawing main surface
			Graphics.Attributes.SetCombo( "D_VIEW_INTERSECTING_WATER", bViewIntersectingWater ); // Send flag to shader if we are intersecting our view with the water
			vbWaterSurface?.Draw( WaterMaterial );

			//DebugView();

			base.RenderSceneObject();
		}

		/// <summary>
		///	Checks if the view is intersecting the water bounding box
		/// Could probably be moved to a method inside BBox.cs to check if Vector3 is in bbox
		/// </summary>
		internal bool ViewIntersetingBBox( BBox bbox )
		{
			Vector3 viewPos = Camera.Position;
			float nearZ = 15;

			if ( viewPos.x < bbox.Mins.x - nearZ || viewPos.x > bbox.Maxs.x + nearZ
				|| viewPos.y < bbox.Mins.y - nearZ || viewPos.y > bbox.Maxs.y + nearZ
				|| viewPos.z < bbox.Mins.z - nearZ || viewPos.z > bbox.Maxs.z + nearZ )
				return false;

			return true;
		}

		internal void DebugView()
		{
			DebugOverlay.Box( RenderBounds.Mins, RenderBounds.Maxs, Color.Red, 0, false );
		}

		public int[] ApproximatePlaneResolution( BBox bounds )
		{
			Vector2 Bounds = new Vector2( bounds.Maxs.x - bounds.Mins.x, bounds.Maxs.y - bounds.Mins.y );

			//int[] res = new int[] { (int)(Bounds.x / 512), (int)(Bounds.y / 512) };
			int[] res = new int[] { (int)(Bounds.x / 16384), (int)(Bounds.y / 16384) };
			res = new int[] { Math.Clamp( res[0], 1, 512 ), Math.Clamp( res[1], 1, 512 ) };
			return res;
		}

		public VertexBuffer MakeTesselatedPlane( Vector3 from, Vector3 to, int xRes, int yRes )
		{
			var vb = new VertexBuffer();
			vb.Init( true );

			WaterHeight = Math.Max( from.z, to.z );

			xRes = Math.Clamp( xRes, 1, 256 );
			yRes = Math.Clamp( yRes, 1, 256 );

			for ( int x = 0; x <= xRes; x++ )
			{
				for ( int y = 0; y <= yRes; y++ )
				{
					float fX = MathX.LerpTo( from.x, to.x, x / (float)xRes );
					fX /= WaterParent.Scale; //Scale fix for naval
					float fY = MathX.LerpTo( from.y, to.y, y / (float)yRes );
					fY /= WaterParent.Scale; //Scale fix for naval

					Vector3 vPos = new Vector3(
						fX,
						fY,
						WaterHeight
					);

					vPos -= Transform.Position/ WaterParent.Scale; //Scale fix for naval

					var uv = new Vector2( x / (float)xRes, y / (float)yRes );

					vb.Add( new Vertex( vPos, Vector3.Down, Vector3.Right, uv ) );

				}
			}

			for ( int y = 0; y < yRes; y++ )
			{
				for ( int x = 0; x < xRes; x++ )
				{
					var i = y + (x * yRes) + x;

					vb.AddRawIndex( i + yRes + 1 );
					vb.AddRawIndex( i + 1 );
					vb.AddRawIndex( i );

					vb.AddRawIndex( i + 1 );
					vb.AddRawIndex( i + yRes + 1 );
					vb.AddRawIndex( i + yRes + 2 );

				}
			}
			return vb;
		}

		/// <summary>
		/// Creates a ripple to be simulated at the given position
		/// </summary>
		public void AddRipple( Vector2 position, Vector2 velocity, float force )
		{
			WaterRipple?.AddRipple( position, velocity, force );
		}

	}
}
