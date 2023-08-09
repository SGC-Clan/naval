using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.entities.Weapons
{
	using Sandbox;

	[Spawnable]
	[Library( "naval_baseturret", Title = "Turret Example" )]
	public partial class BaseTurret : Prop, IUse
	{
		[Net]
		public bool Enabled { get; set; } = true;
		[Net]
		public NavalPlayer User { get; set; }

		public Vector3 TargetedPosition { get; set; }

		public Angles AimAngle { get; set; }
		[Net]
		public Angles CurrentAimAngle { get; set; }

		public TimeSince ReloadTimer { get; set; }
		public int ClipSize { get; set; }
		public AmmoType AmmoType { get; set; }

		public BoneCollection OgBones;

		public Vector3[] BonePositions = new Vector3[]
		{
			//new Vector3( 0, 0, 0 ),
			//new Vector3( 0, 0, 38.43f ),
			//new Vector3( -16.76f, 0,  64.93f ),
			//new Vector3( 61.50f, 0, 64.93f )

			new Vector3( 0, 0, 0 ),
			new Vector3( 0, 0, 2.35f ),
			new Vector3( -3.225f, 0, 13.75f ),
			new Vector3( 46f, 0, 2f ),
			new Vector3( 2.5f, 0, 0 ),
		};

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models_and_materials/weapons/m1917/m1917.vmdl" ); //"models_and_materials/weapons/45mm_gun/45mm_gun.vmdl"
		}

		public override void OnNewModel( Model model )
		{
			base.OnNewModel( model );

			//collect all the original spooky bonez 
			OgBones = Model.Bones;


			Log.Info( "---- Bones for Model: "+Model.ResourceName+" ----" );
			for ( int i = 0; i < model.BoneCount; i++ )
			{
				int boneParent = model.GetBoneParent( i );
				if ( i >= 0 )
				{
					Log.Info( "Bone Name: " + model.GetBoneName( i ) );
					Log.Info( "Pos: " + model.GetBoneTransform( i ).Position );
					Log.Info( "Ang: " + model.GetBoneTransform( i ).Rotation.Angles() );
				}
			}
		}

		[GameEvent.Tick]
		public void Tick() 
		{
			//if player is too far away, reset
			if ( User != null && User.Position.Distance( Position ) > 100 )
				User = null;

			CurrentAimAngle = new Angles( 0,0,90 );

			if ( User != null ) 
			{
				CurrentAimAngle = User.AimRay.Forward.EulerAngles;
			}
		}


		[GameEvent.Client.Frame]
		public void OnFrame()
		{

			// ============== Turret aiming animation ==============
			var BonePos1 = BonePositions[1];
			var NewYawTransform = Transform.ToWorld( new Transform( BonePos1 * Scale, Rotation.From( new Angles( 0, CurrentAimAngle.yaw, 0 ) ) ) );
			NewYawTransform.Scale = Scale;
			SetBoneTransform( "yaw", NewYawTransform );

			var BonePos2 = BonePositions[2];
			var NewPitchTransform = NewYawTransform.ToWorld( new Transform( BonePos1 + BonePos2 * Scale, Rotation.From( new Angles( CurrentAimAngle.pitch, 0, 0 ) ) ) );
			NewPitchTransform.Scale = Scale;
			SetBoneTransform( "pitch", NewPitchTransform );

			var BonePos3 = BonePositions[3];
			var NewBarrelTransform = NewPitchTransform.ToWorld( new Transform( BonePos3 * Scale, Rotation.From( new Angles( 0, 0, 0 ) ) ) );
			NewBarrelTransform.Scale = Scale;
			SetBoneTransform( "barrel01", NewBarrelTransform );

			var BonePos4 = BonePositions[4];
			var NewBarrelEndTransform = NewBarrelTransform.ToWorld( new Transform( BonePos4 * Scale, Rotation.From( new Angles( 0, 0, 0 ) ) ) );
			NewBarrelEndTransform.Scale = Scale;
			SetBoneTransform( "barrel01_end", NewBarrelEndTransform );
			// =============================================

			DebugSkeleton( this, Color.Red, 0, false );
		}


		public bool IsUsable( Entity user )
		{
			return true;
		}

		public bool OnUse( Entity user )
		{
			if ( User != null )
			{
				User = null;
				return false;
			}

			User = user as NavalPlayer;

			return false;
		}

		//DebugOverlay.Skeleton + highlight bones with _end suffix 
		public bool DebugSkeleton( Entity ent, Color color, float duration = 0f, bool depthTest = true )
		{
			ModelEntity modelEntity = ent as ModelEntity;
			if ( modelEntity != null )
			{
				int boneCount = modelEntity.BoneCount;
				if ( boneCount <= 1 )
				{
					return false;
				}

				for ( int i = 0; i < boneCount; i++ )
				{
					int boneParent = modelEntity.GetBoneParent( i );
					if ( i >= 0 )
					{
						Transform boneTransform = modelEntity.GetBoneTransform( i );
						DebugOverlay.Line( end: modelEntity.GetBoneTransform( boneParent ).Position, start: boneTransform.Position, color: color, duration: duration, depthTest: depthTest );

						var name = modelEntity.GetBoneName( i );
						if ( name.EndsWith( "base" ) || name.EndsWith( "root" ) )
						{
							DebugOverlay.Sphere( boneTransform.Position, 0.25f, Color.White, 0, false );
						}
						else 
						{
							DebugOverlay.Axis( boneTransform.Position, boneTransform.Rotation, 10, 0, false );
						}


						//highlight _end bones
						if ( modelEntity.GetBoneName( i ).EndsWith( "_end" ) )
						{
							DebugOverlay.Sphere( boneTransform.Position, 0.25f, Color.Red, 0, false );
							DebugOverlay.Text( "Aim:"+CurrentAimAngle, boneTransform.Position + new Vector3(0,0,10) );
						}

					}
				}

				return true;
			}

			return false;
		}

	}
}
