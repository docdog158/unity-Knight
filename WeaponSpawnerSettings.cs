using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 右クリックメニューに表示する、filenameはデフォルトのファイル名
[CreateAssetMenu(fileName = "WeaponSpawnerSettings", menuName = "ScriptableObjects/WeaponSpawnerSettings")]
public class WeaponSpawnerSettings : ScriptableObject
{
    public List<WeaponSpawnerStats> datas;

    static WeaponSpawnerSettings instance;
    public static WeaponSpawnerSettings Instance
    {
        get
        {
            if (!instance)
            {
                instance = Resources.Load<WeaponSpawnerSettings>(nameof(WeaponSpawnerSettings));
            }

            return instance;
        }
    }

    // リストのIDからデータを検索する
    public WeaponSpawnerStats Get(int id, int lv)
    {
        // 指定されたレベルのデータがなければ一番高いレベルのデータを返す
        WeaponSpawnerStats ret = null;

        foreach (var item in datas)
        {
            if (id != item.Id) continue;

            // 指定レベルと一致
            if ( lv == item.Lv)
            {
                return (WeaponSpawnerStats)item.GetCopy();
            }

            // 仮のデータがセットされていないか、それを超えるレベルがあったら入れ換える
            if(null == ret)
            {
                ret = item;
            }
            // 探してレベル以下であり、暫定データより大きい
            else if(item.Lv<lv && ret.LevelUpItemId < item.Lv)
            {
                ret = item;
            }
        }
        return (WeaponSpawnerStats)ret.GetCopy();
    }

    // 作成
    public BaseWeaponSpawner CreateWeaponSpawner(int id, EnemySpawnerController enemySpawner, Transform parent = null)
    {
        // データ取得
        WeaponSpawnerStats stats = Instance.Get(id, 1);
        // オブジェクト作成
        GameObject obj = Instantiate(stats.PrefabSpawner, parent);
        // データセット
        BaseWeaponSpawner spawner = obj.GetComponent<BaseWeaponSpawner>();
        spawner.Init(enemySpawner, stats);

        return spawner;
    }

}

// 武器生成装置
[System.Serializable]
public class WeaponSpawnerStats : BaseStats
{
    // 生成装置のプレハブ
    public GameObject PrefabSpawner;
    // 武器のアイコン
    public Sprite Icon;
    // レベルアップ時に追加されるアイテムID
    public int LevelUpItemId;

    // 一度に生成する数
    public float SpawnCount;
    // 生成タイマー
    public float SpawnTimerMin;
    public float SpawnTimerMax;

    // 生成時間取得
    public float GetRandomSpawnTimer()
    {
        return Random.Range(SpawnTimerMin, SpawnTimerMax);
    }

    // TODO アイテムを追加
}