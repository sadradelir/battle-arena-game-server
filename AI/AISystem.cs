using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using MobarezooServer.GamePlay;
using MobarezooServer.GamePlay.Champions;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Networking.Serialization.SyncClasses;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Serialization.SyncClasses;

namespace MobarezooServer.AI
{
    public class AISystem
    {
        public static void enqueueInputs(Room r, int deltaTime)
        {
            for (var index = 0; index < r.summoners.Count; index++)
            {
                var summoner = r.summoners[index];
                var enemy = r.summoners[1 - index];
                if (summoner.Bot)
                {
                    Random random = new Random();
                    var myPosition = summoner.champion.hitCircle.position;
                    var sorted = r.objects.Where(t => t.ownerChampion.owner != summoner).OrderBy(t => Vector2.Distance(t.position, myPosition));
                    var nearest = sorted.Take(2).ToList();
                    
                    if (summoner.botBrainData.state == Room.botBrainData.ThinkState.SIDESTEP)
                    {

                        if (nearest.Count > 0)
                        {
                            if (Vector2.Distance(nearest[0].position, myPosition) > 500)
                            {
                                summoner.botBrainData.state = Room.botBrainData.ThinkState.ATTACK;
                            }
                        }
                        else
                        {
                                summoner.botBrainData.state = Room.botBrainData.ThinkState.ATTACK;
                        }

                        ActTheThinkResult(r, deltaTime, summoner.botBrainData.lastDirection, summoner, enemy);
                        return;
                    }
                     
                    if (summoner.botBrainData.state == Room.botBrainData.ThinkState.FIND)
                    {
                        var lineOfSight = new Segment(myPosition,enemy.champion.hitCircle.position);
                        foreach (var o in r.obstacles.Where(t=>t.river == false))
                        {
                            if (lineOfSight.isIntersecting((Rectangle)o.shape))
                            {
                                // no line of sight
                                summoner.botBrainData.state = Room.botBrainData.ThinkState.FIND;
                                ActTheThinkResult(r, deltaTime, summoner.botBrainData.lastDirection, summoner, enemy);
                                return;
                            }
                        }
                        summoner.botBrainData.state = Room.botBrainData.ThinkState.ATTACK;
                    }
                    
                    // line of sight ... 
                    if (summoner.botBrainData.state == Room.botBrainData.ThinkState.ATTACK
                        && summoner.champion.type != Champion.ChampionType.ARITA)
                    {
                       var lineOfSight = new Segment(myPosition,enemy.champion.hitCircle.position);
                       foreach (var o in r.obstacles.Where(t=>t.river == false))
                       {
                           if (lineOfSight.isIntersecting((Rectangle)o.shape))
                           {
                               // no line of sight
                               summoner.botBrainData.state = Room.botBrainData.ThinkState.FIND;
                               var v = enemy.champion.hitCircle.position - myPosition;
                               v.Rotate(random.Next(2) < 1 ? 90 : -90);
                               summoner.botBrainData.lastDirection = v;
                               ActTheThinkResult(r, deltaTime, v, summoner, enemy);
                               return;
                           }
                       }
                    }
                    
 
                    if (nearest.Count > 0)
                    {
                        if (Vector2.Distance(nearest[0].position, myPosition) < 500)
                        {
                            if (nearest[0] is Projectile p)
                            {
                                var v = p.getVectorDirection();
                                v.Rotate(90);
                                // lets fix v
                                if (Math.Abs(myPosition.X) > 300 || Math.Abs(myPosition.Y) > 600 )
                                {
                                    // i mean dont dodge to wall ...
                                    // if u go by that way u end to this point 
                                    var p1 = myPosition + v;
                                    // but if i rotate that vector
                                    v.Rotate(180);
                                    // u end up this point
                                    var p2 = myPosition + v;
                                    // if former is better
                                    if (p1.Length() < p2.Length())
                                    {
                                        // back to that
                                        v.Rotate(180);
                                    }
                                    // todo fix to entropy
                                }
                                summoner.botBrainData.state = Room.botBrainData.ThinkState.SIDESTEP;
                                summoner.botBrainData.lastDirection = v;
                                ActTheThinkResult(r, deltaTime, v, summoner, enemy);
                            }
                            return;
                        }
                        else
                        {
                            ActTheThinkResult(r, deltaTime, Vector2.Zero, summoner, enemy);
                            return;
                        }
                    }
                    else
                    {
                        // check vision 
                        // stop if u have vision 
                        ActTheThinkResult(r, deltaTime, Vector2.Zero, summoner, enemy);
                        return;
                    }
                }
            }
        }

        private static void ActTheThinkResult(Room r, int deltaTime, Vector2 direction, Room.SummonerData summoner, Room.SummonerData enemy)
        {
            var myPosition = summoner.champion.hitCircle.position;

            bool moving = false;
            if (direction.Length() < 0.1f)
            {
                direction = Vector2.Zero;
                summoner.attackTimer += deltaTime;
            }
            else
            {
                moving = true;
                direction = Vector2.Normalize(direction);
                summoner.attackTimer = 0;
            }

            var protoVector = (myPosition + ((deltaTime / 1000f) * direction * 300)).ToProto();
            protoVector.x.clampTo(-525, 525);
            protoVector.y.clampTo(-925, 925);
            ProtoChampion pr = new ProtoChampion()
            {
                attacking = summoner.attackTimer >= summoner.champion.attackInterval,
                position = protoVector,
                moving = moving,
                rotation = (float) (Math.Atan2(direction.Y, direction.X) * 180 / 3.1415926f),
                attackTarget = enemy.champion.hitCircle.position.ToProto(),
                uId = summoner.order
            };
            if (summoner.attackTimer >= summoner.champion.attackInterval)
            {
                pr.rotation = (float) (Math.Atan2(pr.attackTarget.y - myPosition.Y, pr.attackTarget.x - myPosition.X) * 180 / 3.1415926f);
                summoner.attackTimer = 0;
            }

            if (direction == Vector2.Zero)
            {
                pr.rotation = (float) (Math.Atan2(pr.attackTarget.y - myPosition.Y, pr.attackTarget.x - myPosition.X) * 180 / 3.1415926f);
            }
            r.enqueueBotActToProcess(pr.getSerialized());
        }

        public static float getAxisDistance(ProtoVector me, ProtoVector obj, float face)
        {
            var v1 = obj.getVector() - me.getVector();
            float degree = (float) (face - (180 / 3.1415926f) * Math.Atan2(me.x, me.y));
            return (float) (v1.Length() * Math.Tan(degree));
        }


        public class AIPlayer
        {
            public NlNetwork brain;
            public Vector2 position;
            public Vector2 direction;
            public bool died;
            public long score = 0;
            public long Kills = 0;
            public long Dosge = 0;
            public long Shots = 0;
            public AIGameObject[] projectiles;
            public int attackTimer;
            public int health;

            public AIPlayer(int seed)
            {
                attackTimer = 000;
                brain = new NlNetwork(10, 16, 2);
                //  brain.readFromFile();
                Random r = new Random(seed);
                float summonX = 0;
                float summonY = -600;
                float degree = (float) (r.NextDouble() * 360);
                position = new Vector2(summonX, summonY);
                direction = (new Vector2(0, 1));
                direction.Rotate(degree);
            }

            public AIPlayer(AIPlayer p1, AIPlayer p2)
            {
                brain = new NlNetwork(p1.brain, p2.brain);
                position = Vector2.Zero;
                direction = Vector2.Zero;
            }

            public AIPlayer(AIPlayer p1)
            {
                brain = new NlNetwork(p1.brain);
                position = Vector2.Zero;
                direction = Vector2.Zero;
            }

            public void think(ArtificalRoom r)
            {
                // score+=1;

                var sorted = r.gameObjects.Where(t => !t.mine).OrderBy(t => Vector2.Distance(t.position, position));
                var nearest = sorted.Take(2).ToList();
                var inputsMat = new double[]
                {
                    r.boss.X - position.X,
                    r.boss.Y - position.Y,
                    600 - position.X,
                    position.X + 600,
                    1000 - position.Y,
                    position.Y + 1000,
                    nearest[0].position.X,
                    nearest[0].position.Y,
                    nearest[1].position.X,
                    nearest[1].position.Y,
                };
                var brainOut = brain.feedForward(inputsMat);

                string s = "";
                foreach (var c in inputsMat)
                {
                    s += c + "\n";
                }

                //File.WriteAllText(@"../inputesInTrain.txt", s);

                direction = new Vector2((float) brainOut[0, 0], (float) brainOut[0, 1]);
                //direction;
                if (direction.Length() < 0.1f)
                {
                    direction = Vector2.Zero;
                    attackTimer += 16;
                    return;
                }
                else
                {
                    attackTimer += 0;
                }

                direction = Vector2.Normalize(direction);
            }
        }


        public class AIGameObject
        {
            public Vector2 position;
            public Vector2 direction;
            public bool mine;
            public bool deleted;
            public bool outed;

            public AIGameObject()
            {
                float summonX = 10000;
                float summonY = 10000;

                position = new Vector2(summonX, summonY);
                direction = Vector2.Zero;
            }
        }

        public class ArtificalRoom
        {
            public List<AIPlayer> players;
            public List<AIGameObject> gameObjects;
            public Vector2 boss;
            public AIPlayer thePlayer;
            public long maxScore = 0;

            public ArtificalRoom()
            {
                boss = new Vector2(0, 600);
                players = new List<AIPlayer>();
                gameObjects = new List<AIGameObject>();
                for (int i = 0; i < 300; i++)
                {
                    players.Add(new AIPlayer(IdGenerator.Seed()));
                }

                for (int i = 0; i < 3; i++)
                {
                    gameObjects.Add(new AIGameObject() {outed = true});
                }

                Thread t = new Thread(proceed);
                t.Start();
            }


            public void proceed()
            {
                Random x = new Random(IdGenerator.Seed());
                int generationNumber = 0;
                while (true)
                {
                    generationNumber++;
                    List<AIPlayer> grave = new List<AIPlayer>();
                    for (var index = 0; index < players.Count; index++)
                    {
                        var aiPlayer = players[index];
                        ulong tick = 0;
                        thePlayer = aiPlayer;
                        int bossAttack = 0;
                        aiPlayer.health = 100;
                        bool bossOnStop = true;
                        int bossHp = 1;
                        int dodgedBullets = 0;
                        foreach (var obj in gameObjects)
                        {
                            obj.position = Vector2.One * 10000;
                            obj.outed = true;
                        }

                        boss = new Vector2(0, -600);
                        aiPlayer.position = new Vector2(0, 600);
                        // rotate
                        //  var degree = x.Next(360);
                        //  boss.Rotate(degree);
                        //  aiPlayer.position.Rotate(degree);

                        // if (x.Next(2) > 0)
                        // {
                        //     boss = new Vector2(0 , -600);
                        //     aiPlayer.position = new Vector2(0 , 600);
                        // }
                        int bossMovePhaseTimer = 1000;
                        // ... GAME ... 
                        while (true)
                        {
                            // i farz each tick is 16 ms 
                            if (generationNumber % 5 == 1 && index == 1)
                            {
                                Thread.Sleep(16);
                            }

                            tick++;
                            //let them change direction 
                            aiPlayer.think(this);
                            aiPlayer.position += aiPlayer.direction * 300 * (16 / 1000f);
                            aiPlayer.position.X.clampTo(-600, 600);
                            aiPlayer.position.Y.clampTo(-1000, 1000);

                            if (aiPlayer.attackTimer >= 1000)
                            {
                                aiPlayer.attackTimer = 0;
                                AIGameObject projectile = new AIGameObject();
                                projectile.position = aiPlayer.position;
                                projectile.direction = Vector2.Normalize(boss - aiPlayer.position);
                                projectile.mine = true;
                                //  gameObjects.Add(projectile);
                                aiPlayer.score += 2;
                                aiPlayer.Shots += 2;
                            }

                            foreach (var aiGameObject in gameObjects.Where(t => !t.outed))
                            {
                                aiGameObject.position += ((aiGameObject.mine ? 25f : 30f) * (16 / 33f)) * aiGameObject.direction;
                                // wrap
                                if ((aiGameObject.position.X > 800 ||
                                     aiGameObject.position.X < -800 ||
                                     aiGameObject.position.Y > 1200 ||
                                     aiGameObject.position.Y < -1200
                                    ) && !aiGameObject.outed && !aiGameObject.mine)
                                {
                                    aiGameObject.direction = Vector2.Zero;
                                    aiPlayer.score += 1;
                                    aiPlayer.Dosge += 1;
                                    aiGameObject.outed = true;
                                    aiGameObject.position = Vector2.One * 10000;
                                    dodgedBullets++;
                                    //     aiGameObject.direction.Rotate(random.Next(10) - 5);
                                }
                            }

                            foreach (var gameObject in gameObjects.Where(t => !t.outed))
                            {
                                if (!gameObject.mine && Vector2.Distance(aiPlayer.position, gameObject.position) < 50 || tick > 200000)
                                {
                                    // die
                                    //todo 
                                    aiPlayer.health -= 34;
                                    gameObject.outed = true;
                                    if (true || aiPlayer.health <= 0)
                                    {
                                        gameObject.position = Vector2.One * 10000;
                                        aiPlayer.died = true;
                                        grave.Add(aiPlayer);
                                    }

                                    //Console.WriteLine("DIED");
                                    break;
                                }

                                if (gameObject.mine && Vector2.Distance(boss, gameObject.position) < 125)
                                {
                                    // die
                                    bossHp--;
                                    gameObject.deleted = true;
                                    if (bossHp <= 0)
                                    {
                                        dodgedBullets = 0;
                                        //  aiPlayer.score+=5;
                                        // aiPlayer.Kills+=5;
                                        gameObject.deleted = true;
                                        // boss = new Vector2(0, -600);
                                        // aiPlayer.position = new Vector2(0 ,600);

                                        bossHp = 1;
                                    }
                                }
                            }
                            // gameObjects = gameObjects.Where(t => !t.deleted).ToList();

                            //   if (dodgedBullets >= 10)
                            //   {
                            //       aiPlayer.died = true;
                            //       grave.Add(aiPlayer);
                            //   }

                            //   if (Math.Abs(aiPlayer.position.X) >= 600 ||
                            //       Math.Abs(aiPlayer.position.Y) >= 1000)
                            //   {
                            //       aiPlayer.died = true;
                            //       grave.Add(aiPlayer);
                            //   }

                            if (aiPlayer.died)
                            {
                                break;
                            }


                            if (bossMovePhaseTimer > 0)
                            {
                                var dif = aiPlayer.position - boss;
                                boss += 2.8f * Vector2.Normalize(dif);
                                bossAttack = 0;
                                bossMovePhaseTimer -= 16;
                            }
                            else
                            {
                                // phase timer == 0 
                                bossAttack += 16;
                            }

                            if (bossAttack >= 1000)
                            {
                                int a = 0;
                                var gp = gameObjects.FirstOrDefault(t => !t.mine && t.outed);
                                if (gp != null)
                                {
                                    gp.outed = false;
                                    gp.direction = Vector2.Normalize(aiPlayer.position - boss);
                                    gp.position = boss;
                                    bossOnStop = false;
                                    bossAttack = 0;
                                    if (x.Next(10) > 2)
                                    {
                                        bossMovePhaseTimer = 1000;
                                    }
                                }
                                else
                                {
                                    bossMovePhaseTimer = 1000;
                                }
                            }
                        }
                    }

                    Console.WriteLine("GENERATION : " + generationNumber);

                    // cross over
                    // sort 
                    var numberOfPopulation = grave.Count;
                    grave = grave.OrderByDescending(t => t.score).ToList();
                    Console.WriteLine("MAX SCORE : " + grave[0].score);
                    Console.WriteLine("Kills : " + grave[0].Kills);
                    Console.WriteLine("Dodge : " + grave[0].Dosge);
                    Console.WriteLine("Shots : " + grave[0].Shots);

                    var alives = grave.Take(numberOfPopulation / 2).ToList();
                    grave = new List<AIPlayer>();
                    Random r = new Random(IdGenerator.Seed());
                    for (int i = 0; i < alives.Count - 10; i++)
                    {
                        var p1 = r.Next(alives.Count);
                        var p2 = r.Next(alives.Count);
                        while (p1 == p2)
                        {
                            p2 = r.Next(alives.Count);
                        }

                        grave.Add(new AIPlayer(alives[p1], alives[p2]));
                    }

                    // best 1 
                    alives[0].brain.writeToFile();
                    // best 5
                    for (int i = 0; i < 5; i++)
                    {
                        var p1 = alives[i];
                        if (p1.score >= maxScore)
                        {
                            p1.brain.writeToFile(1);
                            Console.WriteLine("writed to file");
                            maxScore = p1.score;
                        }

                        grave.Add(new AIPlayer(p1));
                        grave.Add(new AIPlayer(p1));
                    }

                    grave = grave.Concat(alives).ToList();
                    Console.WriteLine(grave.Count);
                    boss = new Vector2(0, 600);
                    //    boss.Rotate(x.Next(360));
                    foreach (var aiPlayer in grave)
                    {
                        aiPlayer.score = 0;
                        aiPlayer.Kills = 0;
                        aiPlayer.Dosge = 0;
                        aiPlayer.Shots = 0;
                        aiPlayer.died = false;
                        aiPlayer.position = new Vector2(0, -600);
                        aiPlayer.direction = Vector2.Zero;
                        aiPlayer.attackTimer = 10000;
                    }

                    foreach (var obj in gameObjects)
                    {
                        obj.position = Vector2.One * 10000;
                    }

                    players = grave;
                }
            }
        }
    }
}