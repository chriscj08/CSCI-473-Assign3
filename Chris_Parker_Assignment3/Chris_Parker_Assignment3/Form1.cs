using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chris_Parker_Assignment3
{
    public enum Race { Orc, Troll, Tauren, Forsaken };
    public enum Classes{ Warrior, Mage, Druid, Priest, Warlock, Rogue, Paladin, Hunter, Shaman};
    public enum Role { Tank, Healer, Damage };
    public enum GuildType { Casual, Questing, Mythic, Raiding, PVP};

    public partial class Form1 : Form
    {
        public static Dictionary<uint, Player> playerDict = new Dictionary<uint, Player>();
        public static Dictionary<uint, Guild> guildDict = new Dictionary<uint, Guild>();

        public Form1()
        {
            InitializeComponent();

            //Fill the playerDict and guildDict with values
            ReadPlayers();
            ReadGuilds();

            //Initialize our combo boxes for search "All classes from a single server
            classCB.DataSource = Enum.GetNames(typeof(Classes)); //Populates the class combobox with the enum Classes's values
            string[] servers = { "Beta4Azeroth", "TKWasASetback", "ZappyBoi" }; 
            serverCB1.DataSource = servers; 
            //*****************************


           
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Find_Players_Click(object sender, EventArgs e)
        {
            query.Clear();

            List<Player> playerName = new List<Player>();
           
            foreach (KeyValuePair<uint,Player> playerKvp in playerDict)
            {
                playerName.Add(playerKvp.Value);
            }

            var playerQuery = from players in playerName
                              where (players.PlayerClass == (Classes) classCB.SelectedIndex)
                              where (guildDict[players.GuildID].ServerName == (string) serverCB1.SelectedValue)
                              select players;

            foreach (var player in playerQuery)
            {
                query.Text += player.ToString() + "     " +player.PlayerClass + guildDict[player.GuildID].GuildName + " " + guildDict[player.GuildID].ServerName + "\n";
            }
        }

        public void ReadPlayers()
        {
            using (StreamReader inFile = new StreamReader("C:/Users/Chrips/Downloads/players.txt"))
            {
                string source = inFile.ReadLine(); // remember to "prime the read"
                while (source != null)
                {
                    string[] split = source.Split('\t'); //Split up the line of input

                    //Create a temporary Player object
                    Player tempPlayer = new Player(Convert.ToUInt32(split[0]), split[1], (Race)Convert.ToUInt32(split[2]), (Classes)Convert.ToUInt32(split[3]),
                                                   (Role)Convert.ToUInt32(split[4]), Convert.ToUInt32(split[5]), Convert.ToUInt32(split[6]), Convert.ToUInt32(split[7]));

                    playerDict.Add(tempPlayer.PlayerId, tempPlayer); //adds the temp player to the playerDict
                    
                    source = inFile.ReadLine(); //Read the next line
                }
            }
        }

        public void ReadGuilds()
        {
            using (StreamReader inFile = new StreamReader("C:/Users/Chrips/Downloads/guilds.txt"))
            {
                string source = inFile.ReadLine(); // remember to "prime the read"
                while (source != null)
                {
                    string[] splitTabs = source.Split('\t');
                    string[] splitHyphen = splitTabs[2].Split('-');
                    Guild tempGuild = new Guild(Convert.ToUInt32(splitTabs[0]), (GuildType)Convert.ToUInt32(splitTabs[1]), splitHyphen[0], splitHyphen[1]);
                    guildDict.Add(tempGuild.GuildID, tempGuild);
                    
                    source = inFile.ReadLine();
                }
            }
        }

    }



    public class Player : IComparable
    {
        //Constants
        private static uint MAX_ILVL = 360; //Max item level
        private static uint MAX_PRIMARY = 200; // Max primary stat
        private static uint MAX_STAMINA = 275; //Max stamina
        private static uint MAX_LEVEL = 60; //max player level

        //Data members
        private readonly uint playerId; //Player ID
        private readonly string playerName; //Player Name
        private readonly Race playerRace; //Player Race. int because will be indexed to enum
        private readonly Classes playerClass;
        private readonly Role playerRole;
        private uint playerLevel; //Player level
        private uint exp; //player xp
        private uint guildID; //Player GuildID

        //Default Player class constructor
        public Player()
        {
            this.playerId = 0;
            this.playerName = "";
            this.playerRace = 0;
            this.playerClass = 0;
            this.playerRole = 0;
            this.playerLevel = 0;
            this.exp = 0;
            this.guildID = 0;

        }

        //Player class contrsuctor
        public Player(uint playerId, string playerName, Race playerRace, Classes playerClass, Role playerRole, uint playerLevel, uint exp, uint guildID)
        {
            this.playerId = playerId;
            this.playerName = playerName;
            this.playerRace = playerRace;
            this.playerClass = playerClass;
            this.playerRole = playerRole;
            this.playerLevel = playerLevel;
            this.exp = exp;
            this.guildID = guildID;

        }

        //get for PlayerID no set since readonly
        public uint PlayerId
        {
            get { return playerId; }
        }

        //get for playerName no set since readonly
        public string PlayerName
        {
            get { return playerName; }
        }

        //get for race using race enum. No set since readonly
        public Race PlayerRace
        {
            get { return playerRace; }
        }

        //get for race using race enum. No set since readonly
        public Classes PlayerClass
        {
            get { return playerClass; }
        }

        //get for race using race enum. No set since readonly
        public Role PlayerRole
        {
            get { return playerRole; }
        }

        //get set for player level. Makes sure player level is within allowed range 0-60
        public uint PlayerLevel
        {
            get { return playerLevel; }
            set
            {
                if (value <= 0)
                {
                    playerLevel = 0;
                }
                else if (value >= MAX_LEVEL)
                {
                    playerLevel = MAX_LEVEL;
                }
                else
                {
                    playerLevel = value;
                }
            }
        }

        //get and set for exp. Handles the level up process
        public uint Exp
        {
            get { return exp; }
            set
            {
                exp += value; //adds passed in value to current exp
                int num_of_levelups = 0; //counter to track level ups

                while (exp > PlayerLevel * 1000 && PlayerLevel < MAX_LEVEL)//loop handling leveling up
                {
                    exp -= PlayerLevel * 1000;//subracts current exp level requirement from total
                    this.PlayerLevel++;//adds a level to playerLevel
                    num_of_levelups++;//increments level up counter
                }
                if (num_of_levelups > 1)//informs the user how many level ups were granted
                    Console.WriteLine("Congratulations! You went up " + num_of_levelups + " levels!");
                else if (num_of_levelups == 1)
                    Console.WriteLine("Gongratulations! You leveled up!");

                if (PlayerLevel == 60)//informs user max level has been achieved
                {
                    Console.WriteLine("You are at the max level! Congrats!");
                }
            }

        }

        //get and set for guildID
        public uint GuildID
        {
            get { return guildID; }
            set { guildID = value; }
        }

        //Player compare to method
        public int CompareTo(Object alpha)
        {
            if (alpha == null) //makes sure alpha has a value
            { return 1; }

            Player comp = alpha as Player; // assigns alpha to comp

            if (comp != null)
                return playerName.CompareTo(comp.playerName); //compares players
            else
                throw new ArgumentException("[Item]:CompareTo argument is not an Item.");
        }

        /*Player ToString method
         * We had an issue with the 3rd player not having a tab after its name in any text file we downloaded. This caused the spacing to be screwed up
         * for the 3rd player. We fixed it by having seperate outputs for that player alone
         * */
        public override string ToString()
        {
            return "Name: " + PlayerName;
        }
    }

    public class Guild
    {
        private readonly uint guildID;
        private readonly GuildType guildType;
        private string guildName;
        private string serverName;

        //Default Constructor
        public Guild ()
        {
            this.guildID = 0;
            this.guildType = 0;
            this.guildName = string.Empty;
            this.serverName = string.Empty;
        }

        public Guild( uint guildID, GuildType guildType, string guildName, string serverName)
        {
            this.guildID = guildID;
            this.guildType = guildType;
            this.guildName = guildName;
            this.serverName = serverName;
        }

        public uint GuildID {  get { return guildID; } }
        public GuildType GuildType { get { return guildType; } }
        public string GuildName { get { return guildName; } }
        public string ServerName { get { return serverName; } }

    }
    }
