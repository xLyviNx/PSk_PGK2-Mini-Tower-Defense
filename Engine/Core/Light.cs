using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;

namespace PGK2.Engine.Core
{
	public class Light : Core.Component
	{
		public Vector3 Position;
		public Vector3 Ambient;
		public Vector3 Diffuse;
		public Vector3 Specular;
		public ModelRenderer? LightModel;
		public bool CreateModelOnCreation = true;
		public Light()
		{
			MyScene.Lights.Add(this);
			OnSceneTransfer += sceneTransfer;
		}

		private void sceneTransfer(SceneSystem.Scene OldScene)
		{
			if(OldScene!=null)
			{
				OldScene.Lights.Remove(this);
			}
			if(MyScene!=null && !MyScene.Lights.Contains(this))
			{
				MyScene.Lights.Add(this);
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

		}
		public Light(Vector3 position, Vector3 ambient, Vector3 diffuse, Vector3 specular) : this()
		{
			Position = position;
			Ambient = ambient;
			Diffuse = diffuse;
			Specular = specular;
		}

		public override void Start()
		{
			base.Start();
			if (CreateModelOnCreation)
			{
				LightModel = Components.Add<ModelRenderer>();
				LightModel.Model = new Model("Models/cube.fbx");
				if (LightModel.Model != null)
				{
					LightModel.transform.Scale = Vector3.One * 0.001f;
					LightModel.Model.meshes[0].Material = new Material(EngineWindow.lightShader);
					LightModel.Model.meshes[0].Material.Vector3Values["lightcolor"] = Diffuse;
				}
			}
		}

	}

}