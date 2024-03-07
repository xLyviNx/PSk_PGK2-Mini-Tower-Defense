using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Engine
{
	public sealed class Engine
	{
		public static Engine? Instance { get; private set; }
		public EngineWindow? window;

		public static void CreateInstance()
		{
			if (Instance == null) 
			{
				Instance = new Engine();
				Instance.Init();
			}
		}

		private void Init()
		{
			using (window = new EngineWindow(1280, 720, "Application"))
			{
				window.Run();
			}
		}
	}
}
