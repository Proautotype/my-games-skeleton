using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [System.Serializable]
    public class LuduPlayer
    {
        private String name { get; set; }
        private String color { get; set; }
        private bool active { get; set; }
        private string type { get; set; }
        private bool completed { get; set; }

        public PlayerSettings playerSettings;
        public LuduPlayer() {
            playerSettings = new PlayerSettings();
            playerSettings.orderOfDiceDispensary = PlayerSettings.OrderOfDiceDispensary.LR;
        }

        public LuduPlayer(string name, String color, bool active, string type)
        {
            this.name = name;
            this.active = active;
            this.color = color;
            this.type = type;
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public String Color
        {
            get { return color; }
            set { color = value; }
        }

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }
        
        public String Type
        {
            get { return type; }
            set { type = value; }
        }

        public bool Completed
        {
            get { return completed; }
            set { completed = value; }
        }

        public Color GetMaterialColor
        {
            get
            {
                Color color = UnityEngine.Color.white;
                ColorUtility.TryParseHtmlString(this.color, out color);
                return color;
            }
        }

    }
}

[System.Serializable]
public class PlayerSettings
{
    public enum OrderOfDiceDispensary { LR, RL }
    public OrderOfDiceDispensary orderOfDiceDispensary = OrderOfDiceDispensary.LR;
}
