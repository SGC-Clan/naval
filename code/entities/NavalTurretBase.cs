using Sandbox;
using Sandbox.Tools;
using System;
using System.Runtime.CompilerServices;

[Library( "nvl_turret_base", Title = "Naval Turret Example", Spawnable = true )]
public partial class NavalTurretBase : Prop, IUse
{
	public TimeSince UseDelay;
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
	public bool Fire = false; //toggle turret shooting 
	public float FuzeDelay = 0.1f; //(seconds) how much time should pass between igniting the fuze and firing
	public float RecoilForce = 100f; //(hu) how much kickback should the cannon recieve after each shoot
	public float ProjectileVelocity = 5000f; //(hu) how much velocity should be applied to cannon ball upon firing
	public bool IsReloaded = true;
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/courier/88skc30.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		//use delay
		if ( UseDelay < 1f )
		{
			UseDelay = 0;
			return false;
		}

		//Toggle Fire 
		Fire = !Fire;

		return false;
	}


	[Event.Tick]
	public void OnTick()
	{
		//Shoot as soon as Fire is True
		if ( Fire == true )
		{
			if ( IsReloaded == true )
			{
				IsReloaded = false;

				ShootProjectile();

			}
		}
	}


	[Event.Frame]
	public void OnFrame()
	{

		// ==== Turret Bone Animations ====

		//Yaw += 10.0f * Time.Delta;
		//Yaw %= 360.0f;

		//Pitch += (float)Math.Sin( Time.Delta  ) * 100.0f;
		//Pitch %= 360.0f;

		// Modify Yaw
		//string YawBoneID = "joint2"; // turret bearing
		//Vector3 YawBoneOffset = Transform.PointToWorld( TurretBones[1] );

		//var transform = new Transform( YawBoneOffset, Rotation.From( new Angles( 0, Yaw, 90 ) ) , 1 );
		//SetBoneTransform( YawBoneID, transform );

		// Modify Pitch
		//string PitchBoneID = "joint3"; // turret elevation
		//Vector3 PitchBoneOffset = Transform.PointToWorld( TurretBones[2] );

		//transform = new Transform( PitchBoneOffset, Rotation.From( new Angles( Pitch, 0, 0 ) ), 1 );
		//SetBoneTransform( PitchBoneID, transform );

		// Modify Recoil
		//int boneID = 3; //joint4 - turret recoil animation
	}

	public async void ShootProjectile() 
	{

		if ( FuzeDelay > 0f ) 
		{
			await GameTask.DelayRealtimeSeconds( FuzeDelay );
		}

		//Particles.Create( "particles/naval_gunpowder_smoke.vpcf", this, "muzzle" ); //suddenly stoped working.. cool
		//Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" ); // this also is not working

		var PowderSmoke = Particles.Create( "particles/naval_fire_effects_medium.vpcf", Transform.PointToWorld( new Vector3( 110, 0, 66 ) ) ); // i have to hardcode this now
		PowderSmoke.SetForward( 0, Transform.NormalToWorld( new Vector3( 1, 0, 0 ) ) );

		var MuzzleFlash = Particles.Create( "particles/pistol_muzzleflash.vpcf", Transform.PointToWorld( new Vector3( 110, 0, 66 ) ) );
		MuzzleFlash.SetForward( 0, Transform.NormalToWorld( new Vector3( 1, 0, 0 ) ) );

		//empty shell
		var EmptyShell = Particles.Create( "particles/naval_empty_shell_medium.vpcf", Transform.PointToWorld( new Vector3( -50, 0, 66 ) ) );
		EmptyShell.SetForward( 0, Transform.NormalToWorld( new Vector3( -1, 0, 0 ) ) );

		//new Sandbox.ScreenShake.Perlin( 0.5f, 2.0f, 0.5f );

		Sound.FromEntity( "nvl.deckgun_fire", this ); 

		// Create the cannon ball entity

		//var ShootPos = this.GetAttachment( "muzzle" ).Position; // TO:DO  Oh my fuckin god, API has changed I have no idea how to Fix IT!
		//var ShootAngle = this.GetAttachment( "muzzle" ).Rotation; // TO:DO  -||-

		var ShootPos = Transform.PointToWorld( new Vector3( 110, 0, 66 ) ); //I had to hardcode positions for now since I cant just use an attachment as reference.. 
		var ShootAngle = Transform.RotationToWorld( Rotation.From( new Angles( 0, 0, 0 )  ) );
		var ProjScale = Scale;

		var ent = new NavalProjectileBase
		{
			Position = ShootPos,
			Rotation = ShootAngle,
			Scale = ProjScale,
			Owner = Owner,
		};
		ent.SetModel( "models/naval/weapons/shell2.vmdl" );
		//ent.Velocity += ent.Transform.NormalToWorld( new Vector3( ProjectileVelocity, 0, 0 ) ); // this was working when GetAttachment() was also working correctly
		ent.Velocity += Transform.NormalToWorld( new Vector3( ProjectileVelocity, 0, 0 ) );

		ent.CannonParent = this;

		//recoil
		this.Velocity += this.Transform.NormalToWorld( new Vector3( -RecoilForce, 0, 0) );
		//screen shake
		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 2.0f, 0.5f );
		}

		// Reload
		await GameTask.DelayRealtimeSeconds( ReloadTime );

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
