using System;
using System.Numerics;
using MobarezooServer.GamePlay;
using MobarezooServer.GamePlay.Champions;
using MobarezooServer.GamePlay.EventSystem;
using MobarezooServer.Utilities;
using MobarezooServer.Utilities.Geometry;
using Newtonsoft.Json;

namespace MobarezooServer.Gameplay.GameObjects
{
    public class Projectile : GameObject
    {
        public bool homing;
        public bool orbital;
        public Vector2 orbitPivot;
        public bool onHitEffect;
        public Shape homingTarget;
        public bool piercing;
        public bool wallStopping; // speed = 0 at wall intersecting
        public bool wallPassing;
        public bool hitActedOnce;
        public bool remoteSpeedChange;
        public MathUtils.RefInteger remoteSpeed;
        public int speed;
        public bool enemyHitActing; // farivaz hammer is false when back 
        public int damage;
        public Action onReachTargetAction;
        public Action onHitObstacleAction;
        public Vector2 previousFramePosition;
        public Obstacle lastHitedObstacle;
        public bool onHitObstacleCalled;
        public bool pulling;
        public float orbitalRotation;

        public Projectile() : base()
        {
            previousFramePosition = new Vector2();
        }

        public void FaceProjectileToEnemy(Champion enemy)
        {
            var faceToTarget = Math.Atan2(enemy.hitCircle.position.Y - shape.position.Y
                , enemy.hitCircle.position.X - shape.position.X);
            shape.Rotate((float) ((180 / Math.PI) * faceToTarget));
        }

        public override void UpdateTransform(long deltaTime)
        {
            if (remoteSpeedChange)
            {
                speed = remoteSpeed.value;
            }

            if (speed != 0)
            {
                previousFramePosition = shape.position;
                if (homing)
                {
                    var homingTarget = this.homingTarget.position;
                    var target = MathUtils.MoveTowards(shape.position, homingTarget,
                        (deltaTime / 33.0f) * speed);
                    shape.moveTo(target);
                    if (Vector2.Distance(shape.position, homingTarget) < 2)
                    {
                        // land //
                        speed = 0;
                        homing = false;
                        shape.moveTo(homingTarget);
                        onReachTargetAction?.Invoke();
                    }
                    else
                    {
                        FaceToTarget(homingTarget);
                    }

                }
                else // not homing 
                {
                    if (orbital)
                    {
                        // polar 
                        var homingTarget = this.homingTarget.position;
                        Vector2 delta = shape.position - orbitPivot;
                        delta = delta.Rotate( 0.1f * (deltaTime / 33f) * speed );
                        shape.position = orbitPivot + delta;
                        var faceToTarget = Math.Atan2(homingTarget.Y - shape.position.Y
                            , homingTarget.X - shape.position.X);
                        float degreeBeforeRotate = shape.rotation; 
                        shape.Rotate( 180 + (float) ((180 / Math.PI) * faceToTarget));
                        orbitalRotation += 0.1f * (deltaTime / 33f) * speed ;
                        if (orbitalRotation >= 360 || orbitalRotation <= -360) // langaar
                        {
                            orbital = false;
                            speed = 0;
                            homing = true;
                            orbitalRotation = 0;
                            if (ownerChampion is Langaar l)
                            {
                                l.anchorState = Langaar.AnchorState.PULL;
                                l.disabled = false;
                            }
                        }
                    }
                    else
                    {
                        var tar = new Vector2();
                        tar.X = shape.position.X + (float) ((deltaTime / 33.0f) * (speed) * Math.Cos((Math.PI / 180.0) * (shape.rotation)));
                        tar.Y = shape.position.Y + (float) ((deltaTime / 33.0f) * (speed) * Math.Sin((Math.PI / 180.0) * (shape.rotation)));
                        shape.moveTo(tar);
                    }
                }
            }
        }

        public Vector2 getVectorDirection()
        {
            Vector2 tar = Vector2.Zero;
            tar.X =  (float) Math.Cos((Math.PI / 180.0) * (shape.rotation));
            tar.Y =  (float) Math.Sin((Math.PI / 180.0) * (shape.rotation));
            return tar;
        }
        
        public void FaceToTarget(Vector2 target)
        {
            var faceToTarget = Math.Atan2(target.Y - shape.position.Y
                , target.X - shape.position.X);
            var faceToTargetDegree = (float) ((180 / Math.PI) * faceToTarget);
            shape.Rotate(faceToTargetDegree);
        }


        public override void IntersectingWithObstacle(Obstacle thatObs)
        {
            //Console.WriteLine("intersecting with obstacle");
            if (thatObs.river)
            {
                return;
            }

            lastHitedObstacle = thatObs;
            if (wallStopping)
            {
                speed = 0;
             //   PullOutFromObstacle(thatObs);        
                if (!onHitObstacleCalled)
                {
                    ownerChampion.room.AddEvent(GameEvent.GameEventType.IMPACT , 0 , id);
                    onHitObstacleAction?.Invoke();
                    onHitObstacleCalled = true;
                }
            }
            else if (!wallPassing)
            {
                deleted = true;
            }
        }

        private void PullOutFromObstacle(Obstacle thatObs)
        {
            Vector2 delta = previousFramePosition - shape.position;
            delta = Vector2.Normalize(delta);
            while (thatObs.shape.IsIntersecting(shape))
            {
                shape.position += delta;
            }
        }


        public override void IntersectingWithPlayer(Champion enemy)
        {
            // Console.WriteLine("intersecting with player");
            if (enemyHitActing)
            {
                if (hitActedOnce)
                {
                    return;
                }

                var hit = enemy.GetHit(this);
                if (hit)
                {
                    hitActedOnce = true;
                    if (onHitEffect)
                    {
                        ownerChampion.OnHitEffect(enemy, this);
                    }

                    if (!piercing)
                    {
                        deleted = true;
                    }

                    ownerChampion.room.AddEvent(GameEvent.GameEventType.HIT, damage, enemy.owner.order);
                }
            }
        }
    }
}