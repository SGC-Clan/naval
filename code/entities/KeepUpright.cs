using Sandbox;
using System.Linq;

[Library( "keep_upright", Title = "Keep Upright", Spawnable = true )]
public partial class KeepUpright : Prop
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

	public override void Simulate( Client owner )
	{
		if ( owner == null ) return;
		if ( !PhysicsBody.IsValid() )
		{
			return;
		}

		if ( !IsServer ) return;

		using ( Prediction.Off() )
		{

			this.WorldAng = new Angles( this.Rotation.Pitch(), this.Rotation.Yaw(), 0 );

		}

	}
}
