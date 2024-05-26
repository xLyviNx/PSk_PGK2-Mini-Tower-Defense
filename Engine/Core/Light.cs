using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
	public class Light : Core.Component
	{
		[JsonIgnore]
		public Vector3 Position => transform.Position;
		public Vector3 Ambient;
		public Vector3 Diffuse;
		public Vector3 Specular;
		[JsonIgnore]
		public ModelRenderer? LightModel;
		public bool CreateModelOnCreation = true;
		public Light()
		{
			MyScene.Lights.Add(this);
			OnSceneTransfer += sceneTransfer;
		}

		private void sceneTransfer(SceneSystem.Scene? OldScene)
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
		public Light(Vector3 ambient, Vector3 diffuse, Vector3 specular) : this()
		{
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
				LightModel.Model = new Model($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
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