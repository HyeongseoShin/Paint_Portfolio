/*
로그인한 유저의 정보를 저장

UserData 구조체
- 닉네임
- 유저 고유 InDate (데이터에 접근할 때 사용)
- 유저 고유 Id
- 차단 유저 리스트

userData - 서버에서 json형태로 받아온 데이터를 로컬에 저장
*/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BackEnd;

public class UserData {
    public string nickName = string.Empty;
    public string userInDate = string.Empty;
    public string gamerId = string.Empty;
    public List<string> blockedUsers = new List<string>();
}

public class BackendGameData : MonoBehaviour
{

    private static BackendGameData _instance = null;

    public static BackendGameData Instance {
        get {
            if (_instance == null) {
                _instance = new BackendGameData();
            }

            return _instance;
        }
    }

    public static UserData userData;

    private string gameDataRowInDate = string.Empty;

    // 차단 유저 서버에 추가
    public void BlockedUsersInsert() {
        if (userData == null) {
            userData = new UserData();
        }

        DebugX.Log("데이터를 초기화합니다.");
        userData.nickName = Backend.UserNickName;
        
        Param param = new Param();
        param.Add("NickName", userData.nickName);
        param.Add("BlockedUsers", userData.blockedUsers);
        
        var bro = Backend.GameData.Insert("BlockedUsers_List", param);

        if (bro.IsSuccess()) {
            DebugX.Log("게임정보 데이터 삽입에 성공했습니다. : " + bro);
            //삽입한 게임정보의 고유값입니다.
            gameDataRowInDate = bro.GetInDate();
        } else {
            DebugX.LogError("게임정보 데이터 삽입에 실패했습니다. : " + bro);
        }
    }
    
    // 차단 유저 목록 서버에서 가져오기
    public void BlockedUsersGet() {
        
        if(userData.blockedUsers.Count > 0) {
            DebugX.Log("블록 유저 수: " + userData.blockedUsers.Count);
            return ;
        }
        

        // Step 2. 게임정보 불러오기 구현하기
        DebugX.Log("게임 정보 조회 함수를 호출합니다.");
        var bro = Backend.GameData.GetMyData("BlockedUsers_List", new Where());
        if (bro.IsSuccess()) {
            DebugX.Log("게임 정보 조회에 성공했습니다. : " + bro);


            LitJson.JsonData gameDataJson = bro.FlattenRows(); // Json으로 리턴된 데이터를 받아옵니다.

            // 받아온 데이터의 갯수가 0이라면 데이터가 존재하지 않는 것입니다.
            if (gameDataJson.Count <= 0) {
                DebugX.LogWarning("데이터가 존재하지 않습니다.");
                BlockedUsersInsert();
            } else {
                gameDataRowInDate = gameDataJson[0]["inDate"].ToString(); //불러온 게임정보의 고유값입니다.

                //userData = new UserData();

                //userData.nickName = gameDataJson[0]["NickName"].ToString();
            
                foreach (LitJson.JsonData user in gameDataJson[0]["BlockedUsers"]) {
                    userData.blockedUsers.Add(user.ToString());
                }

                DebugX.Log(userData.blockedUsers);
            }
        } else {
            DebugX.LogError("게임 정보 조회에 실패했습니다. : " + bro);
        }
    }
    
    // 차단 유저 목록 서버로 업데이트
    public void BlockedUsersUpdate() {
        // Step 3. 게임정보 수정 구현하기
        if (userData == null) {
            DebugX.LogError("서버에서 다운받거나 새로 삽입한 데이터가 존재하지 않습니다. Insert 혹은 Get을 통해 데이터를 생성해주세요.");
            return;
        }

        Param param = new Param();
        param.Add("nickName", userData.nickName);
        param.Add("BlockedUsers", userData.blockedUsers);

        BackendReturnObject bro = null;
        
        if (string.IsNullOrEmpty(gameDataRowInDate)) {
            DebugX.Log("내 제일 최신 게임정보 데이터 수정을 요청합니다.");

            bro = Backend.GameData.Update("BlockedUsers_List", new Where(), param);
        }

        if (bro.IsSuccess()) {
            DebugX.Log("게임정보 데이터 수정에 성공했습니다. : " + bro);
        } else {
            DebugX.LogError("게임정보 데이터 수정에 실패했습니다. : " + bro);
        }
    }

    // 차단 유저 로컬에 추가
    public bool AddBlockedUser(string nickName) {
        if(userData.blockedUsers.Contains(nickName) == true) {
            return false;
        }
        userData.blockedUsers.Add(nickName);
        DebugX.Log(nickName + " 사용자가 차단되었습니다.");

        return true;
    }

    // 차단 유저 로컬에서 삭제
    public void RemoveBlockedUser(string nickName) {
        userData.blockedUsers.Remove(nickName);
        DebugX.Log(nickName + " 사용자가 차단해제되었습니다.");
        Destroy(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject);
        BlockedUsersUpdate();
    }

    // 현재 닉네임 가져오기
    public string GetUserNickName() {
        if(userData == null) {
            userData = new UserData();
            if(Backend.UserNickName != null) {
                 userData.nickName = Backend.UserNickName;
                DebugX.Log("GetUserNickName: " + userData.nickName);
                DebugX.Log("Backend.UserNickName: " + Backend.UserNickName);
                return userData.nickName;
            }
            else {
                return null;
            }
           
        }
        else if (string.IsNullOrEmpty(userData.nickName)) {
            userData.nickName = Backend.UserNickName;
        }
        return userData.nickName;
    }

    // 닉네임 정하기
    public void SetUserNickName(string nickName) {
        if(userData == null) {
            userData = new UserData();
            userData.nickName = nickName;
        }
        else {
            userData.nickName = nickName;
        }
    }


    // UserInDate 받아오기
    public void SetUserInDate() {
        BackendReturnObject bro = Backend.BMember.GetUserInfo ();
        string inDate = bro.GetReturnValuetoJSON()["row"]["inDate"].ToString();
        if(userData == null) {
            userData = new UserData();
            userData.userInDate = inDate;
        }
        else {
            userData.userInDate = inDate;
        }
    }

    // UserInDate 리턴
    public string GetUserInDate() {
        return userData.userInDate;
    }

    // GamerId 받아오기
    public void SetUserGamerId() {
        BackendReturnObject bro = Backend.BMember.GetUserInfo ();
        string gamerId = bro.GetReturnValuetoJSON()["row"]["gamerId"].ToString();
        if(userData == null) {
            userData = new UserData();
            userData.gamerId = gamerId;
        }
        else {
            userData.gamerId = gamerId;
        }
    }

    // GamerId 리턴
    public string GetUserGamerId() {
        return userData.gamerId;
    }

    // 이미 차단되어 있는 유저인지 확인
    public bool CheckBlockedUsers(string nickName) {
        if(userData.blockedUsers.Contains(nickName)) {
            return true;
        }

        return false;
    }

    // 로컬에서 유저 차단 리스트 가져오기
    public List<string> GetBlockedUsers() {
        return userData.blockedUsers;
    }
}
