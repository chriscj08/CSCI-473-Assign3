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

            //Initialize our combo box for percent of races in server.
            percentage_Server_Selection.DataSource = servers;
            //*****************************
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            classCB.SelectedIndex = -1;
            serverCB1.SelectedIndex = -1;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }       

        /* ReadPlayers()
         * 
         * Use: This method takes the players.txt file and uses its information 
         * to construct our dictionary containing all the Player objects
         * 
         */
        public void ReadPlayers()
        {
            using (StreamReader inFile = new StreamReader("players.txt"))
            {
                string source = inFile.ReadLine(); // remember to "prime the read"
                while (source != null)
                {
                    string[] split = source.Split('\t'); //Split up the line of input

                    //Create a temporary Player object
                    Player tempPlayer = new Player(Convert.ToUInt32(split[0]), split[1], (Race) Convert.ToUInt32(split[2]), (Classes)Convert.ToUInt32(split[3]),
                                                   (Role)Convert.ToUInt32(split[4]), Convert.ToUInt32(split[5]), Convert.ToUInt32(split[6]), Convert.ToUInt32(split[7]));

                    playerDict.Add(tempPlayer.PlayerId, tempPlayer); //adds the temp player to the playerDict
                    
                    source = inFile.ReadLine(); //Read the next line
                }
            }            
        }

        /* ReadGuilds()
         * 
         * Use: This method takes the guilds.txt file and uses its information 
         * to construct our dictionary containing all the Guild objects
         * 
         */
        public void ReadGuilds()
        {
            using (StreamReader inFile = new StreamReader("guilds.txt"))
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

        /* Find_Classes_Click()
         * 
         * Use: This is a windows form event. This method prints to the query textbox
         * all players from a selected server given a certain class.
         * 
         */
        private void Find_Classes_Click(object sender, EventArgs e)
        {
            query.Clear();

            
            if (classCB.SelectedItem == null || serverCB1.SelectedItem == null )
            {
                query.Text += "Please select both criteria (Class and Server Name). ";
                return;
            }

            string textHeader = "All " + (Classes)classCB.SelectedIndex + " from " + (string)serverCB1.SelectedValue + "\r\n";
            query.Text = textHeader;
            query.Text += "----------------------------------------------------------------------------\r\n";
            string textOutput;

            //We are using a List here because they seem to be much easier to query
            List<Player> playerName = new List<Player>();

            //Here we are just adding to the List all players stored in our playerDict dictionary
            foreach (KeyValuePair<uint, Player> playerKvp in playerDict)
            {
                playerName.Add(playerKvp.Value);
            }
            playerName.Sort();

            var playerQuery = from players in playerName
                              where (players.PlayerClass == (Classes)classCB.SelectedIndex) //If we find a player with said class
                              where (guildDict[players.GuildID].ServerName == (string)serverCB1.SelectedValue)//Belonging to said server name
                              select players; //Select that individual

            //With all the players aquired, let's print some information about them
            foreach (var player in playerQuery)
            {
                textOutput = string.Format("{0,-20}", player.ToString());
                textOutput += string.Format("{0,-20}", "(" + player.PlayerClass + "-" + player.PlayerRole + ")");
                textOutput += string.Format("{0,-15}", "Race: " + player.PlayerRace);
                textOutput += string.Format("{0,-15}", "Level: " + player.PlayerLevel);
                textOutput += string.Format("{0,-20}", "<" + guildDict[player.GuildID].GuildName + ">\r\n");

                query.Text += textOutput;
            }
            query.Text += "\r\n";
            query.Text += "END RESULTS\r\n";
            query.Text += "----------------------------------------------------------------------------\r\n";
        }

        /* Race_Percentage_Click()
         * 
         * Use: This is a windows form event. This method displays the percentage of each race given
         * a server name.
         * 
         */
        private void Race_Percentage_Click(object sender, EventArgs e)
        {
            query.Clear();

            if (percentage_Server_Selection.SelectedItem == null)
            {
                query.Text = "Please select a Server Name. ";
                return;
            }
            string textHeader = "Percentage of Each Race from "+ percentage_Server_Selection.SelectedValue + "\r\n";
            query.Text = textHeader;
            query.Text += "----------------------------------------------------------------------------\r\n";
            string textOutput;

            //We are using a List here because they seem to be much easier to query
            List<Player> playerName = new List<Player>(); //Easier to query a list than a dictionary

            //Here we are just adding to the List all players stored in our playerDict dictionary
            foreach (KeyValuePair<uint, Player> playerKvp in playerDict)
            {
                playerName.Add(playerKvp.Value);
            }
            playerName.Sort();

            var serverQuery = from servers in playerName
                              where (guildDict[servers.GuildID].ServerName == (string)percentage_Server_Selection.SelectedValue) //Select players belonging to said server name
                              select servers;
            //Here we rely on the Count() method to return the amount contained in our serverQuery given a certain criteria
            //For each race, compute (members of server name with a given race) DIVIDED BY (total number of players in said server name)
            textOutput = string.Format("Orc: {0,15: 0.00%}\r\n", (double) (serverQuery.Count(element => element.PlayerRace == (Race) 0)) / (serverQuery.Count()));
            textOutput += string.Format("Troll: {0,13: 0.00%}\r\n", (double)(serverQuery.Count(element => element.PlayerRace == (Race) 1)) / (serverQuery.Count()));
            textOutput += string.Format("Tauren: {0,12: 0.00%}\r\n", (double)(serverQuery.Count(element => element.PlayerRace == (Race) 2)) / (serverQuery.Count()));
            textOutput += string.Format("Forsaken: {0,10: 0.00%}\r\n", (double)(serverQuery.Count(element => element.PlayerRace == (Race) 3)) / (serverQuery.Count()));

            query.Text += textOutput;

            query.Text += "\r\n";
            query.Text += "END RESULTS\r\n";
            query.Text += "----------------------------------------------------------------------------\r\n";
        }

        /* Fill_Role_Click()
         * 
         * Use: This method calls our CouldFill method. We use this method
         * to pass to the CouldFill method an argument (in this case a radio button)
         * so that we can determine a certain role player could fulfill but
         * currently aren't.
         * 
         */
        private void Fill_Role_Click(object sender, EventArgs e)
        {
            query.Clear();

            //Call the logic for figuring out the criteria by calling the below function for each button
            CouldFill(tank_Button);
            CouldFill(healer_Button);
            CouldFill(damage_Button);

            if (!tank_Button.Checked && !healer_Button.Checked && !damage_Button.Checked) //If nothing is selected, Print an error
            {
                query.Text = "Please select Tank, Healer, or Damage!";
                return;
            }
        }

        /* CouldFill()
         * 
         * Use: This method takes a radio button object and see if it's
         * checked. If it is, it compares its text value with Tank, Healer,
         * or Damage string values and executes a slightly different code
         * when a condition is satisfied.
         * 
         */
        private void CouldFill (RadioButton rdo)
        {
            if (rdo.Checked) //If the value is not checked, the following code will not execute
            {
                List<Player> playerName = new List<Player>(); //Easier to query a list than a dictionary

                foreach (KeyValuePair<uint, Player> playerKvp in playerDict) //This loop adds dictionary entries to the new List
                {
                    playerName.Add(playerKvp.Value);
                }
                playerName.Sort();

                query.Clear();
                string textHeader = "All Eligible Players not Fulfilling the " + rdo.Text + " Role.\r\n";
                query.Text = textHeader;
                query.Text += "----------------------------------------------------------------------------\r\n";
                string textOutput;

                var playerQuery = from players in playerName
                                  select players;

                if (rdo.Text == "Tank")
                {
                     playerQuery = from players in playerName
                                      where (players.PlayerRole != 0)  //Here we are making sure the Tank role isn't already selected for this player
                                      where (players.PlayerClass == (Classes) 0 || //In this where statement we are only looking at classes with the Tank role
                                             players.PlayerClass == (Classes) 2 || //That is, Warrior, Druid, or Paladin 
                                             players.PlayerClass == (Classes) 6)
                                      select players;
                }

                if (rdo.Text == "Healer")
                {
                     playerQuery = from players in playerName
                                      where (players.PlayerRole != (Role) 1)  //Same logic as above except here we want to see if the Healer role isn't present
                                      where (players.PlayerClass == (Classes) 2 || //Only Druids and Paladins can be healers
                                             players.PlayerClass == (Classes) 6)
                                      select players;
                }

                if (rdo.Text == "Damage")
                {
                     playerQuery = from players in playerName
                                      where (players.PlayerRole != (Role) 2) //Our search is incredibly simple here since any class can be the DPS role
                                      select players;                 //We only need check if they aren't already a DPS role
                }

                //Let's print out what we've found
                foreach (var player in playerQuery)
                {
                    textOutput = string.Format("{0,-20}", player.ToString());
                    textOutput += string.Format("{0,-20}", "(" + player.PlayerClass + "-" + player.PlayerRole + ")");
                    textOutput += string.Format("{0,-20}", "Race: " + player.PlayerRace);
                    textOutput += string.Format("{0,-20}", "Level: " + player.PlayerLevel);
                    textOutput += string.Format("{0,-10}", "<" + guildDict[player.GuildID].GuildName + ">\r\n");

                    query.Text += textOutput;
                }
                query.Text += "\r\n";
                query.Text += "END RESULTS\r\n";
                query.Text += "----------------------------------------------------------------------------\r\n";
            
            }
        }

        /* Percent_Maxlvl_Click()
         * 
         * Use: This determines the percentage of max level players in each guild. This method
         * is the only method that contains a query with a foreach loop
         * 
         */
        private void Percent_Maxlvl_Click(object sender, EventArgs e)
        {
            query.Clear();

            List<Player> playerName = new List<Player>(); //Easier to query a list than a dictionary

            foreach (KeyValuePair<uint, Player> playerKvp in playerDict) //This loop adds dictionary entries to the new List
            {
                playerName.Add(playerKvp.Value);
            }
            

            query.Clear();
            string textHeader = "Percentage of Max Level Players in All Guilds ";
            query.Text = textHeader;
            query.Text += "----------------------------------------------------------------------------\r\n";
            
            //Look at each guild in the guild dictionary
            foreach (KeyValuePair<uint, Guild> guildKvp in guildDict)
            {
                var playerQuery = from players in playerName
                                  where (players.GuildID == guildKvp.Key) //If the player has a matching guild ID with the guild, select that player
                                  select players;

                if (playerQuery.Count() != 0) //We do this check because some guilds have zero players, thus resulting in a divide by zero.
                {
                    query.Text += string.Format("{0,-22}", "<" + guildKvp.Value.GuildName + ">");
                    query.Text += string.Format(": {0,-20: 0.00%}\r\n", (double)(playerQuery.Count(element => element.PlayerLevel == 60)) / (playerQuery.Count()));
                }  
            }

            query.Text += "\r\n";
            query.Text += "END RESULTS\r\n";
            query.Text += "----------------------------------------------------------------------------\r\n";
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
