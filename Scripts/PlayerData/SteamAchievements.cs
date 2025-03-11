/*
인게임 업적이 달성 되었을 때 스팀 업적도 달성하게 하기 위한 스크립트
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamAchievements : MonoBehaviour
{
    private static SteamAchievements _instance = null;

    public static SteamAchievements Instance {
        get {
            if (_instance == null) {
                _instance = new SteamAchievements();
            }

            return _instance;
        }
    }

    public void UnlockAchievements(string key) {
        SteamUserStats.SetAchievement(key);
        SteamUserStats.StoreStats();

        DebugX.Log("스팀업적 달성 : " + key + " in UnlockAchievements");
    }

    public void LockAchievements(string key)
    {
        SteamUserStats.ClearAchievement(key);
        SteamUserStats.StoreStats();

        DebugX.Log("스팀 업적 Lock: " + key + " in LockAchievements");
    }

}
