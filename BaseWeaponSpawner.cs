using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseWeaponSpawner : MonoBehaviour
{
    // 武器のプレハブ
    [SerializeField] GameObject PrefabWeapon;

    // 武器データ
    public WeaponSpawnerStats Stats;

    // 与えた総ダメージ
    public float TotalDamage;
    // 稼働タイマー
    public float TotalTimer;

    // 生成タイマー
    protected float spawnTimer;
    // 生成した武器のリスト
    protected List<BaseWeapon> weapons;
    // 敵生成装置
    protected EnemySpawnerController enemySpawner;

    // 初期化
    public void Init(EnemySpawnerController enemySpawner, WeaponSpawnerStats stats)
    {
        // 変数初期化
        weapons = new List<BaseWeapon>();
        this.enemySpawner = enemySpawner;
        this.Stats = stats;
    }

    // 稼働タイマー
    private void FixedUpdate()
    {
        TotalTimer += Time.fixedDeltaTime;
    }

    // 武器生成

    protected BaseWeapon createWeapon(Vector3 position, Vector2 forward, Transform parent = null)
    {
        // 生成
        GameObject obj = Instantiate(PrefabWeapon, position, PrefabWeapon.transform.rotation, parent);
        // 共通データセット
        BaseWeapon weapon = obj.GetComponent<BaseWeapon>();
        // データ初期化
        weapon.Init(this, forward);
        // 武器リストへ追加
        weapons.Add(weapon);

        return weapon;
    }

    // 武器生成（簡易版
    protected BaseWeapon createWeapon(Vector3 position, Transform parent = null)
    {
        return createWeapon(position, Vector2.zero, parent);
    }

    // 武器のアップデートを停止する

    public void SetEnabled(bool enabled = true)
    {
        this.enabled = enabled;
        // オブジェクトを削除
        weapons.RemoveAll(item => !item);
        // 生成した武器を停止
        foreach (var item in weapons)
        {
            item.enabled = enabled;
            // Rigidbody停止
            item.GetComponent<Rigidbody2D>().simulated = enabled;
        }
    }

    // タイマー消化チェック
    protected bool isSpawnTimerNotElapsed()
    {
        // タイマー消化
        spawnTimer -= Time.deltaTime;
        if (0 < spawnTimer) return true;

        return false;
    }

    // TODO レベルアップ時のデータを返す
}
