using System;

namespace Driftr
{
    public class GameTimer
    {
        private int lastTime = Environment.TickCount;
        private float etime;

        public float GetETime()
        {
            etime = (Environment.TickCount - lastTime) / 1000.0f;
            lastTime = Environment.TickCount;

            return etime;
        }
    }
}

