using OpenTK.Mathematics;
using PGK2.Engine.Components;
using PGK2.Engine.Components.Base;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Komponent światła służy do efektów oświetlenia w scenie.
	/// </summary>
	public class Light : Core.Component
	{
		/// <summary>
		/// Pobiera pozycję światła.
		/// </summary>
		public Vector3 Position => transform.Position;

		/// <summary>
		/// Intensywność światła otoczenia.
		/// </summary>
		public Vector3 Ambient;

		/// <summary>
		/// Intensywność światła rozproszonego.
		/// </summary>
		public Vector3 Diffuse;

		/// <summary>
		/// Intensywność światła lustrzanego.
		/// </summary>
		public Vector3 Specular;

		/// <summary>
		/// dwołanie do komponentu renderowania modelu używanego do wizualnej reprezentacji światła.
		/// </summary>
		[JsonIgnore]
		public ModelRenderer? LightModel { get; private set; }

		/// <summary>
		/// Określa, czy podczas tworzenia światła powinien zostać utworzony model.
		/// </summary>
		public bool CreateModelOnCreation = false;

		/// <summary>
		/// Konstruktor dla komponentu Light.
		/// Automatycznie dodaje światło do listy świateł bieżącej sceny.
		/// </summary>
		public Light()
		{
			MyScene.Lights.Add(this);
			OnSceneTransfer += sceneTransfer;
		}

		private void sceneTransfer(SceneSystem.Scene? OldScene)
		{
			if (OldScene != null)
			{
				/// Usuwa światło z listy świateł starej sceny.
				OldScene.Lights.Remove(this);
			}

			if (MyScene != null && !MyScene.Lights.Contains(this))
			{
				/// Dodaje światło do listy świateł nowej sceny, jeśli jej jeszcze nie zawiera.
				MyScene.Lights.Add(this);
			}
		}

		/// <summary>
		/// Wywoływane, gdy komponent zostaje zniszczony.
		/// </summary>
		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		/// <summary>
		/// Konstruktor dla komponentu Light z początkowymi właściwościami światła.
		/// </summary>
		/// <param name="ambient">Intensywność światła otoczenia.</param>
		/// <param name="diffuse">Intensywność światła rozproszonego.</param>
		/// <param name="specular">Intensywność światła lustrzanego.</param>
		public Light(Vector3 ambient, Vector3 diffuse, Vector3 specular) : this()
		{
			Ambient = ambient;
			Diffuse = diffuse;
			Specular = specular;
		}

		/// <summary>
		/// Wywoływane, gdy komponent się uruchamia.
		/// Jeśli CreateModelOnCreation jest prawdą, tworzy komponent renderowania modelu, aby wizualnie reprezentować światło.
		/// </summary>
		public override void Start()
		{
			base.Start();

			if (CreateModelOnCreation)
			{
				LightModel = Components.Add<ModelRenderer>();
				LightModel.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");

				if (LightModel.Model != null)
				{
					LightModel.transform.LocalScale = Vector3.One * 0.5f;
					LightModel.OverrideMaterials[0] = new Material(EngineWindow.lightShader);
					LightModel.OverrideMaterials[0].Vector3Values["lightcolor"] = Diffuse;
				}
			}
		}
	}
}
