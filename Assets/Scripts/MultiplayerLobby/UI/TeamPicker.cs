using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer.Lobby.UI
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
        public List<TeamData> TeamsDataList = new()
        {
            new TeamData(new Color(245 /255f, 82 / 255f, 82 / 255f), "Red"),
            new TeamData(new Color(59 / 255f, 233 / 255f, 58 / 255f), "Green"),
            new TeamData(new Color(64 / 255f, 131 / 255f, 224 / 255f), "Blue"),
            new TeamData(new Color(241 / 255f, 15 / 255f, 219 / 255f), "Pink"),
        };
        private int m_teamsCount;

        private void Awake()
        {
            m_teamsCount = TeamsDataList.Count;
        }

        public void SelectTeam(int teamIndex)
        {
            if (teamIndex > m_teamsCount - 1) { return; }

            GetComponent<LobbyUIManager>().SetPlayerTeamServerRpc((byte)teamIndex, TeamsDataList[teamIndex].TeamColor);
        }
    }

}