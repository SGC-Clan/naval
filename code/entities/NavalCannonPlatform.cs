using Sandbox;
using Sandbox.Tools;


[Library( "nvl_cannon_platform", Title = "Cannon Platform", Spawnable = true )]
public partial class CannonPlatformEntity : Prop, IUse
{
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/props/de_inferno/cannon_base.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return false;
	}

	public bool OnUse( Entity user )
	{
		return false;
	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}
}
