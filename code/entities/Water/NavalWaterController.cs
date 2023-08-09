using Sandbox.Component;
using System;
namespace Sandbox;

public class NavalWaterController
{
	public Entity WaterEntity { get; set; }

	public float WaterThickness = 80.0f;

	public TimeSince TimeSinceLastEffect = 0;

	public void StartTouch( Entity other )
	{
		OnEnterWater( other as ModelEntity ); 
	}

	public void EndTouch( Entity other )
	{
		var c = other.Components.Get<NavalWaterEffectComponent>( true );
		if ( c.WaterEntity != WaterEntity )
			return;

		OnLeaveWater( other as ModelEntity );
	}

	public virtual void OnEnterWater( ModelEntity other )
	{
		if ( other == null ) return;

		var c = other.Components.GetOrCreate<NavalWaterEffectComponent>( true );
		c.WaterEntity = WaterEntity;
		c.Enabled = true;
	}

	public virtual void OnLeaveWater( ModelEntity other )
	{
		if ( other == null ) return;

		var c = other.Components.Get<NavalWaterEffectComponent>( true );
		if ( c == null || !c.Enabled ) return;

		c.WaterLevel = 0;
		c.Enabled = false;

		if ( other.PhysicsGroup == null ) return;

		var bodyCount = other.PhysicsGroup.BodyCount;
		for ( int i = 0; i < bodyCount; i++ )
		{
			var body = other.PhysicsGroup.GetBody( i );

			body.GravityScale = 1;
			body.LinearDrag = 0;
			body.AngularDrag = 0;
		}
	}


	public void Touch( Entity other )
	{
		if ( other == null ) return;

		var c = other.Components.Get<NavalWaterEffectComponent>( true );
		if ( c == null ) return;

		if ( c.WaterEntity != WaterEntity )
			return;
		
		if ( other is ModelEntity me && me.PhysicsGroup.IsValid() )
		{
			float totalLevel = 0;
			var bodyCount = me.PhysicsGroup.BodyCount;
			for ( int i = 0; i < bodyCount; i++ )
			{
				var body = me.PhysicsGroup.GetBody( i );
				totalLevel += UpdateBody( other, body );
			}

			if ( bodyCount > 0 )
				c.WaterLevel = (totalLevel / (float)bodyCount);
		}
	}

	private float UpdateBody( Entity ent, PhysicsBody body )
	{
		if ( ShouldBeAffectedByWater( ent, body ) == false )
			return 0;

		var waterDensity = 1000;

		var WaterEntityScale = (WaterEntity as ModelEntity).Scale;
		var density = body.Density;
		var waterSurface = WaterEntity.Position + (WaterEntity as ModelEntity).CollisionBounds.Maxs * WaterEntityScale;

		//DebugOverlay.Box( WaterEntity.Position, (WaterEntity as ModelEntity).CollisionBounds.Mins * WaterEntityScale, (WaterEntity as ModelEntity).CollisionBounds.Maxs * WaterEntityScale, Color.Red, 1, false );

		var bounds = body.GetBounds();
		var velocity = body.Velocity;
		var pos = bounds.Center;
		pos.z = waterSurface.z;

		var densityDiff = density - waterDensity;
		var volume = bounds.Volume;
		var level = waterSurface.z.LerpInverse( bounds.Mins.z, bounds.Maxs.z, true );

		if ( ent.IsAuthority )
		{
			var bouyancy = densityDiff.LerpInverse( 0.0f, -300f );
			bouyancy = MathF.Pow( bouyancy, 0.1f );

				DebugOverlay.Text( $"{bouyancy}", pos, Color.Red, 0.1f, 10000 );

			if ( bouyancy <= 0 )
			{
				body.GravityScale = 1.0f - level * 0.8f;
			}
			else
			{
				var point = bounds.Center;
				if ( level < 1.0f ) point.z = bounds.Mins.z - 100;
				var closestpoint = body.FindClosestPoint( point );

				float depth = (waterSurface.z - bounds.Maxs.z) / 100.0f;
				depth = depth.Clamp( 1.0f, 10.0f );
				DebugOverlay.Text( $"{depth}", point, Color.White, 0.1f, 10000 );
				DebugOverlay.Line( point, closestpoint, 1.0f );

				//body.ApplyImpulseAt( closestpoint, (Vector3.Up * volume * level * bouyancy * 0.0001f) );
				body.ApplyForceAt( closestpoint, (Vector3.Up * volume * level * bouyancy * 0.05f * depth) );

				//body.ApplyImpulseAt( )
				body.GravityScale = 1.0f - MathF.Pow( level.Clamp( 0, 0.5f ) * 2.0f, 0.5f );
			}

			body.LinearDrag = level * WaterThickness;
			body.AngularDrag = level * WaterThickness * 0.5f;
		}

		return level;

		/*
		if ( Game.IsClient )
		{
			if ( oldLevel == 0 )
				return;

			var change = MathF.Abs( oldLevel - level );

			//Log.Info( $"{change}" );

			if ( change > 0.001f && body.LastWaterEffect > 0.2f )
			{
				if ( oldLevel < 0.3f && level >= 0.35f )
				{
					var particle = Particles.Create( "particles/water_splash.vpcf", pos );
					particle.SetForward( 0, Vector3.Up );
					body.LastWaterEffect = 0;

					Sound.FromWorld( "water_splash_medium", pos );
				}

				if ( velocity.Length > 2f && velocity.z > -10 && velocity.z < 10 )
				{
					var particle = Particles.Create( "particles/water_bob.vpcf", pos );
					particle.SetForward( 0, Vector3.Up );
					body.LastWaterEffect = 0;
				}

			}
		}
		*/
	}
	public bool ShouldBeAffectedByWater( Entity ent, PhysicsBody body )
	{
		//DO NOT update anything that is static

		if ( ent.PhysicsGroup.IsValid() )
		{
			if ( body.MotionEnabled == false )
				return false;

			if ( body.BodyType != PhysicsBodyType.Static )
				return true;
		}
		return false;
	}
}
