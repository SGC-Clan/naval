using Sandbox;
using System.Linq;

[Spawnable]
[Library( "directional_gravity", Title = "Directional Gravity" )]
public partial class DirectionalGravity : Prop
{
	bool enabled = false;

	public override void Spawn()
	{
		base.Spawn();

		DeleteOthers();

		SetModel( "models/arrow.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		enabled = true;
	}

	private void DeleteOthers()
	{
		// Only allow one of these to be spawned at a time
		foreach ( var ent in All.OfType<DirectionalGravity>()
			.Where( x => x.IsValid() && x != this ) )
		{
			ent.Delete();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Game.PhysicsWorld.Gravity = Vector3.Down * 800.0f;

		enabled = false;
	}

	[Event.Tick]
	protected void UpdateGravity()
	{
		if ( !Game.IsServer )
			return;

		if ( !enabled )
			return;

		if ( !this.IsValid() )
			return;

		Game.PhysicsWorld.Gravity = Rotation.Down * 800.0f;
	}
}
