using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An individual block in the item container panel, contains the item icon and quantity
/// </summary>
public class ItemContainerBlock : MonoBehaviour
{
    [FoldoutGroup("Components"), SerializeField] private UnityEngine.UI.Image itemIcon;
    [FoldoutGroup("Components"), SerializeField] private GameObject quantityTextGroupGO;
    [FoldoutGroup("Components"), SerializeField] private TMPro.TextMeshProUGUI quantityText;
    [FoldoutGroup("Components"), SerializeField] private Button buttonComponent;

    public Button Button { get => buttonComponent; }
    public Item LinkedItem { get; private set; }

    public void SetItem(Item item, int quantity)
    {
        LinkedItem = item;

        itemIcon.sprite = item.icon;

        if (item.stackableItem && quantity > 1)
        {
            quantityTextGroupGO.SetActive(true);
            quantityText.text = quantity.ToString();
        }
        else if (!item.stackableItem && quantity > 1)
        {
            Debug.LogError($"Error found in itemContainerBlock, item is not marked as being stackable, yet has a quantity of {quantity}");
        }
        else
        {
            quantityTextGroupGO.SetActive(false);
        }
    }

    public void ReloadQuantity(int newQuantity)
    {
        if (LinkedItem.stackableItem && newQuantity > 1)
        {
            quantityTextGroupGO.SetActive(true);
            quantityText.text = newQuantity.ToString();
        }
        else if (!LinkedItem.stackableItem && newQuantity > 1)
        {
            Debug.LogError($"Error found in itemContainerBlock, item is not marked as being stackable, yet has a quantity of {newQuantity}");
        }
        else
        {
            quantityTextGroupGO.SetActive(false);
        }
    }
}
