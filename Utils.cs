using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ゲームで使う共通処理をまとめた便利クラス
public static class Utils

{
    // 秒後を0：00の文字列に変換
    public static string GetTextTimer(float timer)
    {
        int seconds = (int)timer % 60;
        int minutes = (int)timer / 60;
        return minutes.ToString() + ":" + seconds.ToString("00");
    }
}
