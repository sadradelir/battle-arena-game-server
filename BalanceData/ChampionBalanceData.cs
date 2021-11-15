using System;

namespace MobarezooServer.BalanceData
{
    public class ChampionBalanceData
    {
        public string name;
        public int aD;
        public int mS;
        public int pS;
        public int hP;
        public int aI;
        public float d1M;
        public float s1M;
        public float sH;
        public float hL;
        public float bV1;
        public float bV2;
        public int x1;
        public int y1;
        public int x2;
        public int y2;
        public int limit1;
        public int limit2;
        public int adpl;
        public int adstar;
        public int adplstar;
        public int hppl;
        public int hpstar;
        public int hpplstar;

        public ChampionBalanceData(string csvRow)
        {
            var splited = csvRow.Split(',');
            this.name = splited[0]; 
            this.aD =  int.Parse( splited[1]); 
            this.mS =  int.Parse( splited[2]); 
            this.pS =  int.Parse( splited[3]); 
            this.hP =  int.Parse( splited[4]); 
            this.aI =  int.Parse( splited[5]); 
            
            this.d1M =  float.Parse( splited[6]); 
            this.s1M =  float.Parse( splited[7]); 
            this.sH =  float.Parse( splited[8]); 
            this.hL =  float.Parse( splited[9]); 
            this.bV1 =  float.Parse( splited[10]); 
            this.bV2 =  float.Parse( splited[11]);
            
            this.x1 =  int.Parse( splited[12]); 
            this.y1 =  int.Parse( splited[13]); 
            this.x2 =  int.Parse( splited[14]); 
            this.y2 =  int.Parse( splited[15]); 
            this.limit1 =  int.Parse( splited[16]); 
            this.limit2 =  int.Parse( splited[17]);
            this.adpl =  int.Parse( splited[18]); 
            this.hppl =  int.Parse( splited[19]); 
            this.hpstar =  int.Parse( splited[20]); 
            this.hpplstar =  int.Parse( splited[21]); 
            this.adstar =  int.Parse( splited[22]); 
            this.adplstar =  int.Parse( splited[23]); 
        }
    }
}