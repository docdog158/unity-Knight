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

    [SerializeField] List<GameObject> prefabXP;

    void Start()
    {
        // プレイヤー作成
        int playerId = 0;
        Player = CharacterSettings.Instance.CreatePlayer(playerId, this,enemySpawner,
        textLv, sliderHP, sliderXP);

        // 初期設定
        OldSeconds = -1;
        enemySpawner.Init(this, tilemapCollider);


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
}
