using System;

namespace MobarezooServer.Networking
{
    public enum MessageType
    {
        ENTER_ROOM_REQUEST = 1, // can i enter a room which u made for me ?
        JOIN_ROOM = 2, // yes u can ! :) now send me your positions and etc 
        GAME_ACT = 3, // ha !! this is my position get that dear server 
        SNAPSHOT = 4, // my dear client, here is the stat of other players and map!
        ROOM_JOIN_ERROR = 5 , // off sorry u cant join room talk to mammad
        MAKE_ROOM_FOR_THIS_PLAYERS = 6, // i am mammad ! make it 
        ROOM_MADE = 7 // ok mammad here is your room 
    } 
    
    public enum Status
    {
        SUCCESS = 0,
        FAILED = 1,
    }
    // just for the sake of HTTP between me and mammad
  
    public enum GameState
    {
        Normal,
        Hell
    }

    public enum AiLevel
    {
        Beginner=1,
        SemiPro=2,
        Pro=3,
        Legendary=4,
    }
    
    public class UserChampion
    {
        public int ChampionRef { get; set; }
        public int Level { get; set; }
        public int Xp { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int RankScore { get; set; }
        public int Stars { get; set; }
    }
    
    public class PlayerMatchDetails
    {
        public UserChampion UserChamp { get; set; }
        public string PlayerRef { get; set; }
        public int Champion { get; set; }
        public int ChampionLevel { get; set; }
        public int ChampionStars { get; set; }
        public int TotalAttacks { get; set; }
        public int AttacksHit { get; set; }
        public int TotalDamageDealt { get; set; }
        public bool FirstHit { get; set; }
        public int TotalHealing { get; set; }
        public int TotalDamageNeglected { get; set; }
        public bool Win { get; set; }
        public int TotalMovingTime { get; set; }
        public int TotalStopingTime { get; set; }
        public int TotalDamageGet { get; set; }
        public int TotalAttacksGet { get; set; }
        public int TotalAttacksDodged { get; set; }
        public int LargestMultiHit { get; set; }
        public int CurrentMultiHit { get; set; }
        public int LargestMultiDodge { get; set; }
        public int CurrentMultiDodge { get; set; }
        public int ActivatorsActivated { get; set; }
        public int LargestAttackSpree { get; set; }
        public int CurrentAttackSpree { get; set; }
        
        public bool Perfect { get; set; }
        public bool SemiPerfect { get; set; }
        public bool ComeBack { get; set; }
        public bool PotentialComeBack { get; set; }
        public bool Great { get; set; }
        public int TimeSpentInHell { get; set; }

        //ai stuff
        public bool IsAi { get; set; }
        public AiLevel AiLevel { get; set; }
    }
    
    public class Battle
    {
        public int Map{ get; set; }
        public String StringId { get; set; }
        public int Winner { get; set; }
        public int BattleDuration { get; set; }
        public string StartTime { get; set; }
        public GameState FinishedState { get; set; }

        public PlayerMatchDetails Player1Details { get; set; }
        public PlayerMatchDetails Player2Details { get; set; }
    }
}