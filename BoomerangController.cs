using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangController : BaseWeapon
{
    // Start is called before the first frame update
    void Start()
    {
        // ランダムな方向に向かって飛ばす
        forward = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        forward.Normalize();

        // 一旦逆に飛ばす
        Vector2 force = new Vector2(-forward.x * stats.MoveSpeed, -forward.y * stats.MoveSpeed);
        rigidbody2d.AddForce(force);
    }

    // Update is called once per frame
    void Update()
    {
        // 回転
        transform.Rotate(new Vector3(0, 0, 5000 * Time.deltaTime));

        // 移動
        rigidbody2d.AddForce(forward * stats.MoveSpeed * Time.deltaTime);
    }

    // トリガーが衝突した時
    private void OnTriggerEnter2D(Collider2D collision)
    {
        attackEnemy(collision);
    }
}
