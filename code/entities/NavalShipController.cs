using Sandbox;
using Sandbox.Tools;


[Library( "nvl_ship_controller", Title = "Ship Controller", Spawnable = true )]
public partial class NavalShipController : Prop, IUse
{
	public Player SeatUser = null; //player can sit on this entity
	
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
		ConsoleSystem.Run( "naval_sit" );

		return true;
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
