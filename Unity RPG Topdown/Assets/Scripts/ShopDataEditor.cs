using PlayerShopping;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
public static class ShopDataEditor
{
    static ShopDataEditor()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ResetShopData();
        }
    }

    private static void ResetShopData()
    {
        ShopData[] shopDataObjects = Resources.LoadAll<ShopData>(""); // Load all ShopData assets

        foreach (var shopData in shopDataObjects)
        {
            // Temp
          //  shopData.itemQuantities.Clear();
          //  shopData.items.Clear();
          //  shopData.sellerGold = 10000;
        }

        //Debug.Log("ShopData reset when exiting play mode.");
    }
}
#endif

