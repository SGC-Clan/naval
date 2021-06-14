using Sandbox;
using Sandbox.Tools;
using System.Runtime.CompilerServices;

[Library( "nvl_blackpowder_cannon", Title = "Blackpowder Cannon", Spawnable = true )]
public partial class BlackpowderCannonEntity : Prop, IUse
{
	public float WickTime = 1.2f; //(seconds) how long the wick burns before shooting the cannonball
	public float ReloadTime = 4f; //(seconds) how long it takes to reload the cannon
	public float RecoilForce = 250f; //(hu) how much kickback should the cannon recieve after each shoot
	public float ProjectileVelocity = 2000f; //(hu) how much velocity should be applied to cannon ball upon firing
	public bool IsReloaded = true;
	public Sound WickSound = new Sound();
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
		if ( IsReloaded == true )
		{
			WickSound = Sound.FromEntity( "nvl.blackpowdercannon.wick", this );
			Particles.Create( "particles/naval_fuze_sparks.vpcf", this, "spark" );

			IsReloaded = false;

			ShootCannonball();
			
		}

		return false;
	}

	public async void ShootCannonball() 
	{
		await GameTask.DelayRealtimeSeconds( 1.2f );

		WickSound.Stop();

		Particles.Create( "particles/naval_gunpowder_smoke.vpcf", this, "muzzle" );
		Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );

		Sound.FromEntity( "nvl.blackpowdercannon.fire", this );

		// Create the cannon ball entity

		//var ShootPos = this.GetAttachment( "muzzle" ).Position; // TO:DO  Oh my fuckin god, API has changed I have no idea how to Fix IT!
		//var ShootAngle = this.GetAttachment( "muzzle" ).Rotation; // TO:DO  -||-

		var ShootPos = Transform.PointToWorld( new Vector3( 0 , -56, 0 ) ); //I had to hardcode positions for now since I cant just use an attachment as reference.. 
		var ShootAngle = Transform.RotationToWorld( Rotation.From( new Angles( 180f, 0, 180f )  ) );

		var ent = new Prop
		{
			Position = ShootPos,
			Rotation = ShootAngle,
		};
		ent.SetModel( "models/naval/props/props/cball.vmdl" );
		//ent.Velocity += ent.Transform.NormalToWorld( new Vector3( ProjectileVelocity, 0, 0 ) ); // this was working when GetAttachment() was also working correctly
		ent.Velocity += ent.Transform.NormalToWorld( new Vector3( 0, ProjectileVelocity, 0 ) );
		Particles.Create( "particles/dev/dev_snapshot_preview_trails_skinned.vpcf", this, "" );

		//recoil
		this.Velocity += this.Transform.NormalToWorld( new Vector3(0, RecoilForce, 0) );
		//screen shake
		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 2.0f, 0.5f );
		}

		// Reload
		await GameTask.DelayRealtimeSeconds( 2f );

		Sound.FromEntity( "nvl.blackpowdercannon.reload", this );
		IsReloaded = true;

	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}
}
