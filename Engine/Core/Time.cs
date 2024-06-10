using System;

namespace PGK2.Engine.Core
{
	/// <summary>
	/// Klasa zarządzająca czasem (miedzy klatkami czyli delta time) w grze.
	/// </summary>
	public class Time
	{
		/// <summary>
		/// Ostatni czas (do obliczen deltatime).
		/// </summary>
		public static long lastTime = 0;

		/// <summary>
		/// Deltatime w podwójnej precyzji.
		/// </summary>
		public static double doubleDeltaTime = 0;

		/// <summary>
		/// Skala czasu.
		/// </summary>
		public static float timeScale = 1f;

		/// <summary>
		/// Nieprzeskalowane deltatime.
		/// </summary>
		public static float unscaledDeltaTime
		{
			get => ((float)doubleDeltaTime);
			set { doubleDeltaTime = value; }
		}

		/// <summary>
		/// DeltaTime, uwzględniający skalę czasu.
		/// </summary>
		public static float deltaTime { get => unscaledDeltaTime * timeScale; }
	}
}
