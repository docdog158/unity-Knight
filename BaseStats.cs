using System;
using UnityEngine;

//追加ステータス
public enum BonusType
{
    // 定数
    Bounus,
    // %
    Boost,
}

//追加データ
public enum StatsType
{
    Attack,
    Defense,
    MoveSpeed,
    HP,
    MaxHP,
    XP,
    MaxXP,
    PickUpRange,
    AliveTime,
    //武器生産用
    SpawnCount,
    SpawnTimeMin,
    SpawnTimeMax,
}


//追加データタイプ
[Serializable]
public class BonusStats
{
    // 追加タイプ
    public BonusType Type;
    // 追加するプロパティ
    public StatsType Key;
    // 追加する値
    public float Value;
}

// キャラクターと武器で共通のステータス
public class BaseStats
{
    // Inspector で表示されるタイトル
    public string Title;

    public int Id;

    public int Lv;

    public string Name;

    [TextArea] public string Description;

    public float Attack;

    public float Defense;

    public float HP;

    public float MaxHP;

    public float XP;

    public float MaxXP;

    public float MoveSpeed;

    public float PickUpRange;

    public float AliveTime;

    public float this[StatsType key]
    {
        get{
            if(key == StatsType.Attack) return Attack;
            else if (key ==StatsType.Defense) return Defense;
            else if (key ==StatsType.MoveSpeed) return MoveSpeed;
            else if (key ==StatsType.HP) return HP;
            else if (key ==StatsType.MaxHP) return MaxHP;
            else if (key ==StatsType.XP) return XP;
            else if (key ==StatsType.MaxXP) return MaxXP;
            else if (key ==StatsType.PickUpRange) return PickUpRange;
            else if (key ==StatsType.AliveTime) return AliveTime;
            else return 0;
        }

        set{
            if(key == StatsType.Attack) Attack =value;
            else if (key ==StatsType.Defense) Defense =value;
            else if (key ==StatsType.MoveSpeed) MoveSpeed =value;
            else if (key ==StatsType.HP) HP =value;
            else if (key ==StatsType.MaxHP) MaxHP =value;
            else if (key ==StatsType.XP) XP =value;
            else if (key ==StatsType.MaxXP) MaxXP =value;
            else if (key ==StatsType.PickUpRange) PickUpRange =value;
            else if (key ==StatsType.AliveTime) AliveTime =value;
        }
    }

    // ボーナス値を計算
    protected float applyBonus(float currentValue, float value, BonusType type)
    {
        // 固定値を加算
        if(BonusType.Bounus == type)
        {
            return currentValue + value;
        }

        // %で加算
        else if(BonusType.Boost == type)
        {
            return currentValue * (1 + value);
        }

        return currentValue;
    }

    // ボーナス追加
    protected void addBonus(BonusStats bonus)
    {
        float value = applyBonus(this[bonus.Key], bonus.Value, bonus.Type);

        // 最大値があるもの
        if(StatsType.HP == bonus.Key)
        {
            value = Mathf.Clamp(value, 0, MaxHP);
        }
        else if(StatsType.XP == bonus.Key)
        {
            value = Mathf.Clamp(value, 0, MaxXP);
        }

        this[bonus.Key] = value;
    }

    //コピーしたデータを返す
    public BaseStats GetCopy()
    {
        return (BaseStats)MemberwiseClone();
    }
}