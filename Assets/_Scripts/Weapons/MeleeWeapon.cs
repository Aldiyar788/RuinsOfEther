using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

/// <summary>
/// Оружие ближнего боя.
/// Реализует атаку через сферу вокруг точки удара.
/// </summary>
public class MeleeWeapon : WeaponBase
{
    [Header("Параметры ближней атаки")]
    [Tooltip("Точка, откуда считается удар (обычно у меча/руки).")]
    [SerializeField]
    private Transform attackOrigin;

    [Tooltip("Радиус удара. Если 0, можно использовать Range из WeaponData.")]
    [SerializeField]
    private float hitRadius = 1.5f;

    [Tooltip("Слои, по которым можно наносить урон (враги, разрушаемые объекты).")]
    [SerializeField]
    private LayerMask hitLayers;

    public override void Attack()
    {
        if (!CanAttack())
            return;

        StartAttackCooldown();
        // ... остальной код атаки
    }

    private void OnDrawGizmosSelected()
    {
        // Рисуем сферу удара в редакторе, чтобы видеть радиус
        Gizmos.color = Color.red;

        float radius = hitRadius > 0f ? hitRadius : (WeaponData != null ? WeaponData.range : 1.5f);
        Vector3 origin = attackOrigin != null
            ? attackOrigin.position
            : (Owner != null ? Owner.position : transform.position);

        Gizmos.DrawWireSphere(origin, radius);
    }
}