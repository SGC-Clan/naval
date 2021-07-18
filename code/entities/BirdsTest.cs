using Sandbox;

[Library( "nvl_birdstest", Title = "Birds Test", Spawnable = true )]
public partial class BirdsTest : Prop
{

	public Particles BirdsParticles;

	private static float GravityScale => 0.02f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/hotdog01.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		PhysicsBody.GravityScale = GravityScale;
		RenderColor = Color.Random.ToColor32();

		BirdsParticles = Particles.Create( "particles/birds_flying_flat.vpcf", this, null );
		BirdsParticles.SetEntity(0, this );
	}

	[Event.Tick]
	public void OnTick()
	{
		if ( BirdsParticles != null )
		{
			BirdsParticles.SetPosition( 0, Position );
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( BirdsParticles != null )
		{
			BirdsParticles.Destroy( true );
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();
	}

	public void OnPostPhysicsStep( float dt )
	{
		if ( !this.IsValid() )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;

		body.GravityScale = GravityScale;
	}
}
