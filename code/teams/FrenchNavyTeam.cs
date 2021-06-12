using Sandbox;
using naval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace naval.teams
{
	public class FrenchNavyTeam : BaseTeam
	{
		public override string TeamName => "French Navy";
		public override Color TeamColor => Color.FromBytes( 150, 20, 20 );

	}
}
