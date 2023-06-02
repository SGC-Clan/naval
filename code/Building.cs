using Sandbox.Internal;
using Sandbox.UI;
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

namespace Sandbox
{

	public partial class Building : ModelEntity
	{

		[Net] public float sizeX { get; set; }
		[Net] public float sizeY { get; set; }
		[Net] public float sizeZ { get; set; }

		[Net] public float uvScale { get; set; }

		public override void Spawn()
		{

			if ( Game.IsServer )
			{
				base.Spawn();
			}

			SetModel( "models/dev/box.vmdl" );

			GenerateModel();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			GenerateModel();
		}


		public async void GenerateModel() 
		{
			await GameTask.Delay( 50 );

			var bounds = new BBox();

			Vector3[] vertices = new Vector3[]
			{
				//Top face
				new Vector3(-sizeX , -sizeY , sizeZ ),   // Vertex 0
				new Vector3(sizeX , -sizeY , sizeZ ),    // Vertex 1
				new Vector3(sizeX , sizeY , sizeZ ),     // Vertex 2
				new Vector3(-sizeX , sizeY , sizeZ ),    // Vertex 3

				//Bottom face
				new Vector3(sizeX , -sizeY , -sizeZ ),   // Vertex 4
				new Vector3(-sizeX , -sizeY , -sizeZ ),  // Vertex 5
				new Vector3(-sizeX , sizeY , -sizeZ ),   // Vertex 6
				new Vector3(sizeX , sizeY , -sizeZ ),    // Vertex 7

				//Right face
				new Vector3(sizeX , -sizeY , sizeZ ),    // Vertex 8
				new Vector3(sizeX , -sizeY , -sizeZ ),   // Vertex 9
				new Vector3(sizeX , sizeY , -sizeZ ),    // Vertex 10
				new Vector3(sizeX , sizeY , sizeZ ),     // Vertex 11

				//Left face
				new Vector3(-sizeX , -sizeY , -sizeZ ),  // Vertex 12
				new Vector3(-sizeX , -sizeY , sizeZ ),   // Vertex 13
				new Vector3(-sizeX , sizeY , sizeZ ),    // Vertex 14
				new Vector3(-sizeX , sizeY , -sizeZ ),   // Vertex 15

				//Front face
				new Vector3(-sizeX , sizeY , sizeZ ),    // Vertex 16
				new Vector3(sizeX , sizeY , sizeZ ),     // Vertex 17
				new Vector3(sizeX , sizeY , -sizeZ ),    // Vertex 18
				new Vector3(-sizeX , sizeY , -sizeZ ),   // Vertex 19

				//Back face
				new Vector3(-sizeX , -sizeY , -sizeZ ),  // Vertex 20
				new Vector3(sizeX , -sizeY , -sizeZ ),   // Vertex 21
				new Vector3(sizeX , -sizeY , sizeZ ),    // Vertex 22
				new Vector3(-sizeX , -sizeY , sizeZ ),    // Vertex 23

			};

			int[] indices = new int[]
			{
				// Top face
				1, 2, 0,
				3, 0, 2,

				// Bottom face
				4, 5, 6,
				6, 7, 4,

				// Right face
				8, 9, 10,
				10, 11, 8,

				// Left face
				13, 14, 12,
				15, 12, 14,

				// Front face
				16,17,18,
				18,19,16,

				// Back face
				23, 20, 22,
				21, 22, 20,

			};

			//TO:DO make it work pls ?
			//Proportions for uvs
			float uvScale2 = uvScale * 500f;
			float pX = sizeX / uvScale2; //sizeX / (200 - (100 * uvScale));
			float pY = sizeY / uvScale2; //sizeY / (200 - (100 * uvScale));
			float pZ = sizeZ / uvScale2; //sizeZ / (200 - (100 * uvScale));
			//Log.Info( "-------" );
			//Log.Info( "pX="+ pX );
			//Log.Info( "pY=" + pY );
			//Log.Info( "pZ=" + pZ );

			Vector2[] uvs1 = new Vector2[]
			{
				// top face
				new Vector2(pX, pY),
				new Vector2(0f, pY),
				new Vector2(0f, 0f),
				new Vector2(pX, 0f),

				//Bottom face
				new Vector2(0f, pY),
				new Vector2(pX, pY),
				new Vector2(pX, 0f),
				new Vector2(0f, 0f),

				// Right face
				new Vector2(0f, pZ),
				new Vector2(pY, pZ),
				new Vector2(pY, 0f),
				new Vector2(0f, 0f),

				// Left face
				new Vector2(pX, pZ),
				new Vector2(0f, pZ),
				new Vector2(0f, 0f),
				new Vector2(pX, 0f),

				// Front face
				new Vector2(0f, 0f),
				new Vector2(pY, 0f),
				new Vector2(pY, pZ),
				new Vector2(0f, pZ),

				// Back face
				new Vector2(pY, pZ),
				new Vector2(0f, pZ),
				new Vector2(0f, 0f),
				new Vector2(pY, 0f),

			};

			List<Vertex> verticesData = new List<Vertex>();
			for ( int i = 0; i < vertices.Length; i++ )
			{
				Vector4 uv = new Vector4( uvs1[i].x, uvs1[i].y, 1, 1 );
				Vertex vert = new Vertex( vertices[i], Vector3.Up, Vector3.Forward, uv );
				verticesData.Add( vert );
				bounds = bounds.AddPoint( vertices[i] );

			}


			//build the model
			var modelBuilder = new ModelBuilder();

			//create mesh - client only
			if ( true )
			{
				Mesh mesh = new Mesh( Material.Load( "materials/concrete_floors_4k_basecolor.vmat" ) ); //"materials/generic/floor_tile_gravel_outdoor.vmat"
				mesh.CreateVertexBuffer( verticesData.Count, Vertex.Layout, verticesData );
				mesh.CreateIndexBuffer( indices.Length, indices );
				mesh.Bounds = bounds;
				modelBuilder.AddMesh( mesh );
			}
			modelBuilder.WithSurface( "concrete" );
			modelBuilder.AddCollisionMesh( vertices, indices );
			var model = modelBuilder.Create();

			this.Model = model;
			this.SetupPhysicsFromModel( PhysicsMotionType.Static );
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );
		}

	}
}
