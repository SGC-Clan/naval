using Sandbox.ModelEditor.Nodes;
using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Sandbox.CitizenAnimationHelper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sandbox
{
	public partial class World: Entity
	{

		[ConVar.Replicated]
		static int proc_gen_seed { get; set; }

		public World()
		{
		}

		public override void Spawn()
		{
			base.Spawn();

			WorldCreation( proc_gen_seed );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			//would be cool to have this server controlled but light dissapears when you dont look at its entity..
			//so its here for now only on client

			var sceneWorld = Game.SceneWorld;

			sceneWorld.AmbientLightColor = new Color { r = 195 / 255, g = 221 / 255, b = 117 / 255 }; //Color.Black;
			sceneWorld.ClearColor = Color.Black;

			//Sky sky = new Sky()
			//{
			//	Skyname = "models_and_materials/cubemap/mirrored_skybox.vmat",
			//	TintColor = Color.White,
			//	FogType = SceneSkyBox.FogType.Distance,
			//	FogMinStart = -25.0f,
			//	FogMinEnd = -35.0f,
			//	FogMaxStart = 25.0f,
			//	FogMaxEnd = 35.0f,

			//};

			//underwater gradient fog
			var gradientFog = new GradientFogController();
			gradientFog.StartDistance = 60000;
			gradientFog.EndDistance = 100000;
			gradientFog.StartHeight = -5000;
			gradientFog.EndHeight = 5000;
			gradientFog.DistanceFalloffExponent = 0.5f;
			gradientFog.Enabled = true;
			gradientFog.Color = Color.White; //new Color( 208/255, 221/255, 244/255 );
			gradientFog.MaximumOpacity = 0.8f;
			sceneWorld.GradientFog = gradientFog;

			//the sun!
			float SunStrenght = 50f;
			var Sun = new SceneSunLight( sceneWorld, Rotation.From( 45, 90, -90 ), new Color { r = 66 / SunStrenght, g = 148 / SunStrenght, b = 209 / SunStrenght } );
			Sun.ShadowsEnabled = true;
			Sun.SkyColor = Color.White * 0.4f;
			Sun.ShadowTextureResolution = 1024;

			//EnvironmentLightEntity Light = new EnvironmentLightEntity();
			//Light.Position = new Vector3( 0, 0, 5000 );
			//Light.Brightness = 1;
			//float SunStrenght = 1f;
			//Light.Color = new Color( 66 / SunStrenght, 148 / SunStrenght, 209 / SunStrenght );
			//Light.SkyColor = Color.White * 0.4f;
			//Light.SkyIntensity = 0.8f;
			//Light.Rotation = Rotation.From( new Angles( 45, 90, -90 ) );
			//Light.AmbientColor = new Color { r = 195 / 255, g = 221 / 255, b = 117 / 255 };

			//cube map
			var cubeMap = new SceneCubemap( sceneWorld, Texture.Load( FileSystem.Mounted, "models_and_materials/cubemap/mirrored_skybox.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 150000 ) ); //"models_and_materials/cubemap/env_cubemap_16.vtex"  "textures/cubemaps/default.vtex"

			//Sky box
			var skyBox = new SceneSkyBox( sceneWorld, Material.Load( "models_and_materials/cubemap/mirrored_skybox.vmat" ) );
			skyBox.SetSkyLighting( new Vector3( 20, 10, -90 ) );

			//var camera = Camera.Main;
			//camera.World = sceneWorld;


		}

		//[GameEvent.Client.Frame]
		//public void UpdateLight() 
		//Make light related entities always visible 
		//no matter how many million of hammer units away we are
		//{
		//	foreach ( Entity ent in Entity.All )
		//	{
			
		//	}
		//}

		public override void Simulate(IClient cl)
		{
			base.Simulate(cl);
		}

		//Its a miracle - imagine doing it in hammer instead and waiting 32 hours for vrad to throw an error.. I need to upgrade my cpu ;-;
		public static async void WorldCreation( int seed )
		{

			await GameTask.Delay( 1000 );

			//if ( Game.IsClient ) { return; } //TO:DO proper server/client side delegation on terrain.cs

			Log.Info( "creating world.. " + seed );


			//temp island platform
			//var platform = new ModelEntity
			//{
			//	Position = new Vector3( 0, 0, -25 ),
			//	Rotation = new Rotation(),
			//	Model = Model.Load( "models_and_materials/terrain/simple_island1.vmdl" ), //( "models/sbox_props/watermelon/watermelon.vmdl" );
			//	Scale = 1f,
			//};
			//platform.SetupPhysicsFromModel( PhysicsMotionType.Static );

			//pelican isle
			var isle = new ModelEntity
			{
				Position = new Vector3( 0, 0, -25 ),
				Rotation = new Rotation(),
				Model = Model.Load( "models_and_materials/harbors/pelican_isle.vmdl" ),
				Scale = 1f,
			};
			isle.SetupPhysicsFromModel( PhysicsMotionType.Static );

			//Island generation
			GenerateIslands( seed, 2, 100*500 );

			//Ocean Bottom generation
			GenerateOceanBottom( seed, 4 ); //6
			
			GenerateOcean( 1 );

			//Respawn everyone
			await GameTask.Delay( 2500 );
			var player = All.OfType<Player>();
			foreach ( var pl in player )
			{
				pl.Respawn();
			}

		}



		public static void GenerateOceanBottom( int seed, int TilesAmount )
		{

			int terrainSize = 100;
			int spacing = 500; //distance between squares mesh is made off
			int offset = terrainSize * spacing;
			int TileHeight = -4600;
			int TileCenter = offset / 2;
			Vector3 StartPos = new Vector3( offset * (TilesAmount / 2), offset * (TilesAmount / 2), TileHeight );

			for ( int y = 0; y < TilesAmount; y++ )
			{
				for ( int x = 0; x < TilesAmount; x++ )
				{
					//ocean bottom tile
					Vector3 pos = StartPos + new Vector3( -offset * (y + 1), -offset * (x + 1), 0 );
					var TerrainEnt = new Terrain()
					{
						Position = pos,
						Rotation = Rotation.From( new Angles( 0, 0, 0 ) ),
						//height map settings
						seed = seed,
						octaves = 8,
						width = terrainSize,
						height = terrainSize,
						offsetW = (int)pos.x / spacing,
						offsetH = (int)pos.y / spacing,
						HeightMapType = "terrain",
						//terrain mesh settings
						scale = 7000,
						spacing = spacing,
						uvScale = 0.8f,//0.3f, 
						material = "models_and_materials/terrain/procedural_ocean_bottom1.vmat",
					};
					//make other systems recognise this as ocean bottom
					TerrainEnt.Tags.Add( "oceanbottom" );
					TerrainEnt.Tags.Add( "solid" );

					OceanBottomTilePlaceObjects( TerrainEnt, pos, offset, offset, 50 ); //TerrainEnt.terrainGrid,

				}
			}

		}


		public static void GenerateOcean( int TilesAmount )
		{
			if ( Game.IsClient ) return;

			int TileSize = 5000;
			float TileScale = 40;
			float offset = TileSize * TileScale;
			int waterHeight = 500;
			int TileCenter = TileSize / 2;
			Vector3 StartPos = new Vector3();
			if ( TilesAmount > 1) 
			{
				StartPos = new Vector3( offset * (TilesAmount / 2), offset * (TilesAmount / 2), 0 );
			} else {
				StartPos = new Vector3( offset/2, offset/2, 0 );
			}

			//water volume
			var waterBoundingBox = new BBox( new Vector3( -TileSize, -TileSize, -waterHeight ), new Vector3( TileSize, TileSize, 0 ) );
			var waterMesh = new Mesh
			{
				//Material = Material.Load( "materials/physics/water.vmat" ),
				PrimitiveType = MeshPrimitiveType.Triangles,
			};
			var waterMeshVB = new VertexBuffer();
			waterMeshVB.AddCube( new Vector3( 0, 0, -waterHeight ), new Vector3( (TileSize*2)+1, (TileSize*2)+1, waterHeight ), new Rotation(), default );
			waterMesh.CreateBuffers( waterMeshVB, true );
			var waterMeshModelBuilder = new ModelBuilder();
			waterMeshModelBuilder.AddMesh( waterMesh );
			waterMeshModelBuilder.AddCollisionBox( new Vector3( TileSize, TileSize, waterHeight/2 ), new Vector3( 0, 0, -waterHeight/2 ) );
			var waterMeshModel = waterMeshModelBuilder.Create();

			for ( int y = 0; y < TilesAmount; y++ )
			{
				for ( int x = 0; x < TilesAmount; x++ )
				{

					// Water tile
					var water = new NavalWater
					{
						EnableReflection = true,
						EnableRipples = true,
						Model = waterMeshModel,
						Position = StartPos + new Vector3( -offset * (y + 1), -offset * (x + 1), 0 ) - new Vector3( (y+1) * 400, (x+1) * 400, 0 ),
						EnableAllCollisions = true,
						Scale = TileScale,
					};
					water.SetupPhysicsFromModel( PhysicsMotionType.Static );
					water.CollisionBounds = waterBoundingBox;

					//water.SetupPhysicsFromAABB( PhysicsMotionType.Static, new Vector3( -TileSize * TileScale, -TileSize * TileScale, -waterHeight ), new Vector3( TileSize * TileScale, TileSize * TileScale, 0 ) );
				
				}
			}
		}


		public async static void GenerateIslands( int seed, int islandsAmount, float spreadArena )
		{
			var rand = new Random();

			int terrainSize = 200;
			int spacing = 100; //distance between squares mesh is made off
			int sizeMax = terrainSize * spacing;
			int IslandHeight = 0;
			int IslandCenter = sizeMax / 2;

			for ( int x = 0; x < islandsAmount; x++ )
			{

				float islandScale = 1f; //SMALLER number means bigger 
				float islandRadius = rand.Float( 0.7f, 1.7f ); //1.3f

				Vector3 pos = new Vector3( rand.Float( -spreadArena, spreadArena ), rand.Float( -spreadArena, spreadArena ), 0 );

				var TerrainEnt = new Terrain()
				{
					Position = pos,
					Rotation = Rotation.From( new Angles( 0, 0, 0 ) ),
					//height map settings
					seed = seed,
					octaves = 3,
					width = terrainSize,
					height = terrainSize,
					offsetW = (int)pos.x / spacing,
					offsetH = (int)pos.y / spacing,
					HeightMapType = "island",
					islandRadius = islandRadius,
					islandScale = islandScale,
					//terrain mesh settings
					scale = 2500,
					spacing = spacing,
					uvScale = 0.8f,
					material = "models_and_materials/terrain/procedural_ground_mat1.vmat",
				};
				//make other systems recognise this as island
				TerrainEnt.Tags.Add( "island" );
				TerrainEnt.Tags.Add( "solid" );

				await GameTask.Delay( 250 );

				//Generate entities on the surface of the island
				var islandCenterPos = TerrainEnt.Position + new Vector3( IslandCenter, IslandCenter, 0 );

				//generate harbors and buildings
				IslandPopulateWithBuildings( islandCenterPos, sizeMax * islandRadius );

				//generate overhangs and cliffs
				IslandPlaceCliffsOverhangs( islandCenterPos, sizeMax * islandRadius, rand.Next(8,16), 0.9f, 1.1f, new string[] 
				{
				"models_and_materials/terrain/overhang_platform_01.vmdl",
				} );


				//Rocks
				IslandPopulateWithObjects( islandCenterPos, sizeMax * islandRadius, 15, 100, new string[] {
				"models_and_materials/rocks/moss rock 08.vmdl",
				"models_and_materials/rocks/moss rock 09.vmdl",
				"models_and_materials/rocks/moss rock 10.vmdl",
				"models_and_materials/rocks/moss rock 11.vmdl",
				"models_and_materials/rocks/moss rock 12.vmdl",
				"models_and_materials/rocks/moss rock 13.vmdl",
				"models_and_materials/rocks/moss rock 14.vmdl",
				},3,5, true );

				//Trees
				IslandPopulateWithObjects( islandCenterPos, sizeMax * islandRadius, 20, 200, new string[] {
				"models/sbox_props/trees/oak/tree_oak_big_a.vmdl",
				"models/sbox_props/trees/oak/tree_oak_big_b.vmdl",
				"models/sbox_props/trees/oak/tree_oak_medium_a.vmdl",
				"models/sbox_props/trees/oak/tree_oak_small_a.vmdl",
				},0.8f,1.2f, false );

				//Shrubs
				IslandPopulateWithObjects( islandCenterPos, sizeMax * islandRadius, 180, 400, new string[] {
				"models/sbox_props/shrubs/beech/beech_bush_large.vmdl",
				"models/sbox_props/shrubs/beech/beech_bush_medium.vmdl",
				"models/sbox_props/shrubs/beech/beech_bush_regular_medium_a.vmdl",
				"models/sbox_props/shrubs/beech/beech_bush_regular_medium_b.vmdl",
				"models/sbox_props/shrubs/beech/beech_bush_small.vmdl",
				"models/sbox_props/shrubs/beech/beech_shrub_tall_large.vmdl",
				"models/sbox_props/shrubs/beech/beech_shrub_tall_medium.vmdl",
				"models/sbox_props/shrubs/beech/beech_shrub_tall_small.vmdl",
				"models/sbox_props/shrubs/beech/beech_shrub_wide_large.vmdl",
				"models/sbox_props/shrubs/beech/beech_shrub_wide_medium.vmdl",
				"models/sbox_props/shrubs/beech/beech_shrub_wide_small.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_low_b.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_low_a.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_regular_a.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_regular_b.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_regular_c.vmdl",
				"models/sbox_props/shrubs/pine/pine_shrub_tall_a.vmdl",
				"models/sbox_props/shrubs/pine/pine_shrub_tall_b.vmdl",
				"models/sbox_props/shrubs/pine/pine_shrub_wide.vmdl",
				},1,2, false );

				//Cactail
				IslandPopulateWithObjects( islandCenterPos, sizeMax * islandRadius, 80, 300, new string[] {
				"models/rust_nature/reeds/reeds_medium.vmdl",
				"models/rust_nature/reeds/reeds_small.vmdl",
				"models/rust_nature/reeds/reeds_small_dry.vmdl",
				"models/rust_nature/reeds/reeds_small_sparse.vmdl",
				"models/rust_nature/reeds/reeds_tall.vmdl",
				"models/rust_nature/reeds/reeds_tall_dry.vmdl",
				},1,1.4f, false );

			}

		}

		public static void IslandPopulateWithBuildings( Vector3 IslandCenter, float islandSize )
		{
			int maxAttempts = 40;
			int amountOfEnts = 2;

			var rand = Game.Random;

			for ( int x = 0; x < maxAttempts; x++ )
			{
				//rotate around island close to the water and place concrete platforms


				ModelEntity placeDecorations( Building Parent, Vector3 offset, Angles ang, float scale, string model )
				{
					var decoration = new ModelEntity()
					{
						Scale = scale,
						Model = Model.Load( model ),
					};
					decoration.SetupPhysicsFromModel( PhysicsMotionType.Static );
					decoration.Position = Parent.Transform.TransformVector( offset );
					decoration.Rotation = Parent.Transform.RotationToWorld( ang.ToRotation() );

					return decoration;
				}

				var height = 20;

				Vector3 randomNormal = new Vector3( MathF.Cos( rand.Next() * 3.1415f ), MathF.Sin( rand.Next() * 3.1415f ), 0 );

				float moveDistance = rand.Float( 0f, 8000f );

				Vector3 creatorPos = randomNormal * moveDistance;
				Rotation rot = randomNormal.EulerAngles.ToRotation();
				creatorPos = new Vector3( creatorPos.x, creatorPos.y, height ) + IslandCenter;

				var tr = Trace.Ray( creatorPos, new Vector3( IslandCenter.x, IslandCenter.y, height ) )
					.UseHitboxes()
					//.Ignore( ent )
					.Run();

				if ( tr.Hit && tr.Entity.Tags.Has( "island" ) )
				{
					Vector3 pos = tr.HitPosition + new Vector3( 0, 0, -1000 + 100 );

					if ( amountOfEnts == 1 )
					{

						var MainBuilding = new Building()
						{
							Position = pos,
							Rotation = rot,
							sizeX = 1500,
							sizeY = 1200,
							sizeZ = 1000,
							uvScale = 0.5f,
						};
						MainBuilding.SetupPhysicsFromModel( PhysicsMotionType.Static );

						//Player Spawnpoints
						var spawnpoint1 = new SpawnPoint()
						{
							Parent = MainBuilding,
							Position = MainBuilding.Transform.TransformVector( new Vector3( 1300, 0, 1080 ) ),
							Rotation = MainBuilding.Transform.RotationToWorld( new Rotation() ),
						};

						//Flag test
						var flag = new AnimatedMapEntity()
						{
							Parent = MainBuilding,
							Position = MainBuilding.Transform.TransformVector( new Vector3( 1400, 0, 1000 ) ),
							//Rotation = MainBuilding.Transform.RotationToWorld( new Rotation() ),  its a flag, it should be rotated towards the wind lol
							Rotation = new Rotation(),
							Model = Model.Load( "models_and_materials/harbors/pirate_flag.vmdl" ),
						};
						flag?.SetAnimation( "Idle" );
						flag.SetMaterialGroup( rand.Next( 1, 2 ) ); //0-pirate, 1-british, 2-french

						//main harbor building
						placeDecorations( MainBuilding, new Vector3( 660, 0, 1000 ), new Angles( 0, 90, 0 ), 0.65f, "models_and_materials/buildings/niebrowice_station/niebrowice_station.vmdl" ); //"models/source1/harbor_shop_building01.vmdl"

						//Sandbags
						placeDecorations( MainBuilding, new Vector3( 200, 1120, 1000 ), new Angles( 0, 60 + 90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );
						placeDecorations( MainBuilding, new Vector3( 200, -1120, 1000 ), new Angles( 0, -60 + -90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );

						placeDecorations( MainBuilding, new Vector3( 320, 1170, 1000 ), new Angles( 0, 90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );
						placeDecorations( MainBuilding, new Vector3( 320, -1170, 1000 ), new Angles( 0, -90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );

						placeDecorations( MainBuilding, new Vector3( 600, 1170, 1000 ), new Angles( 0, 90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );
						placeDecorations( MainBuilding, new Vector3( 600, -1170, 1000 ), new Angles( 0, -90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );

						placeDecorations( MainBuilding, new Vector3( 900, 1170, 1000 ), new Angles( 0, 90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );
						placeDecorations( MainBuilding, new Vector3( 900, -1170, 1000 ), new Angles( 0, -90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );

						placeDecorations( MainBuilding, new Vector3( 1200, 1170, 1000 ), new Angles( 0, 90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );
						placeDecorations( MainBuilding, new Vector3( 1200, -1170, 1000 ), new Angles( 0, -90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );

						placeDecorations( MainBuilding, new Vector3( 1350, 1170, 1000 ), new Angles( 0, 90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );
						placeDecorations( MainBuilding, new Vector3( 1350, -1170, 1000 ), new Angles( 0, -90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );

						placeDecorations( MainBuilding, new Vector3( 1450, 1120, 1000 ), new Angles( 0, -60 + 90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );
						placeDecorations( MainBuilding, new Vector3( 1450, -1120, 1000 ), new Angles( 0, 60 + -90, 0 ), 1.2f, "models/rust_props/barricades/barricade.sandbags.vmdl" );


						//Crane
						placeDecorations( MainBuilding, new Vector3( 2000, -600, 990 ), new Angles( 0, 60 + -90, 0 ), 1.2f, "models_and_materials/harbors/crane_small/crane_small.vmdl" );

						var building1 = new Building()
						{
							Position = pos,
							Rotation = rot,
							sizeX = 800,
							sizeY = 300,
							sizeZ = 990,
							uvScale = 0.5f,
						};
						building1.Position = MainBuilding.Transform.TransformVector( new Vector3( 2000, 800, 0 ) );
						building1.SetupPhysicsFromModel( PhysicsMotionType.Static );

						placeDecorations( building1, new Vector3( 600, 270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );
						placeDecorations( building1, new Vector3( 600, -270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );

						placeDecorations( building1, new Vector3( 400, 270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );
						placeDecorations( building1, new Vector3( 400, -270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );

						var building2 = new Building()
						{
							Position = pos,
							Rotation = rot,
							sizeX = 800,
							sizeY = 300,
							sizeZ = 990,
							uvScale = 0.5f,
						};
						building2.Position = MainBuilding.Transform.TransformVector( new Vector3( 2000, -800, 0 ) );
						building2.SetupPhysicsFromModel( PhysicsMotionType.Static );

						placeDecorations( building2, new Vector3( 600, 270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );
						placeDecorations( building2, new Vector3( 600, -270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );

						placeDecorations( building2, new Vector3( 400, 270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );
						placeDecorations( building2, new Vector3( 400, -270, 990 ), new Angles( 0, 0, 0 ), 1.5f, "models/sbox_props/concrete_bollard/concrete_bollard.vmdl" );
					
					}
					else 
					{
						//covered dock
						var MainBuilding = new ModelEntity()
						{
							Position = pos + new Vector3( 0,0,850 ),
							Rotation = rot,
							Model = Model.Load( "models_and_materials/harbors/covered_dock_01.vmdl" ),
						};
						MainBuilding.SetupPhysicsFromModel( PhysicsMotionType.Static );

					}

					amountOfEnts--;

					if ( amountOfEnts == 0 ) { break; }
				}


			}

		}


		public static void IslandPopulateWithObjects( Vector3 IslandCenter, float islandSize, int amountOfEnts, int maxAttempts, string[] models, float sizeMin, float sizeMax, bool useNormal ) 
		{
			//we will rotate around island center and decide what to place depending on height and beam hit 

			var rand = Game.Random;

			for ( int x = 0; x < maxAttempts; x++ )
			{

				Vector3 randomNormal = new Vector3( MathF.Cos( rand.Next() * 3.1415f ), MathF.Sin( rand.Next() * 3.1415f ), 0 );

				float moveDistance = rand.Float( 0f, 4000f );

				Vector3 creatorPos = randomNormal * moveDistance;
				creatorPos = new Vector3( creatorPos.x, creatorPos.y, 0 ) + IslandCenter;

				var tr = Trace.Ray( new Vector3( creatorPos.x, creatorPos.y, 15000 ), new Vector3( creatorPos.x, creatorPos.y, 10 ) )
					.UseHitboxes()
					//.Ignore( ent )
					.Run();

				if ( tr.Hit && tr.Entity.Tags.Has( "island" ) )
				{
					float noPlaceZoneDistance = 1500f;
					bool canPlace = true;
					foreach ( var building in All.OfType<Building>() )
					{
						if ( building.Position.Distance( tr.HitPosition ) < noPlaceZoneDistance ) 
						{
							canPlace = false;
						}
					}
					if ( canPlace == false ) { continue; }

					Rotation traceNormal = new Rotation(); 
					if ( useNormal ) { traceNormal = tr.Normal.EulerAngles.ToRotation(); }

					var vegetation = new ModelEntity
					{
						Position = tr.HitPosition + new Vector3( 0, 0, 0 ),
						Rotation = randomNormal.EulerAngles.ToRotation() + traceNormal,
						Model = Model.Load( models[rand.Next( 0, models.Length )] ),
						Scale = rand.Float( sizeMin, sizeMax ),
					};
					vegetation.SetupPhysicsFromModel( PhysicsMotionType.Static );

					amountOfEnts--;

					if ( amountOfEnts == 0 ) { break; }
				}


			}

		}


		public static void IslandPlaceCliffsOverhangs( Vector3 IslandCenter, float islandSize, int amount, float sizeMin, float sizeMax, string[] models ) 
		{
			//we will rotate around island center and decide what to place depending on height and beam hit 

			var rand = Game.Random;
			int currentAmount = amount;
			int maxAttempts = amount * 10;

			for ( int x = 0; x < maxAttempts; x++ )
			{

				Vector3 randomNormal = new Vector3( MathF.Cos( rand.Next() * 3.1415f ), MathF.Sin( rand.Next() * 3.1415f ), 0 );

				float moveDistance = rand.Float( 0f, 4000f );

				Vector3 creatorPos = randomNormal * moveDistance;
				creatorPos = new Vector3( creatorPos.x, creatorPos.y, 0 ) + IslandCenter;

				var tr = Trace.Ray( new Vector3( creatorPos.x, creatorPos.y, 15000 ), new Vector3( creatorPos.x, creatorPos.y, 100 ) )
					.UseHitboxes()
					//.Ignore( ent )
					.Run();

				if ( tr.Hit && tr.Entity.GetType().Name == "Terrain" ) //tr.Entity.Tags.Has( "island" )
				{

					var cliff = new ModelEntity
					{
						Position = tr.HitPosition + new Vector3( 0, 0, 0 ),
						Rotation = randomNormal.EulerAngles.ToRotation(),
						Model = Model.Load( models[rand.Next( 0, models.Length )] ),
						Scale = rand.Float( sizeMin, sizeMax ),
					};
					cliff.SetupPhysicsFromModel( PhysicsMotionType.Static );
					//this is also a part of island now
					cliff.Tags.Add( "island" );
					cliff.Tags.Add( "solid" );

					currentAmount--;

					if ( currentAmount <= 0 ) { break; }
				}


			}
		}


		public static async void OceanBottomTilePlaceObjects( Terrain terrain, Vector3 pos, float width, float height, int maxEnts ) //, Vector3[] Normals <- TO:DO
		{
			await GameTask.Delay( 1000 );


			//We will place gigantic rocks, bushes and pirate bases

			var rand = new Random();
			int tries = maxEnts * 50;
			int amount = 0;

			Vector3[] PotentialSpecialBases = new Vector3[tries];

			//chaos
			//Vector3[] shuffled = positions.OrderBy( n => Guid.NewGuid() ).ToArray();


			//Lets start by trying to place rocks 
			//we only want them on shallow water
			float rockMaxHeight = 50;
			float rockMinHeight = -150;
			float vegetationMinHeight = 50;
			float baseMinHeight = 100;

			float minDistanceBases = 25000f;

			string[] RockModels = new string[] {
				"models_and_materials/rocks/moss rock 08.vmdl",
				"models_and_materials/rocks/moss rock 09.vmdl",
				"models_and_materials/rocks/moss rock 10.vmdl",
				"models_and_materials/rocks/moss rock 11.vmdl",
				"models_and_materials/rocks/moss rock 12.vmdl",
				"models_and_materials/rocks/moss rock 13.vmdl",
				"models_and_materials/rocks/moss rock 14.vmdl",
			};
			string[] BushModels = new string[] { 
				"models/sbox_props/shrubs/pine/pine_bush_low_b.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_low_a.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_regular_a.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_regular_b.vmdl",
				"models/sbox_props/shrubs/pine/pine_bush_regular_c.vmdl",
				"models/sbox_props/shrubs/pine/pine_shrub_tall_a.vmdl",
				"models/sbox_props/shrubs/pine/pine_shrub_tall_b.vmdl",
				"models/sbox_props/shrubs/pine/pine_shrub_wide.vmdl",
			};

			for ( int x=0; x < tries; x++ )
			{

				Vector3 newPos = pos + new Vector3( rand.Float(0,width), rand.Float( 0, height ) );
				var rotation = new Angles().ToRotation();
				float heightOffset = 20f;
				float scale = 1;

				var tr = Trace.Ray( new Vector3( newPos.x, newPos.y, 600 ), new Vector3( newPos.x, newPos.y, -600 ) )
					.UseHitboxes()
					//.Ignore( water )
					.WithoutTags( new string[] {"water","island"} )
					.Run();

				if ( tr.Hit && tr.Entity == terrain )
				{
					float hitH = tr.HitPosition.z;

					//decide if we should place a base here
					if ( hitH > baseMinHeight )
					{

						bool canPlace = true;
						var otherBuildings = All.OfType<Building>();
						foreach ( var ent in otherBuildings )
						{
							if ( ent.Position.Distance( tr.HitPosition ) < minDistanceBases )
								canPlace = false;
						}

						if ( canPlace ) 
						{
							var mainBuilding = new Building()
							{
								Position = tr.HitPosition + new Vector3( 0, 0, -450 ),
								Rotation = new Angles( 0, rand.Float( -180, 180 ), 0 ).ToRotation(),
								sizeX = 500,
								sizeY = 500,
								sizeZ = 500,
								uvScale = 0.2f,
							};
							mainBuilding.SetupPhysicsFromModel( PhysicsMotionType.Static );

							//Flag test
							var flag = new AnimatedMapEntity()
							{
								Parent = mainBuilding,
								Position = mainBuilding.Transform.TransformVector( new Vector3( mainBuilding.sizeX / 4, mainBuilding.sizeY / 4, 0 ) ),
								Rotation = mainBuilding.Transform.RotationToWorld( new Angles().ToRotation() ),
								Model = Model.Load( "models_and_materials/harbors/pirate_flag.vmdl" ),
							};
							flag?.SetAnimation( "Idle" );
							flag.SetMaterialGroup( 0 ); //0-pirate, 1-british, 2-french

							//TO:DO clean this garbage up please - move everything to buildings folder and put this shit in separate classes I beg you

							var building2 = new Building()
							{
								Position = mainBuilding.Position,
								Rotation = mainBuilding.Rotation,
								sizeX = 550,
								sizeY = 550,
								sizeZ = 450,
								uvScale = 0.2f,
							};
							building2.SetupPhysicsFromModel( PhysicsMotionType.Static );

							//pirate tent
							var tent = new ModelEntity()
							{
								Position = mainBuilding.Transform.TransformVector( new Vector3( mainBuilding.sizeX / 4, mainBuilding.sizeY / 4, 500 ) ),
								Rotation = mainBuilding.Transform.RotationToWorld( new Angles().ToRotation() ),
								Scale = 1.25f,
								Model = Model.Load( "models_and_materials/buildings/tents/pirate_stretch_tent01.vmdl" ),
							};
							tent.SetupPhysicsFromModel( PhysicsMotionType.Static );

							Log.Info("Pirate base placed at: "+ flag.Position);

							amount++;
							continue;

						}

					}

					// decide what type of model we should place
					int type = 0; //kelp
					string[] models = BushModels;//TO:DO add kelp!
					rotation = new Angles( 0, rand.Float( -180, 180 ), 0 ).ToRotation();

					if ( hitH <= rockMaxHeight || hitH > rockMinHeight )
					{
						type = 2; //rock
						models = RockModels;
						rotation = new Angles( rand.Float( -180, 180 ), rand.Float( -180, 180 ), rand.Float( -180, 180 ) ).ToRotation();
						heightOffset = 20;
						scale = rand.Float( 4, 6 );
					};

					if ( hitH > vegetationMinHeight ) 
					{
						type = 1; //vegetation
						models = BushModels;
						rotation = new Angles( 0, rand.Float( -180, 180 ), 0 ).ToRotation();
						heightOffset = -30;
						scale = rand.Float( 3, 5 );
					}

					if ( type == 0 ) continue; //TO:DO add kelp!

					var decoration = new ModelEntity()
					{
						Scale = scale,
						Model = Model.Load( models[rand.Next( 0, models.Length )] ),
						Position = tr.HitPosition + new Vector3( 0, 0, heightOffset ),
						Rotation = Angles.Random.ToRotation()
					};
					decoration.SetupPhysicsFromModel( PhysicsMotionType.Static );
					Rotation rot = rotation;
					decoration.Rotation = rot;

					amount++;
				}

				if ( amount >= maxEnts ) { break; }
			}
		}



		public static void DebugArrow( Vector3 pos, float scale, Color color )
		{
			if ( Game.IsClient ) { return; }

			var debugArrow = new ModelEntity
			{
				Position = pos,
				Rotation = new Rotation(),
				Model = Model.Load( "models/arrow.vmdl" ),
				Scale = scale,
				RenderColor = color,
			};
			debugArrow.SetMaterialOverride( Material.Load( "materials/default/white.vmat" ) );
			//DebugOverlay.Text( "test", pos, 0, Color.Blue, 10, 1500 );
		}



		//its an apocalypse!
		[ConCmd.Admin( "recreate_world" )]
		public static async void WorldDestruction()
		{
			var rand = Game.Random;


			var terrain = All.OfType<Terrain>();
			foreach ( var ent in terrain )
			{
				ent.Delete();
			}

			var modelEntity = All.OfType<ModelEntity>();
			foreach ( var ent in modelEntity)
			{
				if ( ent.GetType() == typeof( Pawn ) ) { continue; }

				ent.Delete();
			}

			var buildings = All.OfType<Building>();
			foreach ( var ent in buildings )
			{
				ent.Delete();
			}

			var water = All.OfType<Water>();
			foreach ( var ent in water )
			{
				ent.Delete();
			}

			var sun = All.OfType<SceneSunLight>();
			foreach ( var ent in sun )
			{
				ent.Delete();
			}

			await GameTask.Delay( 5000 );

			WorldCreation( proc_gen_seed );

		}


	}

}
