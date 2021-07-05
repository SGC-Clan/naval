using Sandbox;
using Sandbox.Tools;


[Library( "nvl_cannon_platform", Title = "Cannon Platform", Spawnable = true )]
public partial class CannonPlatformEntity : Prop, IUse
{
	public PhysicsJoint AttachJoint;

	public Entity ConnectedCannon = null;
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/props/de_inferno/cannon_base.vmdl" );
		//SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Entity.ClassInfo.Name == "nvl_blackpowder_cannon" )
		{
			var Cannon = eventData.Entity;
			//cordinates from sent_platform.lua
			Cannon.Position = Transform.PointToWorld( new Vector3( 0, -25.2f, 41.3f ) ); ;
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

			ConnectedCannon = Cannon;

			Weld( (Prop)Cannon );
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
		PhysicsGroup?.Wake();
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
