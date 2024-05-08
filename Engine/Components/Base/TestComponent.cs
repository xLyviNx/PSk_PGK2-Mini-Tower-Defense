using PGK2.Engine.Core;
using System.Text.Json.Serialization;

namespace PGK2.Engine.Components.Base
{
    [Serializable]
    public class TestComponent : Component
    {
        public string Test = "Hello World!";
    }
}
