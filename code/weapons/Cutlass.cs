using Sandbox;

[Library( "weapon_cutlass", Title = "Sword (Cutlass)", Spawnable = true )]
partial class Cutlass : Weapon
{
	public override string ViewModelPath => "models/naval/weapons/cutlass2.vmdl";

	public override float PrimaryRate => 0.8f;
	public override float SecondaryRate => 0.8f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/naval/weapons/w_cutlass2/w_cutlass2.vmdl" );

	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );
		PlaySound( "nvl.sword.draw" );
		ViewModelEntity?.SetAnimBool( "Draw", true );
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
		ViewModelEntity?.SetAnimBool( "AttackPrimary", true );

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
		var forward = Owner.EyeRot.Forward;
		forward = forward.Normal;

		bool hit = false;
		ViewModelEntity?.SetAnimBool( "Miss", true );

		//World model animation (temp)
		(Owner as AnimEntity)?.SetAnimBool( "hit", true );
		(Owner as AnimEntity)?.SetAnimFloat( "hit_strenght", 0.5f );

		foreach ( var tr in TraceBullet( Owner.EyePos, Owner.EyePos + forward * 80, 20.0f ) )
		{
			if ( !tr.Entity.IsValid() ) continue;

			if ( tr.Entity.IsWorld )
			{
				PlaySound( "nvl.sword.clash" ); // Only make this annoying sound when you hit ground or something static
				ViewModelEntity?.SetAnimBool( "Stun", true );//temp stun code 
				ViewModelEntity?.SetAnimBool( "Miss", false );
			}

			tr.Surface.DoBulletImpact( tr );

			hit = true;

			if ( !IsServer ) continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPos, forward * 100, 25 )
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
			_ = new Sandbox.ScreenShake.Perlin();
		}
	}

	[ClientRpc]
	private void OnMeleeHit()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
		{
			_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, 3.0f );
		}
	}

	public override void AttackSecondary()
	{
		ViewModelEntity?.SetAnimBool( "Block", true );
	}

	public override bool CanReload()
	{
		return false;
	}

}
