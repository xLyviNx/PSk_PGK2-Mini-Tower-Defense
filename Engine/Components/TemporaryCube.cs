using OpenTK.Mathematics;
using PGK2.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Components
{
	internal class TemporaryCube : Component
	{
		float timeleft = 15f;
		ModelRenderer cube;
		public override void Update()
		{
			timeleft-=Time.deltaTime;
			if(timeleft < 0f)
			{
				gameObject.Destroy();
			}
		}
		public TemporaryCube()
		{
			cube = Components.Add<ModelRenderer>();
			cube.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
			if (cube.Model != null)
			{
				cube.transform.LocalScale = Vector3.One * 0.0005f;
			}
		}
		public override void Awake()
		{
			base.Awake();
			
		}
		public static GameObject Create(Vector3 position)
		{
			GameObject obj = new GameObject();
			TemporaryCube cube = obj.AddComponent<TemporaryCube>();
			obj.transform.Position = position;
			return obj;
		}
	}
}
