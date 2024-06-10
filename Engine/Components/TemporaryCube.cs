using OpenTK.Mathematics;
using PGK2.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGK2.Engine.Components
{
	/// <summary>
	/// Reprezentuje tymczasowy sześcian, który automatycznie znika po upływie określonego czasu.
	/// </summary>
	public class TemporaryCube : Component
	{
		/// <summary>
		/// Pozostały czas do zniszczenia sześcianu.
		/// </summary>
		float timeleft = 15f;

		/// <summary>
		/// Renderer modelu sześcianu.
		/// </summary>
		ModelRenderer cube;

		/// <summary>
		/// Metoda aktualizująca stan sześcianu. Zmniejsza pozostały czas i niszczy obiekt, gdy czas upłynie.
		/// </summary>
		public override void Update()
		{
			timeleft -= Time.deltaTime;
			if (timeleft < 0f)
			{
				gameObject.Destroy();
			}
		}

		/// <summary>
		/// Konstruktor klasy TemporaryCube. Inicjalizuje renderer modelu i ładuje model sześcianu.
		/// </summary>
		public TemporaryCube()
		{
			cube = Components.Add<ModelRenderer>();
			cube.Model = Model.LoadFromFile($"{EngineInstance.ASSETS_PATH}/Models/cube.fbx");
			if (cube.Model != null)
			{
				cube.transform.LocalScale = Vector3.One * 0.1f;
			}
		}

		/// <summary>
		/// Metoda wywoływana przy tworzeniu obiektu. 
		/// </summary>
		public override void Awake()
		{
			base.Awake();
		}

		/// <summary>
		/// Tworzy nowy obiekt tymczasowego sześcianu w określonej pozycji.
		/// </summary>
		/// <param name="position">Pozycja, w której ma zostać utworzony sześcian.</param>
		/// <returns>Nowy obiekt gry z komponentem TemporaryCube.</returns>
		public static GameObject Create(Vector3 position)
		{
			GameObject obj = new GameObject();
			TemporaryCube cube = obj.AddComponent<TemporaryCube>();
			obj.transform.Position = position;
			return obj;
		}
	}
}
