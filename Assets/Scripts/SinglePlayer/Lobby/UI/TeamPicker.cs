using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer.Lobby.UI
{
    public struct TeamData
    {
        public Color TeamColor;
        public string TeamName;

        public TeamData(Color teamColor, string teamName)
        {
            TeamColor = teamColor;
            TeamName = teamName;
        }
    }

    public class TeamPicker : MonoBehaviour
    {
        [SerializeField]
        public List<TeamData> TeamsDataList = new()
        {
            new TeamData(new Color(245 / 255f, 82 / 255f, 82 / 255f), "Red"),
            new TeamData(new Color(59 / 255f, 233 / 255f, 58 / 255f), "Green"),
            new TeamData(new Color(64 / 255f, 131 / 255f, 224 / 255f), "Blue"),
            new TeamData(new Color(241 / 255f, 15 / 255f, 219 / 255f), "Pink"),
        };
    }
}