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
			new Vector3( 0, 0, 2.3f ),
			new Vector3( -3.26f, 0, 16.07f ),
			new Vector3( 41.74f, 0, 18.07f ),
		};

		public Angles[] BoneAngles = new Angles[]
		{
			new Angles( 0, 0, 0 ),
			new Angles( 0, 90, 0 ),
			new Angles( 0, 0, 0 ),
			new Angles( 0, 0, 0 ),
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

			for ( int i = 0; i < model.BoneCount; i++ )
			{
				int boneParent = model.GetBoneParent( i );
				if ( i >= 0 )
				{
					Log.Info( "Bone Name: "+ model.GetBoneName( i ) );
					Log.Info( "Pos: " + model.GetBoneTransform( i ).Position );
					Log.Info( "Ang: " + model.GetBoneTransform( i ).Rotation.Angles() );
				}
			}
		}

		[GameEvent.Tick]
		public void Tick() 
		{
			//if player is too far away, reset
			//if ( User != null && User.Position.Distance( Position ) > 100 )
			//	User = null;

			CurrentAimAngle = new Angles( 0,0,90 );

			if ( User != null ) 
			{
				CurrentAimAngle = User.AimRay.Forward.EulerAngles;
			}
		}


		[GameEvent.Client.Frame]
		public void OnFrame()
		{
			if ( OgBones == null )
				return;

			//some stuff I probably need:
			//SetBoneTransform - Set real physical bone transform
			//SetBone - Set visual only bone transform
			//Rotation.SmoothDamp();
			//ResetBones();

			// Drone Example:
			// var transform = Transform.ToWorld( new Transform( turbinePositions[i] * Scale, Rotation.From( new Angles( 0, spinAngle, 0 ) ) ) );
			// transform.Scale = Scale;
			// SetBoneTransform( i, transform );



			// ============== Working test ==============
			var TestBonePos1 = new Vector3( 0, 0, 2.35f );
			var TestBoneAng1 = new Angles( 0, 0, 0 );

			var NewYawTransform = Transform.ToWorld( new Transform( TestBonePos1 * Scale, Rotation.From( TestBoneAng1 + new Angles( 0, CurrentAimAngle.yaw, 0 ) ) ) );
			NewYawTransform.Scale = Scale;
			SetBoneTransform( "yaw", NewYawTransform );

			var TestBonePos2 = new Vector3( -3.225f, 0, 13.75f );
			var TestBoneAng2 = new Angles( 0, 0, 0 );

			var NewPitchTransform = NewYawTransform.ToWorld( new Transform( TestBonePos1 + TestBonePos2 * Scale, Rotation.From( TestBoneAng2 + new Angles( CurrentAimAngle.pitch, 0, 0 ) ) ) );
			NewPitchTransform.Scale = Scale;
			//NewPitchTransform.Position = NewPitchTransform.PointToWorld( TestBonePos2 * -1 );
			NewPitchTransform = NewPitchTransform.RotateAround( Transform.Position, Rotation.From( TestBoneAng2 ) );
			SetBoneTransform( "pitch", NewPitchTransform );

			var TestBonePos3 = new Vector3( 46f, 0, 2f );
			var TestBoneAng3 = new Angles( 0, 0, 0 );

			var NewBarrelTransform = NewPitchTransform.ToWorld( new Transform( TestBonePos3 * Scale, Rotation.From( TestBoneAng3 ) ) );
			NewBarrelTransform.Scale = Scale;
			//NewBarrelTransform = NewBarrelTransform.RotateAround( Transform.Position, Rotation.From( TestBoneAng3 ) );
			SetBoneTransform( "barrel01", NewBarrelTransform );

			var TestBonePos4 = new Vector3( 2.5f, 0, 0 );
			var TestBoneAng4 = new Angles( 0, 0, 0 );

			var NewBarrelEndTransform = NewBarrelTransform.ToWorld( new Transform( TestBonePos4 * Scale, Rotation.From( TestBoneAng4 ) ) );
			NewBarrelEndTransform.Scale = Scale;
			SetBoneTransform( "barrel01_end", NewBarrelEndTransform );
			// =============================================


			//Local bone transform test

			//var yawBoneTransform = OgBones.GetBone( "yaw" ).LocalTransform;
			//var yawRotationOffset = Rotation.From( 0, 0, 0 );
			//var yawTransform = Transform.ToWorld( new Transform( yawBoneTransform.Position * Scale, Rotation.From( new Angles( 0, CurrentAimAngle.yaw, 0 ) ) * yawRotationOffset ) );
			//yawTransform.Scale = Scale;
			//SetBoneTransform( "yaw", yawTransform );

			//var pitchBoneTransform = OgBones.GetBone( "pitch" ).LocalTransform;
			//var pitchRotationOffset = Rotation.From(0,0,0);
			//var pitchTransform = Transform.ToWorld( new Transform( pitchBoneTransform.Position * Scale, Rotation.From( new Angles( CurrentAimAngle.pitch, CurrentAimAngle.yaw, 0 ) ) * pitchRotationOffset ) );
			//pitchTransform.Scale = Scale;
			//SetBoneTransform( "pitch", pitchTransform );


			//var OgYawBone = OgBones.GetBone( "yaw" );
			//var YawAngleOffset = new Angles( 0, 0, 0 ); //Angles( 33, 0, 90 )
			//Rotation CurrentYawTarget = Transform.Rotation * Rotation.From( new Angles( 0, CurrentAimAngles.yaw + 180, 0 ) + YawAngleOffset );
			//Transform NewYawTransform = new Transform( Transform.ToWorld( OgYawBone.LocalTransform ).Position, CurrentYawTarget ); //Transform.Position + OgYawBone.LocalTransform.Position
			//SetBoneTransform( "yaw", NewYawTransform );

			//var OgPitchBone = OgBones.GetBone( "pitch" );
			//var PitchAngleOffset = new Angles( 0, 0, 0 );
			//Rotation CurrentPitchTarget = Transform.Rotation * Rotation.From( new Angles( CurrentAimAngles.pitch, CurrentAimAngles.yaw, 0 ) + YawAngleOffset + PitchAngleOffset );
			//Transform NewPitchTransform = new Transform( Transform.ToWorld( OgPitchBone.LocalTransform ).Position, CurrentPitchTarget );
			//SetBoneTransform( "pitch", NewPitchTransform );

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
