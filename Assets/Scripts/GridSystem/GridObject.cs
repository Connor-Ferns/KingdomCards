using System;
using UnityEngine;

public class GridObject
{


    public event EventHandler OnValueChanged;


    private GridSystemXY<GridObject> gridSystem;
    private GridPosition gridPosition;
    // Add more fields here to store whatever data you want on each GridObject
    private int value;
    
    private PlacedObject placedObject;

    public GridObject(GridSystemXY<GridObject> gridSystem, GridPosition gridPosition)
    {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
    }

    public override string ToString()
    {
        return gridPosition.ToString() + "; \n" + placedObject;
    }

    public void SetValue(int value)
    {
        this.value = value;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetValue()
    {
        return value;
    }

    public bool CanBuild()
    {
        return placedObject == null;
    }

    public void SetPlacedObject(PlacedObject placedObject)
    {
        this.placedObject = placedObject;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public PlacedObject GetPlacedObject()
    {
        return placedObject;
    }

    public void ClearPlacedObject()
    {
        placedObject = null;
        OnValueChanged?.Invoke(this, EventArgs.Empty);
    }

}