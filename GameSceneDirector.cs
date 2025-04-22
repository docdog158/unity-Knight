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

    // 宝箱関連
    [SerializeField] PanelTreasureChestController panelTreasureChest;
    [SerializeField] GameObject prefabTreasureChest;
    [SerializeField] List<int> treasureChestItemIds;
    [SerializeField] float treasureChestTimerMin;
    [SerializeField] float treasureChestTimerMax;
    float treasureChestTimer;

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
        panelTreasureChest.Init(this);

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
    
        // 初期値
        treasureChestTimer = Random.Range(treasureChestTimerMin,treasureChestTimerMax);
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームタイマー更新
        updateGameTimer();

        // 宝箱生成
        updateTreasureChestSpawner();
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

    // 宝箱パネル表示
    public void DispPanelTreasureChest()
    {
        // ランダムアイテム
        ItemData item = getRandomItemData();
        // データなし
        if (null == item) return;

        // パネル表示
        panelTreasureChest.DispPanel(item);
        // ゲーム中断
        setEnabled(false);
    }

    // アイテムをランダムで返す
    ItemData getRandomItemData()
    {
        if (1 > treasureChestItemIds.Count) return null;

        // 抽選
        int rnd = Random.Range(0, treasureChestItemIds.Count);
        return ItemSettings.Instance.Get(treasureChestItemIds[rnd]);
    }

    // 宝箱生成
    void updateTreasureChestSpawner()
    {
        // タイマー
        treasureChestTimer -= Time.deltaTime;
        // タイマー未消化
        if (0 < treasureChestTimer) return;

        // 生成場所
        float x = Random.Range(WorldStart.x, WorldEnd.x);
        float y = Random.Range(WorldStart.y, WorldEnd.y);

        // 当たり判定のあるタイル状かどうか
        if (Utils.IsColliderTile(tilemapCollider, new Vector2(x, y))) return;

        // 生成
        GameObject obj = Instantiate(prefabTreasureChest, new Vector3(x, y, 0), Quaternion.identity);
        obj.GetComponent<TreasureChestController>().Init(this);

        // 次のタイマーセット
        treasureChestTimer = Random.Range(treasureChestTimerMin, treasureChestTimerMax);
    }
}
