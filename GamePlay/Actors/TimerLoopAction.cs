using System;
using Newtonsoft.Json;

namespace MobarezooServer.GamePlay.Actors
{
    public class TimerLoopAction
    {
        [JsonIgnore] private Action timerAction;
        [JsonIgnore] private int millisToAct;
        [JsonIgnore] private int timer;
        

        public TimerLoopAction(Action timerAction , int millisToAct = 500)
        {
            this.millisToAct = millisToAct;
            this.timerAction = timerAction;
        }

        public void UpdateTimer(int deltaTime)
        {
            proceedTimer(deltaTime);
        }

        private void proceedTimer(int deltaTime)
        {
            timer += deltaTime;
            if (timer >= millisToAct)
            {
                
                timerAction?.Invoke();
                timer = 0;
            }
        }
    }
}