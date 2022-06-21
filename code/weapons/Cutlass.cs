using Sandbox;
using System;

[Spawnable]
[Library( "weapon_cutlass", Title = "Sword (Cutlass)" )]
partial class Cutlass : Weapon
{
	public override string ViewModelPath => "models/naval/weapons/cutlass2.vmdl";

	public override float PrimaryRate => 0.8f;
	public override float SecondaryRate => 0.8f;

	public bool IsBlocking = false; 

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/weapons/w_cutlass2/w_cutlass2.vmdl" );

	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );
		PlaySound( "nvl.sword.draw" );
		ViewModelEntity?.SetAnimParameter( "Draw", true );
	}

	public override void CreateViewModel()
	{
		base.CreateViewModel();

		//Delete that creepy non animated left hand please
		SetBoneTransform( 0, new Transform( new Vector3(0,0,100)), true );
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );
		PlaySound( "nvl.sword.holster" );
	}

	public override async void AttackPrimary()
	{
		IsBlocking = false;

		ViewModelEntity?.SetAnimParameter( "AttackPrimary", true );

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );

		await GameTask.DelayRealtimeSeconds( 0.4f );

		if ( MeleeAttack() )
		{
			OnMeleeHit();

		}
		else
		{
			OnMeleeMiss();
		}

		PlaySound( "nvl.sword.swing" );
	}

	private bool MeleeAttack()
	{
		var forward = Owner.EyeRotation.Forward;
		forward = forward.Normal;

		bool hit = false;
		ViewModelEntity?.SetAnimParameter( "Miss", true );

		//World model animations
		( Owner as AnimatedEntity)?.SetAnimParameter( "hit", true );
		(Owner as AnimatedEntity)?.SetAnimParameter( "hit_strenght", 0.5f );

		Random rand = new Random();
		float RandAttackAnim = (float)rand.Next( 3 );
		(Owner as AnimatedEntity)?.SetAnimParameter( "holdtype_attack", RandAttackAnim );

		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 80, 20.0f ) )
		{
			if ( !tr.Entity.IsValid() ) continue;

			if ( tr.Entity.IsWorld )
			{
				PlaySound( "nvl.sword.clash" ); // Only make this annoying sound when you hit ground or something static
				ViewModelEntity?.SetAnimParameter( "Stun", true );//temp stun code 
				ViewModelEntity?.SetAnimParameter( "Miss", false );
			}

			tr.Surface.DoBulletImpact( tr );

			hit = true;

			if ( !IsServer ) continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100, 25 )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}

		}

		return hit;
	}

	[ClientRpc]
	private void OnMeleeMiss()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
		{
			//_ = new Sandbox.ScreenShake.Perlin();
		}
	}

	[ClientRpc]
	private void OnMeleeHit()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
		{
			//_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, 3.0f );
		}
	}

	public override void AttackSecondary()
	{
		ViewModelEntity?.SetAnimParameter( "Block", true );
		IsBlocking = true;
	}

	public override bool CanReload()
	{
		return false;
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 4 );
		anim.SetAnimParameter( "holdtype_pose_hand", 0.06f ); //nearly pinched fingers
		anim.SetAnimParameter( "aimat_weight", 1.0f );

		//if blocking set animation to 2 handed
		if ( IsBlocking ) {
			anim.SetAnimParameter( "holdtype_handedness", 0 );
		} else {
			anim.SetAnimParameter( "holdtype_handedness", 1 );
		}
	}

}
