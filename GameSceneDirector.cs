using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameSceneDirector : MonoBehaviour
{
    // タイルマップ
    [SerializeField] GameObject grid;
    [SerializeField] Tilemap tilemapCollider;
    //　マップ全体座標
    public Vector2 TileMapStart;
    public Vector2 TileMapEnd;
    public Vector2 WorldStart;
    public Vector2 WorldEnd;

    public PlayerController Player;

    [SerializeField] Transform parentTextDamage;
    [SerializeField] GameObject prefabTextDamage;

    // タイマー
    [SerializeField] Text textTimer;
    public float GameTimer;
    public float OldSeconds;

    // 敵生成
    [SerializeField] EnemySpawnerController enemySpawner;

    // プレイヤー生成
    [SerializeField] Slider sliderXP;
    [SerializeField] Slider sliderHP;
    [SerializeField] Text textLv;

    // 経験値
    [SerializeField] List<GameObject> prefabXP;

    // レベルアップパネル
    [SerializeField] PanelLevelUpController panelLevelUp;

    void Start()
    {
        // プレイヤー作成
        int playerId = 0;
        Player = CharacterSettings.Instance.CreatePlayer(playerId, this,enemySpawner,
        textLv, sliderHP, sliderXP);

        // 初期設定
        OldSeconds = -1;
        enemySpawner.Init(this, tilemapCollider);
        panelLevelUp.Init(this);


        //カメラの移動できる範囲
        // GetComponentInChildren　指定したオブジェクトの子オブジェクト（<>内）を取得できる
        foreach (Transform item in grid.GetComponentInChildren<Transform>())
        {
            //左下のポジションの取得
            if(TileMapStart.x > item.position.x)
            {
                TileMapStart.x = item.position.x;
            }
            if(TileMapStart.y > item.position.y)
            {
                TileMapStart.y = item.position.y;
            }

            //右上のポジションの取得
            if(TileMapEnd.x < item.position.x)
            {
                TileMapEnd.x = item.position.x;
            }
            if(TileMapEnd.y < item.position.y)
            {
                TileMapEnd.y = item.position.y;
            }
        }

        float cameraSize = Camera.main.orthographicSize;
        float aspect = (float)Screen.width / (float)Screen.height;
        WorldStart = new Vector2(TileMapStart.x - cameraSize * aspect, TileMapStart.y -cameraSize);
        WorldEnd = new Vector2(TileMapEnd.x + cameraSize * aspect, TileMapEnd.y + cameraSize);
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームタイマー更新
        updateGameTimer();
    }

    // ダメージ表示
    public void DispDamage(GameObject target, float damage)
    {
        GameObject obj = Instantiate(prefabTextDamage, parentTextDamage);
        obj.GetComponent<TextDamageController>().Init(target, damage);
    }

    // ゲームタイマー
    void updateGameTimer()
    {
        GameTimer += Time.deltaTime;

    // 前回と秒数が同じなら処理をしない
        int seconds = (int)GameTimer % 60;
        if (seconds == OldSeconds) return;

        textTimer.text = Utils.GetTextTimer(GameTimer);
        OldSeconds = seconds;
    }

    // 経験値取得
    public void CreateXP(EnemyController enemy)
    {
        float xp = Random.Range(enemy.Stats.XP, enemy.Stats.MaxXP);
        if (0 > xp) return;

        // 5未満
        GameObject prefab = prefabXP[0];

        // 10以上
        if (10 <= xp)
        {
            prefab = prefabXP[2];
        }
        // 5以上
        else if (5 <= xp)
        {
            prefab = prefabXP[1];
        }

        // 初期化
        GameObject obj = Instantiate(prefab, enemy.transform.position, Quaternion.identity);
        XPController ctrl = obj.GetComponent<XPController>();
        ctrl.Init(this, xp);
    }

    // ゲーム再開/停止
    void setEnabled(bool enabled=true)
    {
        this.enabled = enabled;
        Time.timeScale = (enabled) ? 1 : 0;
        Player.SetEnabled(enabled);
    }

    // ゲーム再開
    public void PlayGame(BonusData bonusData = null)
    {
        // アイテム追加
        Player.AddBonusData(bonusData);
        // TODO ステータス反映

        // ゲーム再開
        setEnabled();
    }

    // レベルアップ時
    public void DispPanelLevelUp()
    {
        // 追加したアイテム
        List<WeaponSpawnerStats> items = new List<WeaponSpawnerStats>();

        // 生成する
        int randomCount= panelLevelUp.GetButtonCount();
        // 武器の数が足りない場合は減らす
        int listCount = Player.GetUsableWeaponIds().Count;

        if(listCount < randomCount)
        {
            randomCount = listCount;
        }

        // ボーナスをランダムで生成
        for (int i = 0; i< randomCount; i++)
        {
            // 装備可能な武器からランダム
            WeaponSpawnerStats randomItem = Player.GetRandomSpawnerStats();
            // データなし
            if (null == randomItem) continue;

            // かぶりチェック
            WeaponSpawnerStats findItem
                = items.Find(item => item.Id == randomItem.Id);

            // かぶり無し
            if(null == findItem)
            {
                items.Add(randomItem);
            }
            else
            {
                i--;
            }
        }

        // レベルアップパネル表示
        panelLevelUp.DispPanel(items);
        // ゲーム停止
        setEnabled(false);
    }
}
