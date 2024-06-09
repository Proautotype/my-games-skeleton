using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{

    public class SortingComparer : IComparer<int>
    {
        private int[] GetCustomOrder(int numPlayers)
        {
            int[] customOrder = new int[numPlayers];
            int index = 0;

            // Create the zigzag pattern
            for (int i = 0; i < numPlayers; i += 2)
            {
                customOrder[index++] = i;
            }
            for (int i = 1; i < numPlayers; i += 2)
            {
                customOrder[index++] = i;
            }

            return customOrder;
        }

        public int Compare(int index1, int index2)
        {
            int[] customOrder = GetCustomOrder(Math.Max(index1, index2) + 1);

            // Get the index of the players' colors in the custom order
            int orderIndex1 = Array.IndexOf(customOrder, index1);
            int orderIndex2 = Array.IndexOf(customOrder, index2);

            // Compare the indices to determine the sorting order
            return orderIndex1.CompareTo(orderIndex2);
        }
    }

    public class PlayerManager
    {
        public List<LuduPlayer> luduPlayers;

        public List<LuduPlayer> LuduPlayers
        {
            get { return luduPlayers; }
            set { luduPlayers = value; }
        }

        public PlayerManager(List<LuduPlayer> players)
        {
            //this.luduPlayers = players;
            AddPlayers(players);
        }

        public void RemovePlayer(LuduPlayer player)
        {
            LuduPlayer found_luduPlayer = luduPlayers.Find((_player) => _player == player);
            found_luduPlayer.Completed = true;
            Debug.Log("printing " + luduPlayers.Count);
            PrintAllayers(luduPlayers);
        }

        public void PrintAllayers(List<LuduPlayer> players)
        {
            players.ForEach((player) =>
            {
                Debug.Log(player.Name + " " + player.Completed + " ");
            });
        }

        public bool RemovePlayer(string player)
        {
            bool result = false;
            LuduPlayer detectedPlayer = GetPlayer(player);
            if ((detectedPlayer) != null)
            {
                result = luduPlayers.Remove(detectedPlayer);
            }
            return result;
        }

        public bool AddPlayer(LuduPlayer player)
        {
            bool result = false;
            if (GetPlayer(player) != null)
            {
                LuduPlayers.Add(player);
                result = true;
            }
            return result;
        }

        public bool AddPlayer(List<LuduPlayer> players)
        {
            this.luduPlayers = new();
            int NO = players.Count;

            // If there are 2 or fewer players, add the new players and return
            if (NO <= 2)
            {
                luduPlayers.AddRange(players);
                return true;
            }

            // If there are 3 players, manually sort them to {0, 2, 1} pattern
            if (NO == 3)
            {
                luduPlayers.Add(players[0]);
                luduPlayers.Add(players[2]);
                luduPlayers.Add(players[1]);
                return true;
            }

            // Sort the players by the custom pattern {0, 2, 1, 3}
            List<LuduPlayer> sortedPlayers = new List<LuduPlayer>();
            for (int i = 0; i < NO; i += 4)
            {
                sortedPlayers.Add(luduPlayers[i]);      // Add player at index 0
                if (i + 2 < NO)
                    sortedPlayers.Add(luduPlayers[i + 2]);  // Add player at index 2
                if (i + 1 < NO)
                    sortedPlayers.Add(luduPlayers[i + 1]);  // Add player at index 1
                if (i + 3 < NO)
                    sortedPlayers.Add(luduPlayers[i + 3]);  // Add player at index 3
            }

            // Clear the original luduPlayers list and add the sorted players
            luduPlayers.Clear();
            luduPlayers.AddRange(sortedPlayers);

            return true;
        }

        public bool AddPlayers(List<LuduPlayer> players)
        {
            this.luduPlayers = players;
            this.luduPlayers.Reverse();
            return true;
        }
        public LuduPlayer GetPlayer(LuduPlayer player)
        {
            LuduPlayer luduPlayer = luduPlayers.Find(lp => lp.Name == player.Name);
            return luduPlayer;
        }

        public LuduPlayer GetPlayer(string player)
        {
            LuduPlayer luduPlayer = luduPlayers.Find(lp => lp.Name == player);
            return luduPlayer;
        }

        public LuduPlayer SequenceChangePlayer()
        {
            LuduPlayer selectedPlayer;
            int currentPlayerIndx = luduPlayers.FindIndex(player => player.Active == true);
            LuduPlayers[currentPlayerIndx].Active = false;
            if (currentPlayerIndx == (luduPlayers.Count - 1))
            {
                selectedPlayer = LuduPlayers[0];
                selectedPlayer.Active = true;
                return selectedPlayer;
            }
            selectedPlayer = LuduPlayers[currentPlayerIndx + 1];
            selectedPlayer.Active = true;
            return selectedPlayer;
        }

        public LuduPlayer SequenceChangePlayer(LuduPlayer current)
        {
            LuduPlayer selectedPlayer = new();
            //get the current player by its color
            int currentPlayerIndx = luduPlayers.FindIndex(player => player.Color == current.Color);
            //deactivate the current player, since we are going to activate another
            this.LuduPlayers[currentPlayerIndx].Active = false;
            if (currentPlayerIndx == LuduPlayers.Count - 1)
            {
                currentPlayerIndx = 0;
            }
            else
            {
                currentPlayerIndx++;
            }
            //go through the list of players from the current player, then select the nearest uncompleted player
            while (this.LuduPlayers[currentPlayerIndx].Completed)
            {

                if (currentPlayerIndx == LuduPlayers.Count - 1)
                {
                    currentPlayerIndx = 0;
                }
                else
                {
                    currentPlayerIndx++;
                }
            }
            return this.LuduPlayers[currentPlayerIndx];
        }

        public int GetActivePlayers()
        {
            int count = 0;
            foreach (LuduPlayer player in luduPlayers)
            {
                if (player.Active == true)
                {
                    count++;
                }
            }
            return count;
        }

        internal LuduPlayer GetPlayer(int selected)
        {
            return this.luduPlayers[selected];
        }

    }


}
