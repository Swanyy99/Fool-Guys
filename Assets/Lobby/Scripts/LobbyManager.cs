using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public enum Panel { Login, Lobby, Room }

    [SerializeField]
    private LoginPanel loginPanel;
    [SerializeField]
    private RoomPanel roomPanel;
    [SerializeField]
    private LobbyPanel lobbyPanel;
    [SerializeField]
    private CustomizePanel customizePanel;

    public Color PlayerColor;

    private void Start()
    {
        //if (PhotonNetwork.IsConnected) 
        //{
        //    OnConnectedToMaster();
        //}
        if (PhotonNetwork.InRoom)
        {
            //SetActivePanel(Panel.Room);
            ColorGet();
            OnJoinedRoom();
            print("룸 참여");
        }
        else if (PhotonNetwork.InLobby)
        {
            OnJoinedLobby();
        }
        else // 접속 해제 되었을 경우 if (!PhotonNetwork.IsConnected)
        {
            OnDisconnected(DisconnectCause.None);
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log(PhotonNetwork.NetworkingClient.AppVersion);
        }
    }

    public void ColorGet()
    {
        object R, G, B;

        Player player = PhotonNetwork.LocalPlayer;

        if (player.CustomProperties.TryGetValue("R", out R) &&
            player.CustomProperties.TryGetValue("G", out G) &&
            player.CustomProperties.TryGetValue("B", out B))
        {
            PlayerColor = new Color((float)R, (float)G, (float)B);
        }
    }
    public override void OnConnectedToMaster()
    {
        SetActivePanel(Panel.Lobby);
        PhotonNetwork.JoinLobby();
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        SetActivePanel(Panel.Login);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(Panel.Lobby);
        StatePanel.Instance.AddMessage(string.Format("Create room failed with error({0}) : {1}", returnCode, message));
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(Panel.Lobby);
        StatePanel.Instance.AddMessage(string.Format("Join room failed with error({0}) : {1}", returnCode, message));
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        StatePanel.Instance.AddMessage(string.Format("Join random room failed with error({0}) : {1}", returnCode, message));

        StatePanel.Instance.AddMessage("Create room!");

        string roomName = string.Format("Room{0}", Random.Range(1000, 10000));
        RoomOptions options = new RoomOptions() { MaxPlayers = (byte)8, IsVisible = true };
        PhotonNetwork.CreateRoom(roomName, options, null);

        //RoomOptions options = new RoomOptions() { MaxPlayers = (byte)maxPlayer, IsVisible = Visible };
        //PhotonNetwork.CreateRoom(roomName, options, null);
        //createRoomPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        SetActivePanel(Panel.Room);

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "Ready", false },
            { "Load", false },
            { "R", (float)PlayerColor.r},
            { "G", (float)PlayerColor.g},
            { "B", (float)PlayerColor.b}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        roomPanel.RoomNameSet(PhotonNetwork.CurrentRoom.Name, PhotonNetwork.LocalPlayer.NickName);
        roomPanel.UpdateRoomState();

    }

    public override void OnLeftRoom()
    {
        SetActivePanel(Panel.Lobby);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        roomPanel.UpdateRoomState();
        roomPanel.UpdateLocalPlayerPropertiesUpdate();

    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        roomPanel.UpdateRoomState();
        roomPanel.UpdateLocalPlayerPropertiesUpdate();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomPanel.UpdateRoomState();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomPanel.UpdateRoomState();
    }

    public override void OnJoinedLobby()
    {
        SetActivePanel(Panel.Lobby);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        lobbyPanel.ClearRoomListView();

        lobbyPanel.UpdateCachedRoomList(roomList);
        lobbyPanel.UpdateRoomListView();
    }

    public override void OnLeftLobby()
    {
        SetActivePanel(Panel.Login);
    }


    private void SetActivePanel(Panel panel)
    {
        loginPanel?.gameObject?.SetActive(panel == Panel.Login);
        lobbyPanel?.gameObject?.SetActive(panel == Panel.Lobby);
        roomPanel?.gameObject?.SetActive(panel == Panel.Room);
    }

}