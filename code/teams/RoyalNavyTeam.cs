using Sandbox;
using naval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace naval.teams
{
	public class RoyalNavyTeam : BaseTeam
	{
		public override string TeamName => "Royal Navy";
		public override Color TeamColor => Color.FromBytes( 20, 20, 150 );

	}
}
