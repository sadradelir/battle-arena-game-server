using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using MobarezooServer.AI;
using MobarezooServer.BalanceData;
using MobarezooServer.GamePlay.Actors;
using MobarezooServer.GamePlay.Champions;
using MobarezooServer.GamePlay.EventSystem;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;
using Serialization.SyncClasses;
using shortid;
using Activator = MobarezooServer.GamePlay.Actors.Activator;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;


namespace MobarezooServer.GamePlay
{
 
    public class Room
    {
        public Queue<playerInput> playerInputs;

        public class playerInput
        {
            public bool botInput;
            public IPEndPoint ipEndPoint;
            public byte[] data;
        }

        public class botBrainData
        {
            public enum ThinkState{
                ATTACK,SIDESTEP,FIND
            }
            public ThinkState state;    
            public Vector2 lastDirection;    
        }
        

        public class SummonerData // SUMMONER
        {
            public byte order;
            
            public bool Bot;
        //    public NlNetwork brain;
            public botBrainData botBrainData;

            public int attackTimer;
            public PlayerMatchDetails details;
            public string deviceId;
            public Champion champion;
            [JsonIgnore] public IPEndPoint playerEndpoint;

            public SummonerData(Champion champion)
            {
                champion.owner = this;
                this.champion = champion;
            }
        }

        public enum ActType
        {
            POSITION = 1, // nothing - just set my position // 
            INSTANTIATE = 2,
            RELOAD = 3,
        }

        public class GameActMessage
        {
            public ActType actType;

            public Vector2 position;
            public int angle;

            public bool moving;
            public int[] parameters;
        }

        class ServerObsObj
        {
            public string type;
            public Vector2 position;
            public Vector2 size;
        }

        class Map
        {
            public List<ServerObsObj> obstacles;
            public byte playerSpawn;
        }

        public string id;
        public Battle battleObj;
        public List<SummonerData> summoners;
        public List<Obstacle> obstacles;
        public List<Activator> actors;
        public List<GameObject> objects;
        public List<Trap> traps;
        public List<GameEvent> gameEvents;
        private List<TimerLoopAction> timers;
        public GameBalance balanceData;
        public ProtoRoomSnapshot lastSnapShot;

        public Object objectsLock;
        public Object playersLock;
        public Object obstaclesLock;
        public Object eventsLock;
        public Object trapsLock;
        public Object timersLock;

        public bool gameEnd;
        public int afterEndTimer;
        public byte[] mapData; 

        
        public Room(GameBalance balanceData, PlayerMatchDetails player1, PlayerMatchDetails player2, Battle battle , int mapIndex = 0)
        {
            battleObj = battle;
            id = ShortId.Generate();
            summoners = new List<SummonerData>();
            obstacles = new List<Obstacle>();
            actors = new List<Activator>();
            objects = new List<GameObject>();
            traps = new List<Trap>();
            timers = new List<TimerLoopAction>();
            playerInputs = new Queue<playerInput>();
            gameEvents = new List<GameEvent>(); 
            this.balanceData = balanceData;

            objectsLock = new object();
            playersLock = new object();
            obstaclesLock = new object();
            eventsLock = new object();
            trapsLock = new object();
            timersLock = new object();

            var roomText = File.ReadAllText("room.json");
            var map = JsonConvert.DeserializeObject<List<Map>>(roomText);
            byte[] data, buffer;
            using (MemoryStream ms = new MemoryStream())
            {
                
                buffer = BitConverter.GetBytes( map[mapIndex].obstacles.Count );
                ms.Write(buffer , 0 , buffer.Length); // 4

                foreach (var serverObsObj in map[mapIndex].obstacles)
                {
                    if (serverObsObj.type == "heal")
                    {
                        var healActor = new Activator()
                        {
                            shape = new Circle(Vector2.Zero, 75),
                            duration = 1500,
                            coolDown = 30000,
                            onChannelAction = (c) =>
                            {
                                c.owner.details.ActivatorsActivated++;
                                c.Heal((int) (c.maxHealth * 0.3f));
                            }
                        };
                        actors.Add(healActor);
                        AddTimer(new TimerLoopAction(() => { healActor.coolDown -= 500; }));
                    }
                    else
                    {
                        var rectangle = new Rectangle(
                            serverObsObj.position,
                            serverObsObj.size.X,
                            serverObsObj.size.Y,
                            0
                        );
                        obstacles.Add(new Obstacle()
                        {
                            shape = rectangle,
                            river = serverObsObj.type == "river",
                            enable = true
                        });
                        ms.WriteByte((byte) (serverObsObj.type == "river" ? 0 : 1));
                        buffer = rectangle.getSerilized(); 
                        ms.Write(buffer , 0 , buffer.Length); // 4
                    }
                    ms.WriteByte(map[mapIndex].playerSpawn);
                    mapData = ms.ToArray();
                }
            }

            prepareWalls();
            // should i prepare summoners ? yes ...
            var champion1 = GetChampionInstance(player1.Champion, player1.ChampionLevel ,player2.ChampionStars, 1);
            var player1SummonerData = new SummonerData(champion1)
            {
                deviceId = player1.PlayerRef,
                order = 1,
                Bot = player1.IsAi,
                details = battleObj.Player1Details,
                // playerEndpoint = we dont have it yet ... he himself should declare this  
            };
            if (player1SummonerData.Bot)
            {
             // player1SummonerData.brain = new NlNetwork(10, 8, 2);
             // player1SummonerData.brain.readFromFile();
                player1SummonerData.botBrainData = new botBrainData()
                {
                    state = botBrainData.ThinkState.ATTACK,
                };
                
            }
            var champion2 = GetChampionInstance(player2.Champion, player2.ChampionLevel , player2.ChampionStars , 2);
            var player2SummonerData = new SummonerData(champion2)
            {
                deviceId = player2.PlayerRef,
                order = 2,
                Bot = player2.IsAi,
                details = battleObj.Player1Details,
                // playerEndpoint = we dont have it yet ... he himself should declare this  
            };
            if (player2SummonerData.Bot)
            {
                // player2SummonerData.brain = new NlNetwork(10, 8, 2);
                // player2SummonerData.brain.readFromFile();
                player2SummonerData.botBrainData = new botBrainData()
                {
                    state = botBrainData.ThinkState.ATTACK,
                };
            }
            lock (playersLock)
            {
                summoners.Add(player1SummonerData);
                summoners.Add(player2SummonerData);
            }

            // ok !~ ready for start the battle .... waiting for players to join the lobby
        }

        public void prepareWalls()
        {
            obstacles.Add(new Obstacle()
            {
                shape = new Rectangle(
                    new Vector2(750, 0),
                    300,
                    2000,
                    0
                ),
                river = false,
                enable = true
            });
            obstacles.Add(new Obstacle()
            {
                shape = new Rectangle(
                    new Vector2(-750, 0),
                    300,
                    2000,
                    0
                ),
                river = false,
                enable = true
            });
            obstacles.Add(new Obstacle()
            {
                shape = new Rectangle(
                    new Vector2(0, 1150),
                    1200,
                    300,
                    0
                ),
                river = false,
                enable = true
            });
            obstacles.Add(new Obstacle()
            {
                shape = new Rectangle(
                    new Vector2(00, -1150),
                    1200,
                    300,
                    0
                ),
                river = false,
                enable = true
            });
        }

        public int bindPlayerToHisChampion(string deviceId, IPEndPoint endPoint)
        {
            lock (playersLock)
            {
                var summoner = summoners.FirstOrDefault(t => t.deviceId == deviceId);
                if (summoner == null) return 0;
                summoner.playerEndpoint = endPoint;
                return summoner.order;
            }
        }

        public Champion GetChampionInstance(int index, int level , int stars , int position)
        {
            var type = (Champion.ChampionType) index;
            Champion champion;
            switch (type)
            {
                case Champion.ChampionType.SHARA:
                    champion = new Shara(this , level , stars);
                    break;
                case Champion.ChampionType.FARIVAZ:
                    champion = new Farivaz(this , level , stars);
                    break;
                case Champion.ChampionType.SIVAN:
                    champion = new Sivan(this , level , stars);
                    break;
                case Champion.ChampionType.ARITA:
                    champion = new Arita(this , level , stars);
                    break;
                case Champion.ChampionType.DIANOUSH:
                    champion = new Dianoush(this , level , stars);
                    break;
                case Champion.ChampionType.MEEKO:
                    champion = new Meeko(this , level , stars);
                    break;
                case Champion.ChampionType.VEEMAN:
                    champion = new Veeman(this , level , stars);
                    break;
                case Champion.ChampionType.ARAKHSH:
                    champion = new Arakhsh(this , level , stars);
                    break;
                case Champion.ChampionType.LANGAAR:
                    champion = new Langaar(this , level , stars);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            champion.hitCircle.position = position == 1 ? new Vector2(0, -700) : new Vector2(0, 700); 
            return champion;
        }

        public void enqueueActToProcess(IPEndPoint player, byte[] data)
        {
            playerInputs.Enqueue(new playerInput()
            {
                ipEndPoint = player, data = data
            });
        }
        
        public void enqueueBotActToProcess(byte[] data)
        {
            playerInputs.Enqueue(new playerInput()
            {
                ipEndPoint = null , data = data , botInput = true
            });
        }


        public void processInputs(int deltaTime)
        {
            while (playerInputs.Count > 0)
            {
                var inpt = playerInputs.Dequeue();
                ProcessPlayerInput(inpt.ipEndPoint, inpt.data , inpt.botInput);
            }
        }

        private void ProcessPlayerInput(IPEndPoint playerEndPoint, byte[] data , bool botInput = false)
        {
            if (playerEndPoint == null && !botInput)
            {
                return;
            }
            
            Champion champion;
            if (!botInput)
            {
                lock (playersLock)
                {
                    var presenceData = summoners.Where(s => s.playerEndpoint != null).FirstOrDefault(t => t.playerEndpoint.Equals(playerEndPoint));
                    if (presenceData == null)
                    {
                        return;
                    }
                    champion = presenceData.champion;
                }
            }
            else
            {
                ProtoChampion pcss1 = ProtoChampion.getFromBytes(data);
                champion = summoners[pcss1.uId - 1].champion;
            }

            ProtoChampion pcss = ProtoChampion.getFromBytes(data);
            if (true)
            {
                if (champion.disabled)
                {
                    //Console.WriteLine("disabled !, u cant move ... ");
                    // todo shall i ? 
                    return;
                }
                else
                {
                    //Console.Write("");
                }

                champion.moving = pcss.moving;

                champion.hitCircle.moveTo(pcss.position);
                champion.hitCircle.Rotate(pcss.rotation);
                champion.ProcessTheActMessage(pcss);

                // clamp to map bounds
                //presenceData.champion.transform.fx.clampTo(-600, 600); // 60  in unity
                //presenceData.champion.transform.fy.clampTo(-1000, 1000); // 100 in unity
            }

            if (pcss.attacking)
            {
                lock (objectsLock)
                {
                    if (champion.reloading)
                    {
                        Console.WriteLine("on reload");
                    }
                    else
                    {
                        var newObj = champion.GetAttackProjectile(pcss);
                        if (!objects.Contains(newObj))
                        {
                            objects.Add(newObj);
                        }
                    }
                }
            }

            if (pcss.reloading)
            {
                lock (objectsLock)
                {
                    champion.Reload();
                }
            }
        }

        public static void KnockBackFromNonRotatedRectangle(Rectangle thatObs, Champion champion)
        {
            // find extraction points
            // obstacles never rotate 
            var incX = new Vector2() {X = thatObs.corner[1].X + champion.hitCircle.radius, Y = champion.hitCircle.position.Y};
            var incY = new Vector2() {X = champion.hitCircle.position.X, Y = thatObs.corner[2].Y + champion.hitCircle.radius};
            var decX = new Vector2() {X = thatObs.corner[0].X - champion.hitCircle.radius, Y = champion.hitCircle.position.Y};
            var decY = new Vector2() {X = champion.hitCircle.position.X, Y = thatObs.corner[0].Y - champion.hitCircle.radius};

            // find the nearest outside point 
            var all = new List<Vector2>();
            all.Add(incX);
            all.Add(incY);
            all.Add(decX);
            all.Add(decY);
            var sorted = all.OrderBy(t => Vector2.Distance(t, champion.hitCircle.position)).ToList();

            // knock back !
            champion.hitCircle.position = sorted[0];
        }


        private Obstacle getAnyIntersectingObstacle(Champion champion)
        {
            foreach (var t in obstacles)
            {
                if (champion.IsIntersecting(t.shape))
                {
                    return t;
                }
            }

            return null;
        }


        private List<Trap> getAllIntersectingTrap(Champion champion)
        {
            var ret = new List<Trap>();
            foreach (var t in traps)
            {
                if (champion.IsIntersecting(t.shape))
                {
                    ret.Add(t);
                }
            }

            return ret;
        }

        private Trap getAnyIntersectingTrap(Champion champion)
        {
            foreach (var t in traps)
            {
                if (champion.IsIntersecting(t.shape))
                {
                    return t;
                }
            }

            return null;
        }

        private Obstacle getAnyIntersectingObstacle(GameObject gObject)
        {
            foreach (var t1 in obstacles)
            {
                if (gObject.shape.IsIntersecting(t1.shape))
                {
                    return t1;
                }
            }

            return null;
        }

        public void AddTimer(TimerLoopAction timer)
        {
            lock (timersLock)
            {
                if (timers.Contains(timer))
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    timers.Add(timer);
                }
            }
        }

        public Champion FindEnemyChampion(Champion self)
        {
            SummonerData enemy = null;
            lock (playersLock)
            {
                enemy = summoners.FirstOrDefault(t => t.champion != self);
            }

            return enemy.champion;
        }

        public void UpdateTimers(int deltaTime)
        {
            if (gameEnd)
            {
                afterEndTimer += deltaTime;
            }
            lock (timersLock)
            {
                foreach (var timer in timers)
                {
                    timer.UpdateTimer(deltaTime);
                }
            }
        }

        public void UpdatePlayers(int deltaTime)
        {
            lock (playersLock)
            {
                foreach (var champion in summoners.Select(t => t.champion))
                {
                    if (champion.isChanneling)
                    {
                        champion.proceedChannel((int) deltaTime);
                    }

                    if (champion.disabled)
                    {
                        var playerPos = champion.hitCircle.position;
                        var targetPos = champion.inDisableMoveTarget;
                        var newpos = MathUtils.MoveTowards(playerPos, targetPos,
                            champion.inDisableMoveSpeed * deltaTime / 33f);
                        champion.hitCircle.position = newpos;
                        if (Vector2.Distance(champion.hitCircle.position, targetPos) < 1)
                        {
                            champion.disabled = false;
                        }
                    }
                    else
                    {
                        var obs = getAnyIntersectingObstacle(champion);
                        if (obs != null)
                        {
                            KnockBackFromNonRotatedRectangle(obs.shape as Rectangle, champion);
                        }
                    }

                    var trap = getAllIntersectingTrap(champion);
                    ;
                    champion.GetTrapsEffect(trap);

                    champion.UpdateBuffs(deltaTime);

                    var thatAct = GetAnyIntersectingActivtor(champion);
                    if (thatAct != null && !thatAct.onChannel && thatAct.coolDown <= 0)
                    {
                        var channelStarted = champion.startChannelTo(() =>
                        {
                            thatAct.coolDown = 30000;
                            thatAct.onChannelAction.Invoke(champion);
                        }, thatAct.duration, () => { thatAct.onChannel = false; });
                        thatAct.onChannel = channelStarted;
                    }
                }
            }
        }

        // checkActors 
        public Activator GetAnyIntersectingActivtor(Champion champion)
        {
            foreach (var activator in actors)
            {
                if (champion.hitCircle.IsIntersecting(activator.shape))
                {
                    return activator;
                }
            }

            return null;
        }

        public void UpdateObjects(long deltaTime)
        {
            // single thread ... 
            // get copy : 
            List<GameObject> objectsCopy = null;
            lock (objectsLock)
            {
                objectsCopy = objects.ToList();
            }

            foreach (GameObject obj in objectsCopy)
            {
                if (obj.isWaiting)
                {
                    obj.ProceedWait((int) deltaTime);
                }

                obj.UpdateTransform(deltaTime);

                // projectile destroy conditions 
                if (Math.Abs(obj.shape.position.X) > 1000 || Math.Abs(obj.shape.position.Y) > 2000)
                {
                    obj.deleted = true;
                    continue;
                }

                var thatObs = getAnyIntersectingObstacle(obj);
                if (thatObs != null)
                {
                    obj.IntersectingWithObstacle(thatObs);
                }

                if (obj.deleted)
                {
                    continue;
                }

                if (obj.selfHit)
                {
                    var self = obj.ownerChampion;
                    if (self != null && self.hitCircle.IsIntersecting(obj.shape))
                    {
                        obj.IntersectingWithOwner();
                    }
                }

                if (obj is AreaOfDamage)
                {
                    var objAsExplosion = (AreaOfDamage) (obj);
                    if (objAsExplosion.perHalfSeconds)
                    {
                        objAsExplosion.proceedPerHalfSeconds((int) deltaTime);
                    }

                    if (!objAsExplosion.immediateCast)
                    {
                        objAsExplosion.proceedCastTime((int) deltaTime);
                    }
                    else 
                    {
                        var enemy = FindEnemyChampion(obj.ownerChampion);
                        if (enemy != null && obj.shape.IsIntersecting(enemy.hitCircle))
                        {
                            objAsExplosion.IntersectingWithPlayer(enemy);
                        }
                        else
                        {
                            objAsExplosion.damagedOnce = false;
                        }

                        if (objAsExplosion.singleFrame)
                        {
                            objAsExplosion.deleted = true;
                        }
                    }
                }

                if (obj is Projectile)
                {
                    var projectileObj = (Projectile) (obj);
                    var enemy = FindEnemyChampion(obj.ownerChampion);
                    if (enemy != null && enemy.hitCircle.IsIntersecting(obj.shape))
                    {
                        projectileObj.IntersectingWithPlayer(enemy);
                    }
                    else
                    {
                        projectileObj.hitActedOnce = false;
                    }
                }
            }

            lock (objectsLock)
            {
                objects = objects.ToList().Where(t => !t.deleted).ToList();
            }
        }

        public void SyncDataToRoomPlayers(UDPServer udpServer)
        {
            // lets make roomSnapShot (SadyVersion)
            var array = prepareRoomSnapshot();
            array = (new[] {(byte) 0x04}).Concat(array).ToArray();
            lock (playersLock)
            {
                foreach (SummonerData player in summoners.Where(t => !t.Bot))
                {
                    if (player.playerEndpoint != null)
                    {
                        udpServer.sendMessageToEndpoint(array, player.playerEndpoint);
                    }
                }
            }
            lock (eventsLock)
            {
                if (!gameEnd && gameEvents.Any(t=>t.type == GameEvent.GameEventType.END_GAME))
                {
                    gameEnd = true;
                    Console.WriteLine("GAME ENDS");
                    Thread t = new Thread(disposeRoom);
                    t.Start();
                }
                else
                {
                    gameEvents.Clear();
                }
            }
        }

        private async void disposeRoom()
        {
            using (var client = new HttpClient())
            {
                Console.WriteLine("GOING TO DISPOSE ROOM");
                client.BaseAddress = new Uri("http://5.144.128.233:5000");
                var strMessage = JsonConvert.SerializeObject(battleObj); 
                Console.WriteLine(strMessage);
                var stringContent=new StringContent(strMessage,Encoding.UTF8,"application/json");
                var result = await client.PostAsync("/api/aftermatch/matchend", stringContent);
                Console.WriteLine(result);
            }
        }
        
        public int snapShotCounter;
        byte[] prepareRoomSnapshot()
        {
            ProtoRoomSnapshot prss = new ProtoRoomSnapshot();
            prss.champions = new List<ProtoChampion>();
            //prss.gameObjects = new ProtoGameObject[room.objects.Count];

            for (int i = 0; i < summoners.Count; i++)
            {
                ProtoChampion pr = new ProtoChampion();
                var champion = summoners[i].champion;
                pr.position = champion.hitCircle.position.ToProto();
                pr.rotation = champion.hitCircle.rotation;
                pr.movementSpeed = (ushort) champion.speed;
                pr.moving = champion.moving;
                pr.channeling = champion.isChanneling;
                pr.attackTarget = new ProtoVector();
                pr.attackInterval = (ushort) champion.attackInterval;
                pr.uId = summoners[i].order;
                pr.health = (ushort) champion.health;
                pr.disabled = champion.disabled;
                pr.shield = (ushort) champion.shield;
                pr.maxHealth = (ushort) champion.maxHealth;
                pr.championIndex = (byte) champion.type;
                pr.reloading = champion.reloading;
                if (champion is Arita a)
                {
                    pr.attackParameter1 = (byte) a.stacks;
                }
                if (champion is Veeman v)
                {
                    pr.attackParameter1 = (byte) (v.blocker ? 1 : 0);
                }
                if (champion is Dianoush d)
                {
                    pr.attackParameter1 = (byte) (d.blocker ? 1 : 0);
                }
                
                prss.champions.Add(pr);
            }

            prss.gameObjects = new ProtoGameObject[objects.Count];
            for (int i = 0; i < objects.Count; i++)
            {
                ProtoGameObject prgo = new ProtoGameObject();
                var obj = objects[i];
                prgo.position = obj.shape.position.ToProto();
                prgo.rotation = obj.shape.rotation;
                prgo.dimension = obj.shape.dimention();
                prgo.uId = obj.id;
                prgo.gameObjectType = (ushort) obj.type;
                prgo.speed = obj.getSpeed();
                if (obj.ownerChampion.owner != null)
                {
                    prgo.ownerOrder = obj.ownerChampion.owner.order;
                }
                if (obj is Projectile p)
                {
                    prgo.orbital = p.orbital;
                    prgo.homing = p.homing;
                    prgo.target = p.homing ? p.homingTarget.position.ToProto() : new ProtoVector();
                }
                else
                {
                    prgo.homing = false;
                    prgo.target = new ProtoVector();
                }
                prss.gameObjects[i] = prgo;
            }

            prss.frameNumber = ++snapShotCounter;

            lock (eventsLock)
            {
                prss.gameEvents = new List<GameEvent>();
               foreach (var gameEvent in gameEvents)
               {
                   prss.gameEvents.Add(gameEvent);
               }
            }

            lastSnapShot = prss;
            return prss.getSerialized();
        }

        public void AddEvent(GameEvent.GameEventType type, int value, uint target)
        {
            lock (eventsLock)
            {
                Console.WriteLine("new event: " + type + " value: " + value);
                gameEvents.Add(new GameEvent()
                {
                    type = type,
                    value = value,
                    targetObject = target
                });
            }
        }
    }

 
}