using PGK2.Engine.Core;
namespace PGK2.TowerDef.Scripts
{
	/**
     * @class MouseLockController
     * @brief Kontroluje blokadę myszy w grze Tower Defense.
     */
	public class MouseLockController : Component
	{
		/// <summary>
		/// Flaga określająca, czy gracz trzyma prawy przycisk i obraca kamere.
		/// </summary>
		public static bool HoldingCamera;

		/// <summary>
		/// Właściwość określająca, czy mysz jest zablokowana.
		/// </summary>
		public static bool IsLocked
		{
			get
			{
				if (HoldingCamera) return true;
				return false;
			}
		}

		/// <summary>
		/// Metoda wywoływana co klatkę aktualizująca stan blokady myszy.
		/// </summary>
		public override void Update()
		{
			base.Update();
			Mouse.IsLocked = IsLocked;
		}
	}
}
