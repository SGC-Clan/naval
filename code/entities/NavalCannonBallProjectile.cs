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

	public void ProjectileExplode() 
	{
		Sound.FromWorld( "nvl.cannonball.hitground", Transform.Position );

		var tempparticle = Particles.Create( "particles/naval_cannonball_hitground.vpcf", this, "" );

		this.Delete();
	}
}
