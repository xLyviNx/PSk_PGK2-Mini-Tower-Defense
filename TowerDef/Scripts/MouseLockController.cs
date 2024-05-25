using PGK2.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.TowerDef.Scripts
{
	public class MouseLockController : Component
	{
		public static bool HoldingCamera;

		public static bool isLocked
		{
			get
			{
				if (HoldingCamera) return true;
				return false;
			}
		}

		public override void Update()
		{
			base.Update();
			Mouse.IsLocked = isLocked;
		}
	}
}
