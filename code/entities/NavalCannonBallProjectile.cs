using Sandbox;
using System;

//[Spawnable]
[Library( "nvl_cannon_ball_projectile", Title = "Cannon Ball" )]
public partial class NavalCannonBallProjectile : Prop
{
	public Prop CannonParent = null; //a cannon this projectile originated from

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/props/props/cball.vmdl" );
		//SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Speed > 550f && eventData.Entity != CannonParent )
		{
			//simple explosion damage to hit entity
			if ( eventData.Entity.IsValid() )
			{
					var damage = DamageInfo.Explosion( eventData.Position, eventData.PreVelocity * 2, 100 )
					.WithAttacker( CannonParent.Owner )
					.WithWeapon( CannonParent );

				eventData.Entity.TakeDamage( damage );
			}

			ProjectileExplode();
		}
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		//Water level
	}

	[Net]
	public bool ShouldEmitTrailParticles { get; set; }

	[Event.Tick]
	public void OnThink() 
	{
		
		if ( IsServer )
		{
			//server toggles client side particle effects from
			ShouldEmitTrailParticles = PhysicsBody.Velocity.Length < 100f ? false : true;

			//water impact effects
			if ( PhysicsBody.WaterLevel > 0 )
			{
				ProjectileExplode( "water" );
			}
		}
		else if ( ShouldEmitTrailParticles ) // if client
		{
			Particles.Create( "particles/naval_projectile_small_smoke_trail.vpcf", this, null );
		}

	}

	public void ProjectileExplode( string type = "normal" ) 
	{

		switch ( type )
		{
			case "water":
				Sound.FromWorld( "nvl.water.splash", Transform.Position ); 
				Particles.Create( "particles/water_splash_medium.vpcf", Transform.PointToWorld( new Vector3( 0, 0, 10 ) ) );
				Particles.Create( "particles/water_splash_medium.vpcf", this, null );
				DamageExplosion( 120f, 50f, 5f );
				break;

			default:
				Sound.FromWorld( "nvl.cannonball.hitground", Transform.Position );
				Particles.Create( "particles/naval_cannonball_hitground.vpcf", this, null );
				DamageExplosion( 170f, 100f, 10f );
				break;
		}

		this.Delete();
	}

	public void DamageExplosion( float Radius = 150f, float Damage = 30f, float Force = 100f ) {

		var debug = false;

		if ( Radius > 0.0f )
		{
			var sourcePos = PhysicsBody.MassCenter;
			var overlaps = FindInSphere( sourcePos, Radius );

			if ( debug )
				DebugOverlay.Sphere( sourcePos, Radius, Color.Orange, 5 );

			foreach ( var overlap in overlaps )
			{
				if ( overlap is not ModelEntity ent || !ent.IsValid() )
					continue;

				if ( ent.LifeState != LifeState.Alive )
					continue;

				if ( !ent.PhysicsBody.IsValid() )
					continue;

				if ( ent.IsWorld )
					continue;

				var targetPos = ent.PhysicsBody.MassCenter;

				var dist = Vector3.DistanceBetween( sourcePos, targetPos );
				if ( dist > Radius )
					continue;

				var tr = Trace.Ray( sourcePos, targetPos )
					.Ignore( this )
					.WorldOnly()
					.Run();

				if ( tr.Fraction < 1.0f )
				{
					if ( debug )
						DebugOverlay.Line( sourcePos, tr.EndPosition, Color.Red, 5, true );

					continue;
				}

				if ( debug )
					DebugOverlay.Line( sourcePos, targetPos, 5, true );

				var distanceMul = 1.0f - Math.Clamp( dist / Radius, 0.0f, 1.0f );
				var damage = Damage * distanceMul;
				var force = (Force * distanceMul) * ent.PhysicsBody.Mass;
				var forceDir = (targetPos - sourcePos).Normal;

				ent.TakeDamage( DamageInfo.Explosion( sourcePos, forceDir * force, damage )
					.WithAttacker( this ) );
			}
		}
	}
}
