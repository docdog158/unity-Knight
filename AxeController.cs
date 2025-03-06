using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeController : BaseWeapon
{
    // Update is called once per frame
    void Update()
    {
        // 回転
        transform.Rotate(new Vector3(0, 0, -1000 * Time.deltaTime));
    }

    // トリガーが衝突した時
    private void OnTriggerEnter2D(Collider2D collision)
    {
        attackEnemy(collision);
    }
}
