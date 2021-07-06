using Sandbox;

[Library( "nvl_cannon_ball_projectile", Title = "Cannon Ball", Spawnable = false )]
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
					var damage = DamageInfo.Explosion( eventData.Pos, eventData.PreVelocity * 2, 100 )
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
		//Client side particle effects
		if ( IsServer )
		{
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
				Particles.Create( "particles/water_splash.vpcf", this, null );
				break;
			default:
				Sound.FromWorld( "nvl.cannonball.hitground", Transform.Position );
				Particles.Create( "particles/naval_cannonball_hitground.vpcf", this, null );
				break;
		}

		this.Delete();
	}
}
