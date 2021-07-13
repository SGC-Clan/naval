using Sandbox;
using System;

struct CarWheel
{
	private readonly CarEntity _parent;

	private float _previousLength;
	private float _currentLength;

	public CarWheel( CarEntity parent )
	{
		_parent = parent;
		_previousLength = 0;
		_currentLength = 0;
	}

	public bool Raycast( float length, bool doPhysics, Vector3 offset, ref float wheel, float dt )
	{
		var position = _parent.Position;
		var rotation = _parent.Rotation;

		var wheelAttachPos = position + offset;
		var wheelExtend = wheelAttachPos - rotation.Up * (length * _parent.Scale);

		var tr = Trace.Ray( wheelAttachPos, wheelExtend )
			.Ignore( _parent )
			.Run();

		wheel = length * tr.Fraction;
		var wheelRadius = (14 * _parent.Scale);

		if ( !doPhysics && CarEntity.debug_car )
		{
			var wheelPosition = tr.Hit ? tr.EndPos : wheelExtend;
			wheelPosition += rotation.Up * wheelRadius;

			if ( tr.Hit )
			{
				DebugOverlay.Circle( wheelPosition, rotation * Rotation.FromYaw( 90 ), wheelRadius, Color.Red.WithAlpha( 0.5f ), false );
				DebugOverlay.Line( tr.StartPos, tr.EndPos, Color.Red, 0, false );
			}
			else
			{
				DebugOverlay.Circle( wheelPosition, rotation * Rotation.FromYaw( 90 ), wheelRadius, Color.Green.WithAlpha( 0.5f ), false );
				DebugOverlay.Line( wheelAttachPos, wheelExtend, Color.Green, 0, false );
			}
		}

		if ( !tr.Hit || !doPhysics )
		{
			return tr.Hit;
		}

		var body = _parent.PhysicsBody;

		_previousLength = _currentLength;
		_currentLength = (length * _parent.Scale) - tr.Distance;

		var springVelocity = (_currentLength - _previousLength) / dt;
		var springForce = body.Mass * 50.0f * _currentLength;
		var damperForce = body.Mass * (1.5f + (1.0f - tr.Fraction) * 3.0f) * springVelocity;
		var velocity = body.GetVelocityAtPoint( wheelAttachPos );
		var speed = velocity.Length;
		var speedDot = MathF.Abs( speed ) > 0.0f ? MathF.Abs( MathF.Min( Vector3.Dot( velocity, rotation.Up.Normal ) / speed, 0.0f ) ) : 0.0f;
		var speedAlongNormal = speedDot * speed;
		var correctionMultiplier = (1.0f - tr.Fraction) * (speedAlongNormal / 1000.0f);
		var correctionForce = correctionMultiplier * 50.0f * speedAlongNormal / dt;

		body.ApplyImpulseAt( wheelAttachPos, tr.Normal * (springForce + damperForce + correctionForce) * dt );

		return true;
	}
}
