using Sandbox;
using System;

[Spawnable]
[Library( "nvl_turret_base", Title = "Naval Turret Example" )]
public partial class NavalTurretBase : AnimatedEntity, IUse
{
	public bool nvl_debug = true;

	public TimeSince UseDelay;
	public Vector3 TargetPos = new Vector3( 1000,5000,0 ); //at what position should turret rotate towards
	public Vector3[] TurretBones = new Vector3[] //array storing original model bone positions so we can transform them
	{
		new Vector3( 0, 0, 0 ), //"joint1"
		new Vector3( 0, 0, 42.5f ), //"joint2"
		new Vector3( 0, 15f, 42f-18f ), //"joint3"
		new Vector3( 0f, -126f, 0f )  //"joint4"
	};
	public readonly string[] RandomTurretModels = new string[]
	{
		"models/courier/88skc30.vmdl",
		"models/courier/88skc30tur.vmdl",
		"models/courier/swivel1.vmdl",
		"models/courier/bofors2.vmdl",
		"models/courier/pompom.vmdl",
		"models/courier/pompom2.vmdl",
		"models/courier/oerlikon.vmdl",
		"models/courier/type90.vmdl",
		"models/courier/twinm2.vmdl",
		"models/courier/grafspee.vmdl",
		"models/courier/agano.vmdl",
	};
	public float Pitch = 30f % 360.0f;
	public float Yaw = 55f % 360.0f;
	public float RecoilAnim = 0;
	public float ShootInterval = 1.2f; //(seconds) delay between shots
	public float ReloadTime = 4f; //(seconds) how long it takes to reload the turret
	public bool Fire; //toggle turret shooting 
	public float FuzeDelay = 0.1f; //(seconds) how much time should pass between igniting the fuze and firing
	public float RecoilForce = 100f; //(hu) how much kickback should the cannon recieve after each shoot
	public float ProjectileVelocity = 5000f; //(hu) how much velocity should be applied to cannon ball upon firing
	public float ProjectileSpread = 14f; //(degrees) how inaccurate is the turret when firing
	public bool IsReloaded = true;
	public override void Spawn()
	{
		base.Spawn();

		//Log.Info( "==== RandomTurretModels ====" );
		//SetModel( RandomTurretModels[ Rand.Int(0, RandomTurretModels.Length-1 ) ] ); //assign random model
		SetModel( "models/courier/88skc30.vmdl" );
		UpdateTurretBones();

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

		//TEMP set Owner
		Owner = user;

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
		// BUG: SetBoneTransform() with world = false is broken! https://github.com/Facepunch/sbox-issues/issues/466

		Yaw = (float)Math.Sin( 5000 + UseDelay/10 + Time.Delta ) * 150.0f;
		Yaw %= 360.0f;

		Pitch = 20 + (float)Math.Sin( UseDelay + Time.Delta  ) * 50.0f;
		Pitch %= 360.0f;

		//For now im gona use AnimGraph even tho its far from ideal
		SetAnimParameter( "turret_aim_pos", Transform.PointToLocal( new Vector3(100,100,800) ) );


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



		//var ShootPos = this.GetAttachment( "muzzle" ).Position; // TO:DO  Oh my fuckin god, API has changed I have no idea how to Fix IT!
		//var ShootAngle = this.GetAttachment( "muzzle" ).Rotation; // TO:DO  -||-

		var MuzzleBoneTransform = GetBoneTransform( "joint4", true );
		var ShootPos = MuzzleBoneTransform.Position; //Transform.PointToWorld( new Vector3( 110, 0, 66 ) ); //I had to hardcode positions for now since I cant just use an attachment as reference.. 
		var ShootRot = MuzzleBoneTransform.Rotation; //Transform.RotationToWorld( Rotation.From( new Angles( 0, 0, 0 )  ) );
		ShootRot *= Rotation.From( Rand.Float( -ProjectileSpread, ProjectileSpread ), Rand.Float( -ProjectileSpread, ProjectileSpread ), Rand.Float( -ProjectileSpread, ProjectileSpread ) );
		var ProjScale = Scale;

		// Create the projectile entity
		var ent = new NavalProjectileBase
		{
			Position = ShootPos,
			Rotation = ShootRot,
			Scale = ProjScale,
			Owner = Owner,
			LastPosition = Position,
		};
		ent.SetModel( "models/naval/weapons/shell2.vmdl" );
		//ent.Velocity += ent.Transform.NormalToWorld( new Vector3( ProjectileVelocity, 0, 0 ) ); // this was working when GetAttachment() was also working correctly
		ent.Velocity += Transform.NormalToWorld( new Vector3( ProjectileVelocity, 0, 0 ) );

		ent.TurretParent = this;

		//recoil
		this.Velocity += this.Transform.NormalToWorld( new Vector3( -RecoilForce, 0, 0) );
		SetAnimParameter( "turret_recoil_anim", true );
		SetAnimParameter( "turret_recoil_strenght", 4f );
		SetAnimParameter( "turret_recoil_offset", Position );

		//screen shake
		if ( IsLocalPawn )
		{
			//new Sandbox.ScreenShake.Perlin( 0.5f, 2.0f, 0.5f );  //screen shake got removed for some reason ?

		}

		// Reload
		await GameTask.DelayRealtimeSeconds( ReloadTime );

		Sound.FromEntity( "nvl.blackpowdercannon.reload", this );
		IsReloaded = true;

	}

	public void UpdateTurretBones()
	{

		this.TurretBones = new Vector3[] //array storing original model bone positions so we can transform them
		{
			GetBoneTransform("joint1").Position,
			GetBoneTransform("joint2").Position,
			GetBoneTransform("joint3").Position,
			GetBoneTransform("joint4").Position
		};

		Log.Info( "===============" );
		for ( int i = 0; i < TurretBones.Length; ++i )
		{
			Log.Info( TurretBones[i] );
		}
		Log.Info( "===============" );
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );
	}

	public void Remove()
	{
		//PhysicsGroup?.Wake();
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
