//Interface that defines an entity capable of equipping and using equipment (Equip class)
public interface IEquipable
{   // Key: value used to identify the equipment
    public bool WearEquip(int key, Equip equipPrefab);
    public bool UseEquip(int key);
    public void RemoveEquip(int key);
    public bool IsEquipped(int key);
}
