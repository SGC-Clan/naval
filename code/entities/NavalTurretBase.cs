using Sandbox;
using Sandbox.Tools;
using System;
using System.Runtime.CompilerServices;

[Library( "nvl_turret_base", Title = "Naval Turret Example", Spawnable = true )]
public partial class NavalTurretBase : Prop, IUse
{
	public Vector3 TargetPos = new Vector3( 1000,5000,0 ); //at what position should turret rotate towards
	public readonly Vector3[] TurretBones = new Vector3[] //array storing original model bone positions so we can transform them
	{
			new Vector3( 0, 0, 0 ),
			new Vector3( 0, 42f, 0 ),
			new Vector3( 0, 20f, 15f ),
			new Vector3( 0, 0, 123f )
	}; 
	public float Pitch = 30f % 360.0f;
	public float Yaw = 55f % 360.0f;
	public float ShootInterval = 1.2f; //(seconds) delay between shots
	public float ReloadTime = 4f; //(seconds) how long it takes to reload the turret
	public float RecoilForce = 100f; //(hu) how much kickback should the cannon recieve after each shoot
	public float ProjectileVelocity = 5000f; //(hu) how much velocity should be applied to cannon ball upon firing
	public bool IsReloaded = true;
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/courier/88skc30tur.vmdl" );
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
			//TEMP set owner is here!
			Owner = user;

			Particles.Create( "particles/naval_fuze_sparks.vpcf", this, "spark" );

			IsReloaded = false;

			ShootCannonball();
			
		}

		return false;
	}

	[Event.Frame]
	public void OnFrame()
	{

		Yaw += 10.0f * Time.Delta;
		Yaw %= 360.0f;

		Pitch += (float)Math.Sin( Time.Delta  ) * 100.0f;
		Pitch %= 360.0f;

		// Modify Yaw
		string YawBoneID = "joint2"; // turret bearing
		Vector3 YawBoneOffset = Transform.PointToWorld( TurretBones[1] );

		var transform = new Transform( YawBoneOffset, Rotation.From( new Angles( 0, Yaw, 90 ) ) , 1 );
		SetBoneTransform( YawBoneID, transform );

		// Modify Pitch
		string PitchBoneID = "joint3"; // turret elevation
		Vector3 PitchBoneOffset = Transform.PointToWorld( TurretBones[2] );

		transform = new Transform( PitchBoneOffset, Rotation.From( new Angles( Pitch, 0, 0 ) ), 1 );
		SetBoneTransform( PitchBoneID, transform );

		// Modify Recoil
		//int boneID = 3; //joint4 - turret recoil animation
	}

	public async void ShootCannonball() 
	{
		//await GameTask.DelayRealtimeSeconds( 1.2f );

		//Particles.Create( "particles/naval_gunpowder_smoke.vpcf", this, "muzzle" ); //suddenly stoped working.. cool
		//Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" ); // this also is not working

		var PowderSmoke = Particles.Create( "particles/naval_gunpowder_smoke.vpcf", Transform.PointToWorld( new Vector3( 0, -59, 0 ) ) ); // i have to hardcode this now
		PowderSmoke.SetForward( 0, Transform.NormalToWorld( new Vector3( 0, -1, 0 ) ) );

		var MuzzleFlash = Particles.Create( "particles/pistol_muzzleflash.vpcf", Transform.PointToWorld( new Vector3( 0, -59, 0 ) ) );
		MuzzleFlash.SetForward( 0, Transform.NormalToWorld( new Vector3( 0, -1, 0 ) ) );

		//new Sandbox.ScreenShake.Perlin( 0.5f, 2.0f, 0.5f );

		Sound.FromEntity( "nvl.blackpowdercannon.fire", this );

		// Create the cannon ball entity

		//var ShootPos = this.GetAttachment( "muzzle" ).Position; // TO:DO  Oh my fuckin god, API has changed I have no idea how to Fix IT!
		//var ShootAngle = this.GetAttachment( "muzzle" ).Rotation; // TO:DO  -||-

		var ShootPos = Transform.PointToWorld( new Vector3( 0 , -59, 0 ) ); //I had to hardcode positions for now since I cant just use an attachment as reference.. 
		var ShootAngle = Transform.RotationToWorld( Rotation.From( new Angles( 180f, 0, 180f )  ) );
		var ProjScale = Scale;

		var ent = new NavalCannonBallProjectile
		{
			Position = ShootPos,
			Rotation = ShootAngle,
			Scale = ProjScale,
		};
		ent.SetModel( "models/naval/props/props/cball.vmdl" );
		//ent.Velocity += ent.Transform.NormalToWorld( new Vector3( ProjectileVelocity, 0, 0 ) ); // this was working when GetAttachment() was also working correctly
		ent.Velocity += ent.Transform.NormalToWorld( new Vector3( 0, ProjectileVelocity, 0 ) );
		Particles.Create( "particles/dev/dev_snapshot_preview_trails_skinned.vpcf", this, null );

		ent.CannonParent = this;

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

	//public override void OnNewModel( Model model )
	//{
	//	base.OnNewModel( model );
	//	Log.Info( model.GetBodyPartForName( "joint2" ) );
	//  // var pos = ent.GetBonePhysicsBody(ent.GetBoneIndex(bone)).Position;
	//}

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
