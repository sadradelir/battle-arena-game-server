using System;
using MobarezooServer.GamePlay.Champions;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;
using shortid; 

namespace MobarezooServer.GamePlay.Actors
{
    public class Activator
    { 
            [JsonProperty("id")] public string id;
            public Shape shape;
            public Action<Champion> onChannelAction;
            public int duration; // in millisecond
            public bool onChannel;
            public int coolDown;
            public Activator()
            {
                id = ShortId.Generate();
            }
    }
}