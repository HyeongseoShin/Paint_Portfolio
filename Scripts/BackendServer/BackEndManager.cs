/*
게임 시작 시 다양한 로그인 (커스텀, 스팀)을 통한 계정 연동
- 현재는 스팀 로그인만 진행중
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using BackEnd;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BackEndManager : MonoBehaviour
{
    private LoadingSceneManager lsm;

    private BackendSteamLogin backendSteamLogin;


    void Start() {
        var bro = Backend.Initialize(true); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess()) {
            //성공일 경우 statusCode 204 Success
            DebugX.Log("초기화 성공 : " + bro);

            SteamLogin();
        }
        else {
            // 실패일 경우 statusCode 400대 에러 발생 
            DebugX.Log("초기화 실패 : " + bro);
        }
    }

    

    public void SteamLogin() {
        backendSteamLogin.SteamLoginInitialize();
    }
}
