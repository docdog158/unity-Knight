using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    // 移動とアニメーション
    Rigidbody2D rigidbody2d;
    Animator animator;

    // 後でInitへセットされる
    GameSceneDirector sceneDirector;
    Slider sliderHP;
    Slider sliderXP;

    public CharacterStats Stats;

    // 攻撃のクールダウン
    float attackCoolDownTimer;
    float attackCoolDownTimerMax = 0.5f;

    // 必要XP
    List<int> levelRequirements;
    // 敵生成装置
    EnemySpawnerController enemySpawner;
    // 向き
    public Vector2 Forward;
    // レベルテキスト
    Text textLv;
    // 現在装備中の武器
    public List<BaseWeaponSpawner> WeaponSpawners;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        updateTimer();

        movePlayer();

        moveCamera();

        moveSliderHP();
    }

    // 初期化
    public void Init(GameSceneDirector sceneDirector, EnemySpawnerController enemySpawner,
        CharacterStats characterStats, Text textLv, Slider sliderHP, Slider sliderXP)
    {
        // 変数初期化
        levelRequirements = new List<int>();
        WeaponSpawners = new List<BaseWeaponSpawner>();


        this.sceneDirector = sceneDirector;
        this.enemySpawner = enemySpawner;
        this.Stats = characterStats;
        this.textLv = textLv;
        this.sliderHP = sliderHP;
        this.sliderXP = sliderXP;

        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // プレイヤーの向き
        Forward = Vector2.right;

        // 経験値の閾値リスト
        levelRequirements.Add(0);
        for (int i = 1; i < 1000; i++)
        {
            // 1つ前の閾値
            int prevxp = levelRequirements[i - 1];
            // 41以降はレベル毎に16XPずつ追加
            int addxp = 16;

            // レベル2までレベルアップするのに5XP
            if(i==1)
            {
                addxp = 5;
            }
            else if (20 >= i)
            {
                addxp = 10;
            }
            else if (40 >= i)
            {
                addxp = 13;
            }

            // 必要経験値
            levelRequirements.Add(prevxp + addxp);
        }

        // LV2の必要経験値
        Stats.MaxHP = levelRequirements[1];

        // UI初期化
        setTextLv();
        setSliderHP();
        setSliderXP();

        moveSliderHP();

        // 武器データセット
        foreach (var item in Stats.DefaultWeaponIds)
        {
            addWeaponSpawner(item);
        }
    }

    void movePlayer()
    {
        Vector2 dir = Vector2.zero;
        string trigger = "";

        if(Input.GetKey(KeyCode.UpArrow))
        {
            dir +=Vector2.up;
            trigger = "isUp";
        }

        if(Input.GetKey(KeyCode.DownArrow))
        {
            dir +=Vector2.down;
            trigger = "isDown";
        }

        if(Input.GetKey(KeyCode.RightArrow))
        {
            dir +=Vector2.right;
            trigger = "isRight";
        }

        if(Input.GetKey(KeyCode.LeftArrow))
        {
            dir +=Vector2.left;
            trigger = "isLeft";
        }

        //入力がなければ抜ける
        if (Vector2.zero == dir ) return;

        //プレイヤー移動
        rigidbody2d.position += dir.normalized * Stats.MoveSpeed * Time.deltaTime;

        //アニメーションを再生する
        animator.SetTrigger(trigger);

        //移動範囲制御
        if(rigidbody2d.position.x < sceneDirector.WorldStart.x)
        {
            Vector2 pos = rigidbody2d.position;
            pos.x = sceneDirector.WorldStart.x;
            rigidbody2d.position = pos;
        }
        if(rigidbody2d.position.y < sceneDirector.WorldStart.y)
        {
            Vector2 pos = rigidbody2d.position;
            pos.y = sceneDirector.WorldStart.y;
            rigidbody2d.position = pos;
        }

        if(sceneDirector.WorldEnd.x < rigidbody2d.position.x)
        {
            Vector2 pos = rigidbody2d.position;
            pos.x = sceneDirector.WorldEnd.x;
            rigidbody2d.position = pos;
        }
        if(sceneDirector.WorldEnd.y < rigidbody2d.position.y)
        {
            Vector2 pos = rigidbody2d.position;
            pos.y = sceneDirector.WorldEnd.y;
            rigidbody2d.position = pos;
        }
    }

    // カメラ移動
    void moveCamera()
    {
        Vector3 pos = transform.position;
        pos.z = Camera.main.transform.position.z;
        if(pos.x < sceneDirector.TileMapStart.x)
        {
            pos.x = sceneDirector.TileMapStart.x;
        }
        if(pos.y < sceneDirector.TileMapStart.y)
        {
            pos.y = sceneDirector.TileMapStart.y;
        }

        if(sceneDirector.TileMapEnd.x < pos.x)
        {
            pos.x = sceneDirector.TileMapEnd.x;
        }
        if(sceneDirector.TileMapEnd.y < pos.y)
        {
            pos.y = sceneDirector.TileMapEnd.y;
        }

    // カメラの位置を更新する
    Camera.main.transform.position = pos;
    }

    // HPスライダー移動
    void moveSliderHP()
    {
        // ワールド座標をスクリーン座標に変換
        Vector3 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
        pos.y -= 50;
        sliderHP.transform.position = pos;
    }

    // ダメージ
    public void Damage(float attack)
    {
        // 非アクティブならばぬける
        if (!enabled) return;

        float damage = Mathf.Max(0, attack - Stats.Defense);
        Stats.HP -= damage;

        //ダメージ表示
        sceneDirector.DispDamage(gameObject, damage);

        // ToDo ゲームオーバー
        if(0 >Stats.HP)
        {

        }

        if (0 > Stats.HP) Stats.HP = 0;
        setSliderHP();
    }

    // HPスライダーの値を変更
    void setSliderHP()
    {
        sliderHP.maxValue = Stats.MaxHP;
        sliderHP.value = Stats.HP;
    }

    // XPスライダーの値を変更
    void setSliderXP()
    {
        sliderXP.maxValue = Stats.MaxXP;
        sliderXP.value = Stats.XP;
    }

    // 衝突したとき
    private void OnCollisionEnter2D(Collision2D collision)
    {
        attackEnemy(collision);
    }

    // 衝突している間
    private void OnCollisionStay2D(Collision2D collision)
    {
        attackEnemy(collision);
    }

    // 衝突が終わった時
    private void OnCollisionExit2D(Collision2D collision)
    {

    }

    // プレイヤーへ攻撃する
    void attackEnemy(Collision2D collision)
    {
        // プレイヤー以外
        if (!collision.gameObject.TryGetComponent<EnemyController>(out var enemy)) return;

        // タイマー未消化
        if (0 < attackCoolDownTimer) return;

        enemy.Damage(Stats.Attack);
        attackCoolDownTimer = attackCoolDownTimerMax;
    }

    // 各種タイマー更新
    void updateTimer()
    {
        if (0 < attackCoolDownTimer)
        {
            attackCoolDownTimer -= Time.deltaTime;
        }

    }

    // レベルテキスト更新
    void setTextLv()
    {
        textLv.text = "LV " + Stats.Lv;
    }

    // 武器を追加
    void addWeaponSpawner(int id)
    {
        // TODO 装備済みならレベルアップ
        BaseWeaponSpawner spawner = WeaponSpawners.Find(item => item.Stats.Id == id);

        if(spawner)
        {
            return;
        }

        // 新規追加
        spawner = WeaponSpawnerSettings.Instance.CreateWeaponSpawner(id, enemySpawner, transform);

        if(null == spawner)
        {
            Debug.LogError("武器データがありません");
            return;
        }

        // 装備済みリストへ追加
        WeaponSpawners.Add(spawner);
    }
}

