using System;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private PlayerProjectile[] projectiles;
    public ReadOnlySpan<PlayerProjectile> All => projectiles;

    public PlayerProjectile GetNextInactive()
    {
        for (int i = 0; i < projectiles.Length; i++)
            if (projectiles[i] != null && !projectiles[i].gameObject.activeInHierarchy)
                return projectiles[i];
        return projectiles.Length > 0 ? projectiles[0] : null;
    }
}