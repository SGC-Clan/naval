using Sandbox;
using naval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace naval.teams
{
	public class PiratesTeam : BaseTeam
	{
		public override string TeamName => "Pirates";
		public override Color TeamColor => Color.FromBytes( 30, 30, 30 );

	}
}
