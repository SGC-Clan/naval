using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public partial class NavalProp : Prop
	{
		[Net] public int Health { get; set; }

		public NavalProp() 
		{
		
		}

		public override void Spawn()
		{
			base.Spawn();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
		}

		public override void Simulate( IClient cl )
		{
			base.Simulate( cl );
		}
	}
}
