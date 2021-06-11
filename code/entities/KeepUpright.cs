using Sandbox;
using System.Linq;

[Library( "keep_upright", Title = "Keep Upright", Spawnable = true )]
public partial class KeepUpright : Prop, IPhysicsUpdate
{
	bool enabled = false;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/arrow.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		enabled = true;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
		{
			PhysicsWorld.UseDefaultGravity();
			PhysicsWorld.WakeAllBodies();
		}

		enabled = false;
	}

	public void OnPostPhysicsStep( float dt )
	{
		if ( !PhysicsBody.IsValid() )
		{
			return;
		}
	


	}
}
