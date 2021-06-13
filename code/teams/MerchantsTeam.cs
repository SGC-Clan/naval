using Sandbox;
using naval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace naval.teams
{
	public class MerchantsTeam : BaseTeam
	{
		public override string TeamName => "Merchants";
		public override Color TeamColor => Color.FromBytes( 255, 127, 0 );

	}
}
