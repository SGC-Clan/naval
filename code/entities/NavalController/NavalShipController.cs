using Sandbox;
using System;
using System.Collections.Generic;


[Library( "nvl_ship_controller", Title = "Ship Controller", Spawnable = true )]
public partial class NavalShipController : Prop, IUse
{
	[ConVar.Replicated( "nvl_debug" )]
	public static bool nvl_debug { get; set; } = false;

	[Net]
	public float MovementSpeed { get; private set; } //for camera?
	[Net]
	public float ShipThorttle { get; private set; }
	[Net]
	public float ShipMaxThorttle { get; private set; }

	//TEMP Hardcoded Waterlevel TO:DO for the love of god make it not hardcoded
	public float WaterSurfaceHeight = 0;

	private ModelEntity vehicle_detail_controllstick;

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

		var modelName = "models/citizen_props/chair03.vmdl";

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

		Seat.PhysicsBody.Mass = 800;
		Seat.PhysicsGroup.SetSurface( "wood" ); //temp buoyeancy

	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		//{
		//	var vehicle_detail_controllstick = new ModelEntity();
		//	vehicle_detail_controllstick.SetModel( "entities/modular_vehicle/chassis_transmission.vmdl" );
		//	vehicle_detail_controllstick.Transform = Transform;
		//	vehicle_detail_controllstick.Parent = this;
		//	vehicle_detail_controllstick.LocalPosition = new Vector3( 0.75f, 0, 0 ) * 40.0f;
		//	clientModels.Add( vehicle_detail_controllstick );
		//}

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
		if ( !IsServer )
			return;

		var body = PhysicsBody;
		if ( !body.IsValid() )
			return;

		if ( body.WaterLevel>0 ) //If it at least touches water a lil bit
		{
			ApplyBouyancyToShip();

			ShipMaxThorttle = 600f;

			//Thortl
			if ( currentInput.throttle != 0 )
			{
				//Modify thorttle
				ShipThorttle = Math.Clamp( ShipThorttle += currentInput.throttle, (ShipMaxThorttle/3) * -1, ShipMaxThorttle );
			} 
			else 
			{
				//slowly decrease thorttle 
				ShipThorttle = Math.Clamp( ShipThorttle -= 1* Math.Sign(ShipThorttle), (ShipMaxThorttle / 3) * -1, ShipMaxThorttle );
			}

			//Apply forward movement
			if ( ShipThorttle != 0) 
			{
				body.ApplyForceAt( body.MassCenter, body.Rotation.Forward * body.Mass * (ShipThorttle * currentInput.throttle) );
			}

			if ( nvl_debug )
				DebugOverlay.Text( Position + Transform.NormalToWorld( new Vector3(100,0,10) ), "Thorttle:\n" + ShipThorttle.ToString()+"/"+ShipThorttle.ToString() );

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

	}

	[Event.Frame]
	public void OnFrame()
	{

		//float frontOffset = 20.0f - Math.Min( -10, 10 );

		//RaycastPhysics();

		//vehicle_detail_controllstick.SetBoneTransform( "Transmission_Axle_Aim", new Transform( Vector3.Up * 70 ), false );
		//chassis_axle_rear.SetBoneTransform( "Axle_Rear_Center", new Transform( Vector3.Up * backOffset ), false );

	}

	public void ApplyBouyancyToShip() 
	{
		//We will only apply bouyeancy to props in the water
		foreach ( Prop ShipProp in this.childrenProps )
		{
			var body = ShipProp.PhysicsBody;
			if ( !body.IsValid() )
				continue;
			var bounds = body.GetBounds();
			var level = WaterSurfaceHeight.LerpInverse( bounds.Mins.z, bounds.Maxs.z, true );
			var bouyancy = (body.Density - 1000).LerpInverse( 0.0f, -300f );
			float depth = ((WaterSurfaceHeight - bounds.Maxs.z) / 100.0f).Clamp( 1.0f, 10.0f );
			body.ApplyForceAt( ShipProp.WaterLevel.WaterSurface, (Vector3.Up * bounds.Volume * level * bouyancy * 0.5f * depth) );
		}
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
