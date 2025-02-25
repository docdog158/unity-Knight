using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidbody2d;
    Animator animator;
    float moveSpeed = 10;
    // Start is called before the first frame update

    [SerializeField] GameSceneDirector sceneDirector;
    [SerializeField] Slider sliderHP;
    [SerializeField] Slider sliderXP;

    public CharacterStats Stats;

    // 攻撃のクールダウン
    float attackCoolDownTimer;
    float attackCoolDownTimerMax = 0.5f;

    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        updateTimer();

        movePlayer();

        moveCamera();

        moveSliderHP();
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
        rigidbody2d.position += dir.normalized * moveSpeed * Time.deltaTime;

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

        //HPスライダーの値を変更
        void setSliderHP()
        {
            sliderHP.maxValue = Stats.MaxHP;
            sliderHP.value = Stats.HP;
        }

        //XPスライダーの値を変更
        //void setSliderXP()
        //{
        //    sliderXP.maxValue = Stats.MaxXP;
        //    sliderXP.value = Stats.XP;
        //}
    }

    // 衝突したとき
    private void OnCollisionEnter2D(Collision2D collision)
    {
        attackEnemy(collision);
    }

    // 衝突している間
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    attackPlayer(collision);
    //}

    // 衝突が終わった時
    //private void OnCollisionEnter2D(Collision2D collision)


    // プレイヤーへ攻撃する
    void attackEnemy(Collision2D collision)
    {
        // プレイヤー以外
        if (!collision.gameObject.TryGetComponent<EnemyController>(out var enemy)) return;

        // タイマー未消化
        if (0 <attackCoolDownTimer) return;

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
}

