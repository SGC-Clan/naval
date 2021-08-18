using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.Tools
{
	[Library( "tool_naval_randomshipgenerator", Title = "Random Ship Generator (Naval)", Description = "Create randomly generated ships", Group = "construction2" )]
	public partial class NavalRandomShipGenerator : BaseTool
	{
		public float Seed = 124182748;
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{

				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					CreateShip();
				}

				if ( Input.Pressed( InputButton.Attack2 ) )
				{
					
				}

			}
		}


		public void CreateShip()
		{

			Entity SpawnEnt( Vector3 entPos, Angles entRot, string entModel, string entClass )
			{
				var ent = Library.Create<Entity>( entClass );
				ent.Position = entPos;
				ent.Rotation = Rotation.From( entRot );
				(ent as ModelEntity)?.SetModel( entModel );

				return ent;
			}

			var startPos = Owner.EyePos;
			var dir = Owner.EyeRot.Forward;

			var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				.Ignore( Owner )
				.HitLayer( CollisionLayer.Debris )
				.Run();
			var SpawnPos = tr.EndPos + new Vector3( 0, 0, 50 );

			CreateHitEffects( SpawnPos );

			SpawnEnt( SpawnPos, Owner.WorldAng, "models/rust_vehicles/rowboat/rowboat.vmdl", "Sandbox.Prop" );

		}
	}	
}
