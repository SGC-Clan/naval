using Sandbox;
using Sandbox.Tools;

[Spawnable]
[Library( "nvl_cannon_platform", Title = "Cannon Platform" )]
public partial class CannonPlatformEntity : Prop, IUse
{
	public PhysicsJoint AttachJoint;

	public float AimPitch = 0;
	public float AimYaw = 0;

	public Entity ConnectedCannon = null;
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/props/de_inferno/cannon_base.vmdl" );
		//SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Entity.Name == "nvl_blackpowder_cannon" ) //eventData.Entity.ClassInfo.Name
		{
			var Cannon = eventData.Entity;
			//cordinates from sent_platform.lua
			Cannon.Position = Transform.PointToWorld( new Vector3( 0, -25.2f, 41.3f ) );
			Cannon.Rotation = Transform.Rotation;

			// !! this acts as nocolide! what the hell!
			//AttachJoint = PhysicsJoint.Generic
			//		.From( (ModelEntity)Cannon, 0, null, null )
			//		.To( this, 0, null, null )
			//		//.WithBasis( Rotation globalBasis )
			//		.WithBlockSolverEnabled()
			//		.WithFriction( 0.02f )
			//		//.WithPivot( Vector3 globalPivot )
			//		.Create();

			this.ConnectedCannon = Cannon; // !!! this does nothing for some reason :(

			Weld( (Prop)Cannon );
		}
		
	}

	[Event.Frame]
	public void OnFrame()
	{
		AimPitch = 25f;
		AimYaw = 25f;

		//DebugOverlay.Text( Position + Transform.NormalToWorld( new Vector3( 0, 0, 100 ) ), "debug:"+ConnectedCannon.ToString() );

		if ( ConnectedCannon.IsValid() ) 
		{
			DebugOverlay.Line( Position, Transform.PointToWorld( new Vector3( 0, -25.2f, 41.3f ) ) );
			Transform NewTransform = new Transform( Transform.PointToWorld( new Vector3( 0, -25.2f, 41.3f ) ), Transform.RotationToWorld( Rotation.From( 0, AimYaw, AimPitch ) ) );
			ConnectedCannon.Transform = NewTransform;
		}
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
		//PhysicsGroup?.Wake();
		Delete();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( AttachJoint.IsValid() )
		{
			AttachJoint.Remove();
		}
	}
	public void OnPostPhysicsStep( float dt )
	{
		if ( !this.IsValid() )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;
	}
}
