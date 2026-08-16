using UnityEngine;

[CreateAssetMenu(fileName = "NewPistol", menuName = EquipmentConstants.PistolMenuName)]
public class Pistol : ScriptableEquipmentBase
{
    private void OnValidate()
    {
        Category = EquipmentCategory.Gun;
    }

    public override void UseEquipment(Transform user, Transform target, WeaponObject weaponContext)
    {
        if (weaponContext == null) return;

        PlayAttackSound(weaponContext.AudioSource);

        Debug.Log($"[Pistol]{user.name} fires at {target.name} using {Name} (mounted on {weaponContext.name}), dealing {damage} damage!");
    }
}