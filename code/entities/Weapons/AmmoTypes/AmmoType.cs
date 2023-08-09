using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	//Truth is - its more like turrets stats than only ammo, but I named it like that in early 2017 and I dont wanna fix it okay?
	public struct AmmoType
	{
		//===|| Turret specific stats:

		public string NiceName;
		
		public string Description;	

		public string Tags;

		public string Author;

		public string LogoImage;

		public int Price;

		public string TurretModel;

		//basic damage
		public float Damage;

		//randomise base damage
		public float DamageRandomnes;

		public float DamageMultVsShips;
		public float DamageMultVsSubmarines;
		public float DamageMultVsPlanes;
		public float DamageMultVsAirships;

		//force applied to the gun when firing
		public float Recoil;

		public float Inaccuracy;

		//How much time it takes to reload
		public float ReloadTime;

		//how much times we can shoot (with firerate as delay) without reloading
		public int ClipSize;

		//how much time between the shoots from the clip
		public float Firerate;

		public float LaunchSpeed;

		public float MaxLifeTime;

		public string Decal;

		//  ======TO:DO======

		//after this distance projectile will trajectory will start to move around due to randomly decreased speed and external forces
		//public float effectiveRange;

		//public float Drag;

		//public float Gravity;

		//public float Mass;

		//public float PenetrationCoefficient;

		//public float DeflectionCoefficient;

		// Standard = using fake physics with penetration systems
		// Physics = yeeting source2 physics objects at great distances
		// Hitscan = like laser with limited ranges
		// Flame = simple sticky fluid simulation
		// Spawner = spawns another selected entity
		//public string simulationTechnique;
	}
}
