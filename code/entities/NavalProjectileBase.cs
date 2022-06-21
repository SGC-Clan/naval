using Sandbox;
using System;

//[Spawnable]
[Library( "nvl_projectile_base", Title = "Naval Projectile" )]
public partial class NavalProjectileBase : Prop
{
	public float ProjectileSize = 3f;
	public NavalTurretBase TurretParent = null; //a cannon this projectile originated from
	public Particles GlowEffect = null;
	public Sound ShellWhine;
	public Vector3 LastPosition;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/weapons/shell.vmdl" );
		//SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		//particle glow
		GlowEffect = Particles.Create( "particles/naval_projectile_glow.vpcf", this, null );
		GlowEffect.SetEntity(0, this, true);

		//shell whine
		//ShellWhine = Sound.FromWorld( "sounds/nvl.shellwhine.sound", Position );

		//projectile size
		Scale = ProjectileSize;
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Entity != TurretParent ) //eventData.Speed > 550f 
		{
			//simple explosion damage to hit entity
			if ( eventData.Entity.IsValid() )
			{
					var damage = DamageInfo.Explosion( eventData.Position, eventData.PreVelocity * 2, 100 )
					.WithAttacker( TurretParent.Owner )
					.WithWeapon( TurretParent );

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

			//when projectile is not moving at all we should consider it a dud
			if ( IsNotMoving() )
			{
				ProjectileExplode( "dud" );
			}
		}
		else if ( ShouldEmitTrailParticles ) // if client
		{
			Particles.Create( "particles/naval_projectile_small_smoke_trail.vpcf", this, null );
		}

	}

	public void ProjectileExplode( string type = "normal" ) 
	{
		var waterSurface = MoveToWaterSurface(Position);  

		switch ( type )
		{
			case "water": //when projectile hits water
				Sound.FromWorld( "nvl.explosion.water", Transform.Position ); 
				Particles.Create( "particles/water_splash_medium.vpcf", Transform.PointToWorld( new Vector3( 0, 0, 0 ) ) );
				DamageExplosion( 120f, 50f, 7f );
				//distant sound
				Sound.FromWorld( "nvl.distant.explosion.water", Transform.Position );
				break;

			case "dud": //when projectile is a dud (its velocity is suddenly at near 0)
				//Sound.FromWorld( "nvl.cannonball.hitground", Transform.Position );
				Particles.Create( "particles/impact.generic.vpcf", this, null );
				DamageExplosion( 50f, 30f, 5f );
				break;

			default: //when projectile explodes on impact
				Sound.FromWorld( "nvl.explosion.medium", Transform.Position );
				Particles.Create( "particles/naval_projectile_explosion_medium.vpcf", this, null );
				DamageExplosion( 170f, 100f, 10f );
				//distant sound
				Sound.FromWorld( "nvl.distant.explosion", Transform.Position );
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

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( GlowEffect != null )
			GlowEffect.Destroy( true );

		//ShellWhine.Stop();

	}

	public bool IsNotMoving() 
	{

		if ( Position == LastPosition )
			return true;

		LastPosition = Position;
		return false;
	}

	public Vector3 MoveToWaterSurface( Vector3 Point ) 
	{

		return Point;
	}

}
