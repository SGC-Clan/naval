using Sandbox.Internal;
using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Sandbox.ParticleSnapshot;
using static Sandbox.Terrain;
using static Sandbox.Utility.Easing;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sandbox
{

	public partial class Terrain : AnimatedEntity
	{

		//TerrainEnt.GenerateHeightMapTerrain(seed, 8, terrainSize, terrainSize, (int) TerrainEnt.Position.x / spacing, (int) TerrainEnt.Position.y / spacing );
		//TerrainEnt.GenerateTerrain( 7000, spacing, 0.3f );
		//TerrainEnt.SetMaterialOverride( "models_and_materials/terrain/procedural_ocean_bottom1.vmat" );

		//public void GenerateHeightMapTerrain( int seed, int octaves, int width, int height, int offsetW, int offsetH )
		//float scale, float spacing, float uvScale 

		//Height Map settings:
		[Net] public string HeightMapType { get; set; }
		[Net] public float seed { get; set; }
		[Net] public int octaves { get; set; }
		[Net] public int width { get; set; }
		[Net] public int height { get; set; }
		[Net] public int offsetW { get; set; }
		[Net] public int offsetH { get; set; }
		[Net] public float islandRadius { get; set; }
		[Net] public float islandScale { get; set; }
		[Net] public string islandHeightmapEasings { get; set; } //https://easings.net/


		//Terrain mesh settings:
		[Net] public float scale { get; set; }
		[Net] public float spacing { get; set; }
		[Net] public float uvScale { get; set; }
		[Net] public string material { get; set; }


		public float[,] heightMap;

		//public Vector3[] terrainGrid { get; set; }

		public override void Spawn()
		{

			if ( Game.IsServer ) 
			{
				base.Spawn();
			}
				
			SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );

			GenerateTerrain();

		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			GenerateTerrain();
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );
		}

		public async void GenerateTerrain()
		{
			await GameTask.Delay( 50 );

			if ( HeightMapType == "island" )
			{
				GenerateHeightMapIsland( islandHeightmapEasings );
			}
			else
			{
				GenerateHeightMapTerrain();
			}

			//Threading
			//await GameTask.RunInThreadAsync( () => 
			//{
				//Log.Info("Task is running on client? "+Game.IsClient);

				int width = heightMap.GetLength( 0 );
				int height = heightMap.GetLength( 1 );

				int numVerts = width * height;
				Vector3[] vertices = new Vector3[numVerts];
				Vector3[] normals = new Vector3[numVerts];
				Vector4[] uvs = new Vector4[numVerts];
				Vector3[] tangents = new Vector3[numVerts];
				//Vector3[] terrainGrid = new Vector3[numVerts];
				int TriangleNext = 0;
				Vector3 TriangleNormal = new Vector3();

				var bounds = new BBox();
				var verticesData = new List<Vertex>();

				// Generate vertices based on heightmap
				int index = 0;
				for ( int y = 0; y < height; y++ )
				{
					for ( int x = 0; x < width; x++ )
					{
						float heightValue = heightMap[x, y] * scale;
						Vector3 vertPos = new Vector3( (x * spacing) - spacing, (y * spacing) - spacing, heightValue );
						vertices[index] = vertPos;

						//terrainGrid[index] = vertPos;

						bounds = bounds.AddPoint( vertPos );


						Vector3 normal = new Vector3();
						//normals by neighbors 
						normal = CalculateNeighborNormal( x, y, width, height, vertices );
						normals[y * width + x] = normal;

						//those additional points on the height map are only for correct normal maping
						//lets try to hide them (and for the love of god find a better solution)
						if ( x == 0 || y == 0 )
						{
							vertPos = vertPos + new Vector3( 0, 0, -5 );
							vertices[index] = vertPos;
						}

						//normals by triangles
						//if ( TriangleNext == 2 )
						//{
						//	TriangleNext = 0;

						//	TriangleNormal = CalculateTriangleNormal( vertices, index );
						//	normals[y * width + x] = TriangleNormal;
						//	normals[y * width + x - 1] = TriangleNormal;
						//	normals[y * width + x - 2] = TriangleNormal;
						//}
						//else { TriangleNext++; }
						//normal = TriangleNormal;




						float u = x * uvScale;
						float v = y * uvScale;
						Vector4 uv = new Vector4( (float)x / (width - 1) - u, (float)y / (height - 1) - v, heightValue, 1.0f );
						uvs[y * width + x] = uv;

						//no idea what im doing - I need to do more research http://intcomputergraphics.blogspot.com/2013/04/tangent-space-normal-mapping.html
						Vector3 tangent = new Vector3( 0, 0, 1 );

						//var tempAng = normal.EulerAngles;
						//var tempColor = Color.Red;
						//if ( x == 0 ) { tempColor = Color.Blue; }
						//DebugArrow( vertPos, Rotation.From( tempAng ), 1f, tempColor );

						verticesData.Add( new Vertex( vertPos, normal, tangent, uv ) );

						//if ( Game.IsServer ) {
						//	DebugOverlay.Text( Math.Round( heightMap[x, y], 3 ).ToString(), vertPos + this.Position, 0, Color.Blue, 100000, 1500 );
						//}

						index++;
					}
				}

				// Generate indices for triangle strips
				int[] indices = new int[(width - 1) * (height - 1) * 6];
				index = 0;
				for ( int y = 0; y < height - 1; y++ )
				{
					for ( int x = 0; x < width - 1; x++ )
					{
						int topLeft = y * width + x;
						int topRight = topLeft + 1;
						int bottomLeft = (y + 1) * width + x;
						int bottomRight = bottomLeft + 1;

						//indices[index++] = topLeft;
						//indices[index++] = bottomLeft;
						//indices[index++] = topRight;

						//indices[index++] = topRight;
						//indices[index++] = bottomLeft;
						//indices[index++] = bottomRight;

						indices[index++] = topRight;
						indices[index++] = bottomLeft;
						indices[index++] = topLeft;

						indices[index++] = bottomRight;
						indices[index++] = bottomLeft;
						indices[index++] = topRight;
					}
				}

				//build the model
				var ModelBuilder = new ModelBuilder();

				//create mesh - client only
				if ( Game.IsClient ) {
					Mesh mesh = new Mesh( Material.Load( material ) );
					mesh.CreateVertexBuffer( vertices.Length, Vertex.Layout, verticesData );
					mesh.CreateIndexBuffer( indices.Length, indices );
					mesh.Bounds = bounds;
					ModelBuilder.AddMesh( mesh );
				}

				ModelBuilder.WithSurface( "dirt" );
				ModelBuilder.AddCollisionMesh( vertices, indices );
				var TerrainModel = ModelBuilder.Create();
				this.Model = TerrainModel;
				this.SetupPhysicsFromModel( PhysicsMotionType.Static );
	
				if ( Game.IsServer )
				{
					
					this.Scale = 1f;
				}

			//} );
		}


		public void GenerateHeightMapTerrain()
		{
			int offset = 2; //we need additional vertices on all sides
			int w = width + offset;
			int h = height + offset;
			heightMap = new float[w, h];
			var seedOffset = seed;

			for ( int y = 0; y < h; y++ )
			{
				for ( int x = 0; x < w; x++ )
				{
					heightMap[y, x] = Noise.Fbm( octaves, x + offsetH + seedOffset, y + offsetW + seedOffset ); //offsetW and offsetH has to be switched around dont ask me why
				}
			}

		}

		public void GenerateHeightMapIsland( string heightmapEasings )
		{
			heightMap = new float[width, height];
			var seedOffset = seed;

			// Calculate the center position of the heightmap
			float centerX = width / 2f;
			float centerY = height / 2f;

			// Generate height values using Perlin noise
			for ( int y = 0; y < height; y++ )
			{
				for ( int x = 0; x < width; x++ )
				{

					float sampleX = x / islandScale;
					float sampleY = y / islandScale;

					// Normalize the coordinates to the range [-1, 1]
					float normalizedX = (sampleX - centerX) / centerX;
					float normalizedY = (sampleY - centerY) / centerY;

					float distanceFromCenter = MathF.Sqrt( normalizedX * normalizedX + normalizedY * normalizedY );

					// Use Perlin noise to generate the height value
					float heightValue = Noise.Fbm( octaves, x + offsetW + seedOffset, y + offsetH + seedOffset );


					//add heightmap easings - based on those examples https://easings.net/
					//if ( heightmapeasings.contains( "easeinoutcirc" ) ) 
					//{
					//	heightvalue = easeinoutcirc( heightvalue );
					//}
					//if ( heightmapeasings.contains( "easeoutcubic" ) )
					//{
					//	heightvalue = easeoutcubic( heightvalue );
					//}

					//make island beaches more flat
					//TO:DO

					////cllapse land that hits -0.1 value
					//if ( heightValue < -0.10f )
					//{
					//	heightValue *= 1.1f;
					//}

					// Apply island shape based on the distance from the center
					float islandFactor = SmoothStep( 0f, islandRadius, distanceFromCenter );
					heightValue = Math.Clamp( heightValue - islandFactor, -1, 1 );

					float newHeightValue = 0;
					//make beaches more flat etc
					if ( heightValue < 0 )
					{
						//easings.net/#easeOutCirc
						newHeightValue = 1 - MathF.Sqrt( 1 - MathF.Pow( Math.Abs(1 - heightValue) - 1, 2 ) );
						newHeightValue *= -1;
					}
					else
					{
						newHeightValue = -(MathF.Cos( MathF.PI * heightValue ) - 1) / 2;

						//easings.net/#easeInOutQuint
						//if ( heightValue < 0.5 )
						//{ newHeightValue = 16 * heightValue * heightValue * heightValue * heightValue * heightValue; }
						//else
						//{ newHeightValue = 1 - MathF.Pow( -2 * heightValue + 2, 5 ) / 2; }

						//newHeightValue = MathF.Sqrt( 1 - MathF.Pow( heightValue - 1, 2 ) );

					}

					// Set the height value in the heightmap
					
					heightMap[x, y] = newHeightValue;
				}
			}
		}

		public static float SmoothStep( float from, float to, float t )
		{
			t = Math.Clamp( t, 0, 1 );
			t = t * t * (3f - 2f * t);
			return MathX.Lerp( from, to, t );
		}
		public float EasingFunction( float x )
		{
			float k = 10f; // Controls the rate of change
			float value = 1f / (1f + MathF.Exp( -k * (x - 0.5f) ));
			return value;
		}

		public static float easeInOutCirc( float x ) {
			//easings.net/#easeInOutCirc
			if ( x < 0.5 ) {
			return (1 - MathF.Sqrt( 1 - MathF.Pow( 2 * x, 2 ) )) / 2;
			} else {
			return (MathF.Sqrt( 1 - MathF.Pow( -2 * x + 2, 2 ) ) + 1) / 2;
			}
		}
		public static float easeInCirc( float x ) {
			//easings.net/#easeInCirc
			return 1 - MathF.Sqrt(1 - MathF.Pow(x, 2));
		}
		public static float easeOutCubic( float x ) {
			//easings.net/#easeOutCubic
			return 1 - MathF.Pow(1 - x, 3);
		}
		public static float easeInOutSine( float x ) {
			return -(MathF.Cos( MathF.PI* x) - 1) / 2;
		}

		//calculation based on neighbor vectors
		private Vector3 CalculateNeighborNormal( int x, int y, int width, int height, Vector3[] vertices )
		{
			Vector3 position = vertices[y * width + x];

			// Find adjacent vertices
			Vector3 left = (x > 0) ? vertices[y * width + x - 1] : position;
			Vector3 right = (x < width - 1) ? vertices[y * width + x + 1] : position;
			Vector3 top = (y > 0) ? vertices[(y - 1) * width + x] : position;
			Vector3 bottom = (y < height - 1) ? vertices[(y + 1) * width + x] : position;

			// Calculate edge vectors
			Vector3 leftEdge = position - left;
			Vector3 rightEdge = right - position;
			Vector3 topEdge = position - top;
			Vector3 bottomEdge = bottom - position;

			// Calculate cross product of adjacent edges
			Vector3 normal1 = Vector3.Cross( leftEdge, topEdge );
			Vector3 normal2 = Vector3.Cross( rightEdge, bottomEdge );

			// Calculate average normal
			Vector3 normal = (normal1 + normal2).Normal;

			return normal;
		}

		//calculation based on triangles
		private Vector3 CalculateTriangleNormal( Vector3[] vertices, int index ) 
		{
			Vector3 p1 = vertices[index];
			Vector3 p2 = vertices[index-1];
			Vector3 p3 = vertices[index-2];

			Vector3 U = p2 - p1;
			Vector3 V = p3 - p1;

			Vector3 normal = new Vector3();

			normal.x = U.y * V.z - U.z * V.y;
			normal.y = U.z * V.x - U.x * V.z;
			normal.z = U.x * V.y - U.y * V.x;

			return normal;
		}

		private void DebugArrow( Vector3 pos, Rotation rot, float scale, Color color )
		{
			if ( Game.IsClient ) { return; }

			var debugArrow = new ModelEntity
			{
				Position = pos,
				Rotation = rot,
				Model = Model.Load( "models/arrow.vmdl" ),
				Scale = scale,
				RenderColor = color,
			};
			debugArrow.SetMaterialOverride( Material.Load( "materials/default/white.vmat" ) );
			//DebugOverlay.Text( "test", pos, 0, Color.Blue, 10, 1500 );
		}

	}
}
