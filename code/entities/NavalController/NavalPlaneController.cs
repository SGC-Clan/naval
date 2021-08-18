using Sandbox;
using System;
using System.Collections.Generic;


[Library( "nvl_plane_controller", Title = "Plane Controller", Spawnable = true )]
public partial class NavalPlaneController : Prop, IUse
{
	[ConVar.Replicated( "nvl_debug" )]
	public static bool nvl_debug { get; set; } = false;

	[Net]
	public float MovementSpeed { get; private set; } //for camera?
	[Net]
	public float PlaneThorttle { get; private set; }
	[Net]
	public float PlaneMaxThorttle { get; private set; }

	public float PlanePropRotation = 0f;

	//TEMP Hardcoded Waterlevel TO:DO for the love of god make it not hardcoded
	public float WaterSurfaceHeight = 0;

	public ModelEntity plane_engine_prop;

	public Prop plane_wing_left;
	public Prop plane_wing_right;
	public Prop plane_tail_left;
	public Prop plane_tail_right;
	public Prop plane_vertical_stabiliser;

	private struct InputState
	{
		public float throttle;
		public float turning;
		public float breaking;
		public float tilt;
		public float roll;

		public void Reset()
		{
			throttle = 0;
			turning = 0;
			breaking = 0;
			tilt = 0;
			roll = 0;
		}
	}

	private InputState currentInput;

	private Player driver;

	private readonly List<ModelEntity> clientModels = new();

	public override void Spawn()
	{
		base.Spawn();

		var modelName = "models/citizen_props/chair01.vmdl";

		SetModel( modelName );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		SetInteractsExclude( CollisionLayer.Player );

		var Seat = new ModelEntity
		{
			Parent = this,
			Position = Position,
			Rotation = Rotation,
			EnableTouch = true,
			CollisionGroup = CollisionGroup.Trigger,
			Transmit = TransmitType.Never
		};

		Seat.SetModel( modelName );
		Seat.SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );

		Seat.PhysicsBody.Mass = 30;
		Seat.PhysicsGroup.SetSurface( "wood" ); //temp buoyeancy

		//physical wheels
		//SpawnPlaneWheel( new Vector3(10,4,-1), new Angles(0,0,90), "models/citizen_props/wheel01.vmdl", 0.5f, 100 );

		//physical entities of plane like wings
		SpawnPlaneParts();
	}

	public void SpawnPlaneWheel( Vector3 offset, Angles angle, string model, float size, float mass ) 
	{
		var wheel = new WheelEntity
		{
			Position = Transform.PointToWorld( offset ),
			Rotation = Rotation.From( angle ),
			Scale = size,
	};
		wheel.SetModel( model ); // "models/citizen_props/wheel01.vmdl"
		wheel.PhysicsBody.Mass = mass;
		wheel.Joint = PhysicsJoint.Revolute
			.From( wheel.PhysicsBody )
			.To( this.PhysicsBody )
			.WithPivot( Transform.PointToWorld( offset ) )
			.WithBasis( Rotation.From( angle ) )
			.Create();
	}

	//plane parts with physics
	public void SpawnPlaneParts() 
	{

		// Wings
		{
			this.plane_wing_left = new Prop();
			plane_wing_left.SetModel( "models/citizen_props/newspaper01.vmdl" );
			plane_wing_left.Transform = Transform;
			this.Weld( plane_wing_left );
			plane_wing_left.LocalPosition = new Vector3( -5, 110, 15 );
			plane_wing_left.LocalRotation = Rotation.From( new Angles( 0, 0, 0 ) );
		}

		{
			this.plane_wing_right = new Prop();
			plane_wing_right.SetModel( "models/citizen_props/newspaper01.vmdl" );
			plane_wing_right.Transform = Transform;
			this.Weld( plane_wing_right );
			plane_wing_right.LocalPosition = new Vector3( -5, -110, 15 );
			plane_wing_right.LocalRotation = Rotation.From( new Angles( 0, 0, 0 ) );
		}

		{
			this.plane_tail_left = new Prop();
			plane_tail_left.SetModel( "models/citizen_props/newspaper01.vmdl" );
			plane_tail_left.Transform = Transform;
			this.Weld( plane_tail_left );
			plane_tail_left.LocalPosition = new Vector3( -125, 25, 25 );
			plane_tail_left.LocalRotation = Rotation.From( new Angles( 0, 0, 0 ) );
		}

		{
			this.plane_tail_right = new Prop();
			plane_tail_right.SetModel( "models/citizen_props/newspaper01.vmdl" );
			plane_tail_right.Transform = Transform;
			this.Weld( plane_tail_right );
			plane_tail_right.LocalPosition = new Vector3( -125, -25, 25 );
			plane_tail_right.LocalRotation = Rotation.From( new Angles( 0, 0, 0 ) );
		}

		{
			this.plane_vertical_stabiliser = new Prop();
			plane_vertical_stabiliser.SetModel( "models/citizen_props/newspaper01.vmdl" );
			plane_vertical_stabiliser.Transform = Transform;
			this.Weld( plane_vertical_stabiliser );
			plane_vertical_stabiliser.LocalPosition = new Vector3( -125, 0, 25+20 );
			plane_vertical_stabiliser.LocalRotation = Rotation.From( new Angles( 90, 90, 0 ) );
		}

		//Plane parts
		{
			var engine_cowl = new Prop();
			engine_cowl.SetModel( "models/courier/cowl1a.vmdl" );
			engine_cowl.Transform = Transform;
			this.Weld( engine_cowl );
			engine_cowl.LocalPosition = new Vector3( 40, 0, 15 );
			engine_cowl.LocalRotation = Rotation.From( new Angles( 0, 0, 0 ) );
		}

		{
			var engine_cowl2 = new Prop();
			engine_cowl2.SetModel( "models/courier/cowl1a.vmdl" );
			engine_cowl2.Transform = Transform;
			this.Weld( engine_cowl2 );
			engine_cowl2.LocalPosition = new Vector3( -30, 0, 15 );
			engine_cowl2.LocalRotation = Rotation.From( new Angles( 0, 180, 0 ) );
		}

		{
			var cockpit_floor = new Prop();
			cockpit_floor.SetModel( "models/courier/barbette4s.vmdl" );
			cockpit_floor.Transform = Transform;
			this.Weld( cockpit_floor );
			cockpit_floor.LocalPosition = new Vector3( 0, 0, -5 );
			cockpit_floor.LocalRotation = Rotation.From( new Angles( 0, 180, 0 ) );
			cockpit_floor.LocalScale = 0.3f;
		}

		{
			var fuselage = new Prop();
			fuselage.SetModel( "models/courier/fuselage1a.vmdl" );
			fuselage.Transform = Transform;
			this.Weld( fuselage );
			fuselage.LocalPosition = new Vector3( -30, 0, 15 );
			fuselage.LocalRotation = Rotation.From( new Angles( -90, 0, 0 ) );
		}
	}

	//non physical details
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		{
			this.plane_engine_prop = new ModelEntity();
			plane_engine_prop.SetModel( "models/courier/radialrotary.vmdl" );
			plane_engine_prop.Transform = Transform;
			plane_engine_prop.Parent = this;
			plane_engine_prop.LocalPosition = new Vector3( 40, 0, 15 );
			plane_engine_prop.LocalRotation = Rotation.From( new Angles(0,0,0) );
			clientModels.Add( plane_engine_prop );
		}

	}

	private void RemoveDriver( NavalPlayer player )
	{
		driver = null;
		player.Vehicle = null;
		player.VehicleController = null;
		player.VehicleCamera = null;
		player.PhysicsBody.Enabled = true;

		ResetInput();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( driver is NavalPlayer player )
		{
			RemoveDriver( player );
		}

		foreach ( var model in clientModels )
		{
			model?.Delete();
		}

		clientModels.Clear();
	}

	public void ResetInput()
	{
		currentInput.Reset();
	}

	[Event.Tick.Server]
	protected void Tick()
	{
		if ( driver is NavalPlayer player )
		{
			if ( player.LifeState != LifeState.Alive || player.Vehicle != this )
			{
				RemoveDriver( player );
			}
		}
	}

	public override void Simulate( Client owner )
	{
		if ( owner == null ) return;
		if ( !IsServer ) return;

		using ( Prediction.Off() )
		{
			currentInput.Reset();

			if ( Input.Pressed( InputButton.Use ) )
			{
				if ( owner.Pawn is NavalPlayer player )
				{
					RemoveDriver( player );

					return;
				}
			}

			currentInput.throttle = (Input.Down( InputButton.Forward ) ? 1 : 0) + (Input.Down( InputButton.Back ) ? -1 : 0);
			currentInput.turning = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
			currentInput.breaking = (Input.Down( InputButton.Jump ) ? 1 : 0);
			currentInput.tilt = (Input.Down( InputButton.Run ) ? 1 : 0) + (Input.Down( InputButton.Duck ) ? -1 : 0);
			currentInput.roll = (Input.Down( InputButton.Left ) ? 1 : 0) + (Input.Down( InputButton.Right ) ? -1 : 0);
		}
	}

	[Event.Physics.PreStep]
	public void OnPrePhysicsStep()
	{
		//client side rotation
		if ( !IsServer ) 
		{
			PlanePropRotation += PlaneThorttle;
			plane_engine_prop.Rotation = Transform.RotationToWorld( Rotation.From( 0, 0, PlanePropRotation/7.5f ) );
		}

		if ( !IsServer )
		return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;

		PlaneMaxThorttle = 600f;

		//Thortl
		if ( currentInput.throttle != 0 )
		{
			//Modify thorttle
			PlaneThorttle = Math.Clamp( PlaneThorttle += currentInput.throttle, (PlaneMaxThorttle/3) * -1, PlaneMaxThorttle );
		} 
		else 
		{
			//slowly decrease thorttle 
			PlaneThorttle = Math.Clamp( PlaneThorttle -= 1* Math.Sign(PlaneThorttle), (PlaneMaxThorttle / 3) * -1, PlaneMaxThorttle );
		}

		//Apply forward movement
		if ( PlaneThorttle != 0) 
		{
			body.ApplyForceAt( body.MassCenter, body.Rotation.Forward * body.Mass * (PlaneThorttle * currentInput.throttle) );
		}

		//if ( nvl_debug )
			DebugOverlay.Text( Position + Transform.NormalToWorld( new Vector3(100,0,10) ), "Thorttle:\n" + PlaneThorttle.ToString()+"/"+ PlaneMaxThorttle.ToString() );

		//Turning
		if ( currentInput.turning != 0 )
		{
			Vector3 Force = body.Rotation.Left * body.Mass * (50 * currentInput.turning);
			Vector3 ForcePos = Transform.PointToWorld( new Vector3( 100, 0, 0 ) );
			Vector3 ForcePosAtWater = new Vector3( ForcePos.x, ForcePos.y, WaterSurfaceHeight + 20 );
			body.ApplyForceAt( ForcePosAtWater, Force );

			if ( nvl_debug )
			{
				DebugOverlay.Line( Position, ForcePosAtWater );
				DebugOverlay.Line( ForcePosAtWater, ForcePosAtWater + Force );
			}

		}

	}

	[Event.Frame]
	public void OnFrame()
	{

		//float frontOffset = 20.0f - Math.Min( -10, 10 );

		ApplyFinPhysics( plane_wing_left );
		ApplyFinPhysics( plane_wing_right );
		ApplyFinPhysics( plane_tail_left );
		ApplyFinPhysics( plane_tail_right );
		ApplyFinPhysics( plane_vertical_stabiliser );

	//vehicle_detail_controllstick.SetBoneTransform( "Transmission_Axle_Aim", new Transform( Vector3.Up * 70 ), false );
	//chassis_axle_rear.SetBoneTransform( "Axle_Rear_Center", new Transform( Vector3.Up * backOffset ), false );

}

	public void ApplyFinPhysics( Prop entity ) 
	{
		if ( !IsServer )
			return;

		var PhysBody = entity.PhysicsBody;
		if ( !PhysBody.IsValid() )
			return;

		float efficiency = 1000f;

		var curvel = PhysBody.Velocity;
		var curup = entity.Transform.Rotation.Forward;

		Vector3 vec1 = curvel;
		Vector3 vec2 = curup;
		vec1 = vec1 - 2 * (Vector3.Dot( vec1, vec2 )) * vec2;
		float sped = vec1.Length;

		Vector3 finalvec = curvel;
		float modf = Math.Abs( Vector3.Dot( curup, curvel.Normal ) );
		float nvec = Vector3.Dot( curup, curvel.Normal );

		if ( nvec > 0 )
		{
			vec1 = vec1 + (curup * 10);
		}
		else
		{
			vec1 = vec1 + (curup * -10);
		}
		finalvec = vec1.Normal * (float)(Math.Pow( sped, modf ) - 1f);
		finalvec = finalvec.Normal;
		finalvec = (finalvec * efficiency) + curvel;

		//lift by plane
		float liftmul = 1 - Math.Abs( nvec );
		finalvec = finalvec + (curup * liftmul * curvel.Length * efficiency) / 700;

		finalvec = finalvec.Normal;
		finalvec = finalvec * curvel.Length;


		PhysBody.Velocity = finalvec;
	}

	public bool OnUse( Entity user )
	{
		if ( user is NavalPlayer player && player.Vehicle == null )
		{
			player.Vehicle = this;
			player.VehicleController = new VehicleController();
			player.VehicleCamera = new VehicleCamera();
			player.PhysicsBody.Enabled = false;
			driver = player;
		}

		return true;
	}

	public bool IsUsable( Entity user )
	{
		return driver == null;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !IsServer )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;

		if ( other is NavalPlayer player && player.Vehicle == null )
		{
			var speed = body.Velocity.Length;
			var forceOrigin = Position + Rotation.Down * Rand.Float( 20, 30 );
			var velocity = (player.Position - forceOrigin).Normal * speed;
			var angularVelocity = body.AngularVelocity;

			OnPhysicsCollision( new CollisionEventData
			{
				Entity = player,
				Pos = player.Position + Vector3.Up * 50,
				Velocity = velocity,
				PreVelocity = velocity,
				PostVelocity = velocity,
				PreAngularVelocity = angularVelocity,
				Speed = speed,
			} );
		}
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( !IsServer )
			return;

		if ( eventData.Entity is NavalPlayer player && player.Vehicle != null )
		{
			return;
		}

		var propData = GetModelPropData();

		var minImpactSpeed = propData.MinImpactDamageSpeed;
		if ( minImpactSpeed <= 0.0f ) minImpactSpeed = 500;

		var impactDmg = propData.ImpactDamage;
		if ( impactDmg <= 0.0f ) impactDmg = 10;

		var speed = eventData.Speed;

		if ( speed > minImpactSpeed )
		{
			if ( eventData.Entity.IsValid() && eventData.Entity != this )
			{
				var damage = speed / minImpactSpeed * impactDmg * 1.2f;
				eventData.Entity.TakeDamage( DamageInfo.Generic( damage )
					.WithFlag( DamageFlags.PhysicsImpact )
					.WithFlag( DamageFlags.Vehicle )
					.WithAttacker( driver != null ? driver : this, driver != null ? this : null )
					.WithPosition( eventData.Pos )
					.WithForce( eventData.PreVelocity ) );
			}
		}
	}
}
