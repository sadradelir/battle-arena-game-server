using System.CodeDom.Compiler;

namespace MobarezooServer.Utilities
{
    public static class IdGenerator
    {
        private static uint _lastId;
        private static int _lastSeed;
        private static object locker;
        private static object locker2;

        static IdGenerator()
        {
            _lastId = 0;
            _lastSeed = 0;
            locker = new object();
            locker2 = new object();
        }

        public static uint Generate()
        {
            lock (locker)
            {
                return ++_lastId;
            }
        }
        public static int Seed()
        {
            lock (locker2)
            {
                return ++_lastSeed;
            }
        }
    }
}