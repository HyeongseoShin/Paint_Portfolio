/*
실제 스팀 Federation 로그인 구현한 스크립트
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using BackEnd;
using System;

public class BackendSteamLogin : MonoBehaviour
{
    private byte[] m_Ticket;
    private uint m_pcbTicket;
    private HAuthTicket m_HAuthTicket;

    private SteamNetworkingIdentity pSteamNetworkingIdentity;

    string sessionTicket = string.Empty;
    
    protected Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;

    [SerializeField]
    private BackEndManager backEndManager;

    [SerializeField]
    private GameObject nickNamePanel;


    // 스팀 세션 티켓 받아오기
    void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback) {
        //Resize to buffer of 1024
        System.Array.Resize(ref m_Ticket, (int)m_pcbTicket);

        //format as Hex 
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (byte b in m_Ticket) sb.AppendFormat("{0:x2}", b);

        sessionTicket = sb.ToString();
        DebugX.Log("Hex encoded ticket: " + sb.ToString());

        RealSteamLogin();
    }

    // 스팀 SDK 관련 Manager Initialize
    public void SteamLoginInitialize() {
        Backend.Initialize(true);

        if (SteamManager.Initialized)
        {
            m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);

            m_Ticket = new byte[1024];
            m_HAuthTicket = SteamUser.GetAuthSessionTicket(m_Ticket, m_Ticket.Length, out m_pcbTicket, ref pSteamNetworkingIdentity);

        }
    }

    // 실제 스팀 로그인
    public void RealSteamLogin() {
        BackendReturnObject bro = Backend.BMember.AuthorizeFederation(sessionTicket, FederationType.Steam);

        if(bro.IsSuccess()) {
            DebugX.Log("Steam 로그인 성공");

            // 24-12-11 로그인 구분 => 스팀 로그인 : loginType = 1
            DataController.Instance.gameData.loginType = 1;

            // 만약 닉네임을 따로 입력할 경우
            if(BackendGameData.Instance.GetUserNickName() == "") {
                nickNamePanel.SetActive(true);
            }
            // 닉네임 이미 존재하면 로그인 진행
            else
            {
                BackendGameData.Instance.SetUserInDate();
                BackendGameData.Instance.SetUserGamerId();
                DebugX.Log("닉네임: " + BackendGameData.Instance.GetUserNickName());
                DebugX.Log("게이머아이디: " + BackendGameData.Instance.GetUserGamerId());
                BackendPlayerManager.Instance.StartPlayerDataLoad();
                BackendSkinManager.Instance.StartPlayerDataLoad();
                BackendAchManager.Instance.StartPlayerDataLoad();
                BackendSaveDataManager.Instance.GetSaveData();

                CSteamID SteamID = SteamUser.GetSteamID();
                string filePath = Application.persistentDataPath + "/" + SteamID + "/SaveData.pa";


                backEndManager.AddPercentage();
            }

        }
        else {
            Debug.LogError("Steam 로그인 실패");
            //실패 처리
        }
    }
}


