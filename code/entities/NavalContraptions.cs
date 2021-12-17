using Sandbox;
using System;
using System.Collections.Generic;

[Library( "nvl_contraption_base", Title = "Naval Contraption Base", Spawnable = true )]
public partial class NavalContraptions : Prop
{
	//public Prop[] HullProps { get; set; } //list of props this contraption is build of
	List<Prop> HullProps = new List<Prop>();

	public NavalPlayer ContraptionOwner; //current owner of contraption


	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/courier/platform1s.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		SetInteractsExclude( CollisionLayer.Player );

		//lest test if this even works at all shall we ?
		RandomlyGenerateHull( Rand.Int( 5, 8 ), Rand.Float( 0.5f, 3 ), true );
		PhysicsGroup.SetSurface( "wood" );
	}

	public void RandomlyGenerateHull( int Props, float Scale, bool Symetry )
	{

		for ( int i = 0; i < Props; i++ )
		{
			bool IsPropCentered = Convert.ToBoolean( Rand.Int( 0, 1 ) );
			int IsPropRotationDefault = 0;//Rand.Int( 0, 1 );

			var PositionOffset = new Vector3(Rand.Float(-150,150)*Scale, Rand.Float( -50, 50 ) * Scale, Rand.Float( -20, 20 )*Scale );
			var RotationOffset = new Angles( 16 * Rand.Int( -8, 8 ) * IsPropRotationDefault, 180 * Rand.Int( -1, 1 ) * IsPropRotationDefault, 0 );
			string[] HullModels = {
				"courier/barbette4l.vmdl",
				"courier/barbette4m.vmdl",
				"courier/barbette4s.vmdl",
				"courier/barbette1l.vmdl",
				"courier/barbette1m.vmdl",
				"courier/barrack1.vmdl",
				"courier/88skc30tur.vmdl",
				"courier/agano.vmdl",
				"courier/barbette2l.vmdl",
				"courier/barbette2s.vmdl",
				"courier/barbette3l.vmdl",
				"courier/barbette3m.vmdl",
				"courier/barbette3s.vmdl",
				"courier/bridge1s.vmdl",
				"courier/bridge2l.vmdl",
				"courier/bridge2s.vmdl",
				"courier/bridge3l.vmdl",
				"courier/bridge3m.vmdl",
				"courier/bridge3s.vmdl",
				"courier/bridge4.vmdl",
				"courier/bridge4m.vmdl",
				"courier/comms1l.vmdl",
				"courier/comms1m.vmdl",
				"courier/comms1s.vmdl",
				"courier/funnel1l.vmdl",
				"courier/funnel1m.vmdl",
				"courier/funnel1s.vmdl",
				"courier/funnel2l.vmdl",
				"courier/funnel2s.vmdl",
				"courier/grafspee.vmdl",
				"courier/platform1l.vmdl",
				"courier/platform1m.vmdl",
				"courier/platform1s.vmdl",
				"courier/pontoon1l.vmdl",
				"courier/pontoon1m.vmdl",
				"courier/pontoon1s.vmdl",
				"courier/fuselage1a.vmdl",
				"courier/fuselage1b.vmdl",
				"courier/tower1es.vmdl",
				"courier/tower1m.vmdl",
				"courier/tower1s.vmdl",
				"nita/shipwreck/ship_metalgrate_01.vmdl",
				"nita/shipwreck/cliffside_pipe_01.vmdl",
				"nita/shipwreck/float02a.vmdl",
				"nita/shipwreck/float03a.vmdl",
				"nita/shipwreck/float01a.vmdl",
				"nita/shipwreck/pipe_256_01.vmdl",
				"nita/shipwreck/ship_metalbeam_01.vmdl",
				"nita/shipwreck/ship_metalbeam_01_cluster512.vmdl",
				"nita/shipwreck/submarine_ceiling_01a.vmdl",
				"nita/shipwreck/submarine_ceiling_01.vmdl",
				"nita/shipwreck/sub_pipe_02b.vmdl",
				"nita/shipwreck/sub_pipe_02.vmdl",
				"nita/shipwreck/submarine_ceiling_01b.vmdl",
				"nita/shipwreck/substation_transformer01c.vmdl",
				"nita/shipwreck/wallwebbing_01.vmdl",
				"nita/shipwreck/wallwebbing_01b.vmdl",
				"nita/shipwreck/watertank_02.vmdl",
				"nita/shipwreck/ship_crane_01.vmdl",
				"nita/shipwreck/spip_part04.vmdl",
				//"nita/shipwreck/shipwreck_part02.vmdl",
				//"nita/shipwreck/shipwreck_part03.vmdl",
				//"nita/shipwreck/shipwreck_part01.vmdl",
				"nita/shipwreck/submarine_ladder_01.vmdl",
				"courier/type90.vmdl",
				"courier/twinm2.vmdl",
				"courier/radial2.vmdl",
				"courier/torpedoturret1.vmdl",
				"courier/radar.vmdl",

			};
			var RandomModel = "models/" + HullModels[Rand.Int( 0, HullModels.Length - 1 )];

			if ( IsPropCentered ) {
				PositionOffset.y = 0;
			}

			{
				var ent = new Prop();
				ent.SetModel( RandomModel );
				ent.Transform = Transform;
				this.Weld( ent );
				ent.LocalPosition = base.Position + PositionOffset;
				ent.LocalRotation = Rotation.From( this.Rotation.Angles() + RotationOffset );
			}

			if ( !Symetry || IsPropCentered ) continue;

			RotationOffset.pitch*= -1;
			PositionOffset.y	*= -1;
			RotationOffset.yaw	*= -1;

			{
				var ent = new Prop();
				ent.SetModel( RandomModel );
				ent.Transform = Transform;
				this.Weld( ent );
				ent.LocalPosition = base.Position + PositionOffset;
				ent.LocalRotation = Rotation.From( this.Rotation.Angles() + RotationOffset );
			}
		}
	}

}

