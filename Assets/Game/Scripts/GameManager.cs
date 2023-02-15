using Cinemachine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{

    [SerializeField]
    private TMP_Text infoText;

    [SerializeField]
    private CinemachineFreeLook playerCam;

    private PhotonView pv;

    public GameObject NowRespawnArea;
    public GameObject TestRespawnArea;

    public LoadScreenScript loadscreen;

    public GetSetGoScript countdown;

    public enum Team { None, Red, Blue };
    public Team team;
    public string myTeam;

    public List<string> Red_TeamList;
    public List<string> Blue_TeamList;

    public int RedTeamNeedGoalPoint = 0;
    public int BlueTeamNeedGoalPoint = 0;

    public bool onGameStart;

    public bool isTestGame;

    public int GoalInCount;

    public string WinTeam;

    public GameObject GameOverSTAGE;

    public List<string> Winner;
    public List<string> Loser;

    Vector3 spawn_1p_Pos, spawn_2p_Pos, spawn_3p_Pos, spawn_4p_Pos;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Start()
    {

        if (PhotonNetwork.InRoom)
        {
            Hashtable props = new Hashtable { { "Load", true } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            //StartCoroutine(DelayGameStart());
            //TestGameStart();
        }

        spawn_1p_Pos = GameObject.Find("Start Point 1P").transform.position;
        spawn_2p_Pos = GameObject.Find("Start Point 2P").transform.position;
        spawn_3p_Pos = GameObject.Find("Start Point 3P").transform.position;
        spawn_4p_Pos = GameObject.Find("Start Point 4P").transform.position;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    pv.RPC("GoalPointChange", RpcTarget.All, "Red", 1);
        //}

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestRespawnPointSet(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestRespawnPointSet(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestRespawnPointSet(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestRespawnPointSet(4);
        }
    }

    public void TestRespawnPointSet(int area)
    {
        GameObject Left_1 = GameObject.Find("RespawnArea1_Left");
        GameObject Left_2 = GameObject.Find("RespawnArea2_Left");
        GameObject Left_3 = GameObject.Find("RespawnArea3");
        GameObject Left_4 = GameObject.Find("RespawnArea4");


        switch (area)
        {
            case 1: TestRespawnArea = Left_1; break;
            case 2: TestRespawnArea = Left_2; break;
            case 3: TestRespawnArea = Left_3; break;
            case 4: TestRespawnArea = Left_4; break;
            default: break;
        }
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("TestRoomSW_0215", new RoomOptions() { MaxPlayers = 4 }, null);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            //StartCoroutine(SpawnStone());
        }
    }

    public override void OnJoinedRoom()
    {
        // 테스트 게임 시작
        StartCoroutine(DelayGameStart());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(string.Format("Disconnected : {0}", cause.ToString()));
        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnLeftRoom()
    {
        Debug.Log(string.Format("OnLeftRoom"));
        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Load"))
        {
            if (AllPlayersLoadLevel())
            {
                GameStart();
                StartCoroutine(AllReady());
            }

            else
            {
                Debug.Log("Waiting others");
                //PrintInfo("Waiting Other Players...");
            }
        }
    }

    IEnumerator AllReady()
    {
        yield return new WaitForSeconds(1f);
        loadscreen.gameObject.GetComponent<PhotonView>().RPC("SetOpenDoor", RpcTarget.All);
        yield return new WaitForSeconds(1f);
        StartCoroutine(CountDown());

    }

    IEnumerator CountDown()
    {
        yield return new WaitForSeconds(1f);
        countdown.gameObject.GetComponent<PhotonView>().RPC("StartAnimation", RpcTarget.All);
        yield return new WaitForSeconds(3f);
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("GameStartSend", RpcTarget.All);
        }
    }


    private void GameStart()
    {
        PrintInfo("");

        int PlayerNum = PhotonNetwork.LocalPlayer.ActorNumber;

        Vector3 position;
        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            switch (PlayerNum)
            {
                case 1: position = spawn_2p_Pos; team = Team.Red; myTeam = "Red"; break;
                case 2: position = spawn_3p_Pos; team = Team.Blue; myTeam = "Blue"; break;
                default: position = spawn_2p_Pos; break;
            }
        }

        else
        {
            switch (PlayerNum)
            {
                case 1: position = spawn_1p_Pos; team = Team.Red; myTeam = "Red"; break;
                case 2: position = spawn_2p_Pos; team = Team.Red; myTeam = "Red"; break;
                case 3: position = spawn_3p_Pos; team = Team.Blue; myTeam = "Blue"; break;
                case 4: position = spawn_4p_Pos; team = Team.Blue; myTeam = "Blue"; break;
                default: position = spawn_1p_Pos; break;
            }
        }

        GameObject Player = PhotonNetwork.Instantiate("Player", position, rotation, 0);
        playerCam.LookAt = Player.transform;
        playerCam.Follow = Player.transform;

        Player.gameObject.GetComponent<PhotonView>().RPC("SetColor", RpcTarget.AllViaServer);
        Player.gameObject.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllViaServer);

        if (team == Team.Red)
        {
            Player.gameObject.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllViaServer, "Red");

        }
        else
        {
            Player.gameObject.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllViaServer, "Blue");
        }

        



    }

    

    private void TestGameStart()
    {
        PrintInfo("");

        int PlayerNum = PhotonNetwork.LocalPlayer.ActorNumber;
        Vector3 position;
        Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            switch (PlayerNum)
            {
                case 1: position = spawn_2p_Pos; team = Team.Red; myTeam = "Red"; break;
                case 2: position = spawn_3p_Pos; team = Team.Blue; myTeam = "Blue"; break;
                default: position = spawn_3p_Pos; break;
            }
        }

        else
        {
            switch (PlayerNum)
            {
                case 1: position = spawn_1p_Pos; team = Team.Red; myTeam = "Red"; break;
                case 2: position = spawn_2p_Pos; team = Team.Red; myTeam = "Red"; break;
                case 3: position = spawn_3p_Pos; team = Team.Blue; myTeam = "Blue"; break;
                case 4: position = spawn_4p_Pos; team = Team.Blue; myTeam = "Blue"; break;
                default: position = spawn_3p_Pos; break;
            }
        }

        GameObject Player = PhotonNetwork.Instantiate("Player", position, rotation, 0);
        playerCam.LookAt = Player.transform;
        playerCam.Follow = Player.transform;

        Player.gameObject.GetComponent<PhotonView>().RPC("SetColor", RpcTarget.AllViaServer);
        Player.gameObject.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllViaServer);

        if (team == Team.Red)
        {
            Player.gameObject.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllViaServer, "Red");

        }
        else
        {
            Player.gameObject.GetComponent<PhotonView>().RPC("SetTeam", RpcTarget.AllViaServer, "Blue");
        }

        if (PhotonNetwork.IsMasterClient)
        {
            isTestGame = true;
            //pv.RPC("GameStartSend", RpcTarget.All);
        }

    }


    private bool AllPlayersLoadLevel()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object playerLoaded;
            if (player.CustomProperties.TryGetValue("Load", out playerLoaded))
            {
                if ((bool)playerLoaded)
                {
                    continue;
                }
                else
                    return false;
            }
            else
                return false;
        }

        return true;
    }

    [PunRPC]
    public void GoalPointChange(string team, int score)
    {
        switch (team)
        {
            case "Red": RedTeamNeedGoalPoint += score; break;
            case "Blue": BlueTeamNeedGoalPoint += score; break;
            default: break;
        }

        DetectGameOver();
    }

    [PunRPC]
    public void AddTeamList(string team, string nick)
    {
        switch (team)
        {
            case "Red": Red_TeamList.Add(nick); break;
            case "Blue": Blue_TeamList.Add(nick); break;
            default: break;
        }

    }

    private void DetectGameOver()
    {
        if (!PhotonNetwork.IsMasterClient || !onGameStart) return;

        if (!isTestGame)
        {
            if (GoalInCount != 0)
            {
                if (RedTeamNeedGoalPoint <= 0)
                {
                    WinTeam = "Red";
                    pv.RPC("GameOver", RpcTarget.All, WinTeam);

                }

                else if (BlueTeamNeedGoalPoint <= 0)
                {
                    WinTeam = "Blue";
                    pv.RPC("GameOver", RpcTarget.All, WinTeam);

                }
            }
        }

        else
        {
            if (RedTeamNeedGoalPoint <= 0)
                pv.RPC("GameOver", RpcTarget.All);
        }

    }

    [PunRPC]
    public void GameStartSend()
    {
        onGameStart = true;
    }

    //public void TeamListADD()
    //{
    //    foreach (Player player in PhotonNetwork.PlayerList)
    //    {
    //        //switch
    //    }

    //    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

    //    foreach (GameObject player in players)
    //    {
    //        if (player.GetComponent<PhotonView>().IsMine)
    //        {
    //            string team = player.GetComponent<PlayerController>().Team;
                
    //            switch (team)
    //            {
    //                case "Red": Red_TeamList()
    //            }

    //            break;
    //        }
    //    }
    //}

    [PunRPC]
    public void GameOver(string team)
    {
        //GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        //foreach (GameObject player in players)
        //{
        //    if (player.GetComponent<PhotonView>().IsMine)
        //    {
        //        player.GetComponent<PlayerController>().SendMyInfo();
        //        break;
        //    }
        //}
        GameOverSTAGE.SetActive(true);
        playerCam.LookAt = GameOverSTAGE.transform;

        PrintInfo("게임 끝");
        Time.timeScale = 0;
        // TODO: 게임오버 로직 ex) 게임오버 씬 이동 or 시상식, etc...
    }

    [PunRPC]
    public void AddWinnerNicknameList(string name)
    {
        Winner.Add(name);
    }

    public void SetCheckPoint(GameObject area)
    {
        NowRespawnArea = area;
    }

    [PunRPC]
    public void CheckGoalIn(int count)
    {
        GoalInCount += count;
        DetectGameOver();
    }


    private void PrintInfo(string info)
    {
        Debug.Log(info);
        infoText.text = info;
    }

    private IEnumerator DelayGameStart()
    {
        yield return new WaitForSeconds(0.1f);
        TestGameStart();
        StartCoroutine(AllReady());
    }

    private IEnumerator SpawnStone()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 5));

            Vector2 direction = Random.insideUnitCircle;
            Vector3 position = Vector3.zero;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Make it appear on the left/right side
                position = new Vector3(Mathf.Sign(direction.x) * Camera.main.orthographicSize * Camera.main.aspect, 0, direction.y * Camera.main.orthographicSize);
            }
            else
            {
                // Make it appear on the top/bottom
                position = new Vector3(direction.x * Camera.main.orthographicSize * Camera.main.aspect, 0, Mathf.Sign(direction.y) * Camera.main.orthographicSize);
            }

            // Offset slightly so we are not out of screen at creation time (as it would destroy the asteroid right away)
            position -= position.normalized * 0.1f;


            Vector3 force = -position.normalized * 1000.0f;
            Vector3 torque = Random.insideUnitSphere * Random.Range(100.0f, 300.0f);
            object[] instantiationData = { force, torque };

            if (Random.Range(0, 10) < 5)
            {
                PhotonNetwork.InstantiateRoomObject("BigStone", position, Quaternion.Euler(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0, instantiationData);
            }
            else
            {
                PhotonNetwork.InstantiateRoomObject("SmallStone", position, Quaternion.Euler(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), 0, instantiationData);
            }
        }
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
