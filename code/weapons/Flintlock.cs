using Sandbox;

[Library( "weapon_flintlock", Title = "Flintlock Pistol", Spawnable = true )]
partial class Flintlock : Weapon
{
	public override string ViewModelPath => "models/naval/weapons/v_pistol.vmdl";
	public override float PrimaryRate => 0.5f;
	public override float SecondaryRate => 0.5f;
	public override float ReloadTime => 2;

	public bool BulletIsLoaded = true; // start reloaded
	public TimeSince TimeSinceDischarge { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/weapons/w_pistol/w_pistol.vmdl" );
	}

	/// <summary>
	/// Lets make primary attack semi automatic
	/// </summary>
	public override bool CanPrimaryAttack()
	{
		if ( !Input.Pressed( InputButton.Attack1 ) )
			return false;

		return base.CanPrimaryAttack();
	}

	public override void Reload()
	{
		base.Reload();

		ViewModelEntity?.SetAnimBool( "reload", true );

		Sound.FromEntity( "nvl.flintlock.reloadflock", this );

		BulletIsLoaded = true;
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		//Check if bullet is in the chember
		if ( BulletIsLoaded == true )
		{
			//Shoot the gun
			BulletIsLoaded = false;
			Shoot( Owner.EyePos, Owner.EyeRot.Forward );
		}
		else
		{
			//Try shooting without a bullet and make couple sparks instead
			ShootEmpty();
		}
	}

	private void Shoot( Vector3 pos, Vector3 dir )
	{
		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();


		bool InWater = Physics.TestPointContents( pos, CollisionLayer.Water );
		var forward = dir * (InWater ? 500 : 4000);

		//
		// ShootBullet is coded in a way where we can have bullets pass through shit
		// or bounce off shit, in which case it'll return multiple results
		//
		foreach ( var tr in TraceBullet( pos, pos + dir * 4000 ) )
		{
			//make a bulletpass sound
			Sound.FromWorld( "nvl.bulletpass", tr.EndPos );

			//custom bullet tracer effects
			var tracer = Particles.Create( "particles/naval_hitscan_projectile_small.vpcf", this, "muzzle" );
			//tracer.SetPosition( 0, tr.StartPos );
			tracer.SetPosition( 1, tr.EndPos );

			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			//
			// We turn predictiuon off for this, so aany exploding effects
			//
			using ( Prediction.Off() )
			{
				var damage = DamageInfo.FromBullet( tr.EndPos, forward.Normal * 20, 60 )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damage );
			}

		}
	}

	private void ShootEmpty()
	{

		Sound.FromEntity( "nvl.flintlock.blankshot", this );
		if ( IsClient )
		{
			Particles.Create( "particles/naval_fuze_sparks.vpcf", EffectEntity, "spark" );
		}

		ViewModelEntity?.SetAnimBool( "shootempty", true );
	}

	private void Discharge()
	{
		if ( TimeSinceDischarge < 0.5f )
			return;

		TimeSinceDischarge = 0;

		var muzzle = GetAttachment( "muzzle" ) ?? default;
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;
		Shoot( pos, rot.Forward );

		ApplyAbsoluteImpulse( rot.Backward * 200.0f );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( eventData.Speed > 500.0f && BulletIsLoaded == true )
		{
			BulletIsLoaded = false;
			Discharge();
		}
	}

	public override void Simulate( Client owner )
	{
		base.Simulate( owner );

		//DebugTrace( owner );

		//if ( !NavMesh.IsLoaded )
		//	return;

		//var forward = owner.EyeRot.Forward * 2000;


		//var tr = Trace.Ray( owner.EyePos, owner.EyePos + forward )
		//				.Ignore( owner )
		//				.Run();

		//var closestPoint = NavMesh.GetClosestPoint( tr.EndPos );

		//DebugOverlay.Line( tr.EndPos, closestPoint, 0.1f );

		//DebugOverlay.Axis( closestPoint, Rotation.LookAt( tr.Normal ), 2.0f, Time.Delta * 2 );
		//DebugOverlay.Text( closestPoint, $"CLOSEST Walkable POINT", Time.Delta * 2 );

		//NavMesh.BuildPath( Owner.Position, closestPoint );
	}

	//public void DebugTrace( Player player )
	//{
	//	for ( float x = -10; x < 10; x += 1.0f )
	//		for ( float y = -10; y < 10; y += 1.0f )
	//		{
	//			var tr = Trace.Ray( player.EyePos, player.EyePos + player.EyeRot.Forward * 4096 + player.EyeRot.Left * (x + Rand.Float( -1.6f, 1.6f )) * 100 + player.EyeRot.Up * (y + Rand.Float( -1.6f, 1.6f )) * 100 ).Ignore( player ).Run();

	//			if ( IsServer ) DebugOverlay.Line( tr.EndPos, tr.EndPos + tr.Normal, Color.Cyan, duration: 20 );
	//			else DebugOverlay.Line( tr.EndPos, tr.EndPos + tr.Normal, Color.Yellow, duration: 20 );
	//		}
	//}

	[ClientRpc]
	public virtual void ShootEffects()
	{
		Host.AssertClient();

		var muzzle = EffectEntity.GetAttachment( "muzzle" );
		//bool InWater = Physics.TestPointContents( muzzle.Position, CollisionLayer.Water );

		Sound.FromEntity( "nvl.flintlock.fire", this );
		Particles.Create( "particles/naval_gunpowder_smoke.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/naval_fuze_sparks.vpcf", EffectEntity, "spark" );
		Particles.Create( "particles/pistol_muzzleflash.vpcf", this, "muzzle" );

		ViewModelEntity?.SetAnimBool( "shoot", true );
		(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );
		CrosshairPanel?.OnEvent( "onattack" );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin( 0.5f, 2.0f, 0.5f );
		}
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 1 );
		anim.SetParam( "holdtype_pose_hand", 0.06f );
		anim.SetParam( "aimat_weight", 1.0f );
	}

}
