using System.Numerics;
using MobarezooServer.GamePlay;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;
using shortid;


namespace MobarezooServer.Gameplay.GameObjects
{
    public class Obstacle
    {
        [JsonProperty("id")] public string id;
        public Shape shape;
        public bool enable;
        public bool river;

        public Obstacle()
        {
            id = ShortId.Generate();
        }
    }
}