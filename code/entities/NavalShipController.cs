using Sandbox;
using Sandbox.Tools;


[Library( "nvl_ship_controller", Title = "Ship Controller", Spawnable = true )]
public partial class NavalShipController : Prop, IUse, IPhysicsUpdate
{
	
	public override void Spawn() 
	{
		base.Spawn();

		SetModel( "models/citizen_props/chair03.vmdl" );
	}

	public bool IsUsable( Entity user )
	{
		return true;
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

	public void OnPostPhysicsStep( float dt )
	{
		if ( !this.IsValid() )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;
	}
}
