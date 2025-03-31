using System.Collections.Generic;
using UnityEngine;

public class ItemSwitcher : MonoBehaviour
{
    // A list to hold all wardrobe items that can be switched
    [SerializeField] private List<WardrobeItem> items = new();
    [SerializeField] private GameObject _panel;
	[SerializeField] private GameObject _wardrobeButton;

    private void Start()
    {
		PlotsManager.OnPlotActivated += HandlePlotSwitch;
		PlotsManager.OnPlotDeactivated += HandlePlotsDeactivated;
    }

	private void DeactivateWardrobeUI() 
    {
		_wardrobeButton.SetActive(false);
		_panel.SetActive(false);
	}

	private void ActivateWardrobeUI() 
    {
		_wardrobeButton.SetActive(true);
	}

    private void HandlePlotSwitch (Plot newPlot)
    {
		DeactivateWardrobeUI();

        if (newPlot != Plot.Ocean) return;

		DeactivateObjectsOfType(WardrobeItem.ItemType.hat);
        GameObject goggles = FindItemOfType(WardrobeItem.ItemType.goggles);
        goggles.SetActive(true);
        // DeactivateObjectsExcept(WardrobeItem.ItemType.goggles);
    }

	private void HandlePlotsDeactivated (Plot plot) 
    {
		if (plot == Plot.Ocean) 
        {
			GameObject goggles = FindItemOfType(WardrobeItem.ItemType.goggles);
			if (goggles.activeSelf) goggles.SetActive(false);
		}

		ActivateWardrobeUI();
	}

	private void DeactivateObjectsOfType (WardrobeItem.ItemType type)
	{
		foreach (var item in items)
		{
			if (item.itemType != type) continue;
			
			if (item.gameObject.activeSelf) 
            {
				item.gameObject.SetActive(false);
				break;
			}
		}
	}

    private void DeactivateObjectsExcept (WardrobeItem.ItemType type)
    {
        foreach (var item in items)
        {
            if (item.itemType == type) continue;

            item.gameObject.SetActive(false);
        }
    }

    private GameObject FindItemOfType (WardrobeItem.ItemType type) => items.Find(item => item.itemType == type).gameObject;

    // Method to toggle the specified wardrobe item (itemToSwitchOn) on or off
    public void SwitchOn (WardrobeItem itemToSwitchOn)
    {
        // Check if the item to be switched on is of type 'goggles'
        if (itemToSwitchOn.itemType == WardrobeItem.ItemType.goggles)
        {
            // Loop through all wardrobe items
            foreach (var item in items)
            {
                // If the item is not the one being switched on, deactivate it
                if (item != itemToSwitchOn)
                {
                    item.gameObject.SetActive(false);
                }
            }
        } 
        else
        {
            // For items that are not 'goggles'
            foreach (var item in items)
            {
                // Deactivate other items of the same type as the one being switched on
                if (item != itemToSwitchOn && item.itemType == itemToSwitchOn.itemType)
                {
                    item.gameObject.SetActive(false);
                }

                // Ensure that goggles are also deactivated when switching on a different item
                if (item != itemToSwitchOn && item.itemType == WardrobeItem.ItemType.goggles)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        // Toggle the active state of the item being switched on
        if (itemToSwitchOn.gameObject.activeSelf)
        {
            // If the item is already active, deactivate it
            itemToSwitchOn.gameObject.SetActive(false);
        }
        else
        {
            //If the item is inactive, activate it
            itemToSwitchOn.gameObject.SetActive(true);
        }
    }

    public void ActivatePanel()
    {
        _panel.SetActive(true);
    }

    private void OnDestroy()
    {
		PlotsManager.OnPlotActivated -= HandlePlotSwitch;
		PlotsManager.OnPlotDeactivated -= HandlePlotsDeactivated;
    }
}
