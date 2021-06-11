using Sandbox;
using Sandbox.Tools;

[Library( "nvl_blackpowder_cannon", Title = "Blackpowder Cannon", Spawnable = true )]
public partial class BlackpowderCannonEntity : Prop, IUse
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/props/props/cannon_barrel.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		PlaySound( "flashlight-on" );

		ShootCannonball();

		return false;
	}

	public async void ShootCannonball() 
	{
		await GameTask.DelayRealtimeSeconds( 1.2f );

		Particles.Create( "particles/naval_gunpowder_smoke.vpcf", this, "muzzle" );

		PlaySound( "flashlight-off" );

	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}
}
