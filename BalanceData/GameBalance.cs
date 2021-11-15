using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MobarezooServer.BalanceData
{
    public class GameBalance
    {
        public Dictionary<string, ChampionBalanceData> championsBalance;
        public GameBalance()
        {
            championsBalance = new Dictionary<string, ChampionBalanceData>();
            var lines = File.ReadLines("Balance.csv").ToArray();
            for (int i = 1; i < lines.Length; i++)
            {
                var newChamp = new ChampionBalanceData(lines[i]);
                championsBalance.Add(newChamp.name.ToUpper() , newChamp);
            }
        }
    }
}