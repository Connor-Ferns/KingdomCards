using System;
using UnityEngine;

public class GridSystemXY<TGridObject> {


    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public GridPosition gridPosition;
    }


    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridObjectArray;


    public GridSystemXY(int width, int height, float cellSize, Func<GridSystemXY<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridObjectArray = new TGridObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GridPosition gridPosition = new GridPosition(x, y);
                gridObjectArray[x, y] = createGridObject(this, gridPosition);
            }
        }
    }

    public Vector3 GetWorldPosition(GridPosition gridPosition) {
        return new Vector3(gridPosition.x, gridPosition.y, 0) * cellSize;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) {
        return new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.y / cellSize)
        );
    }

    public void CreateDebugObjects(Transform debugPrefab, GameObject gridDebugObjectPrefabParent) {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GridPosition gridPosition = new GridPosition(x, y);

                Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity, gridDebugObjectPrefabParent.transform);
                GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();
                gridDebugObject.SetGridObject(GetGridObject(gridPosition));
            }
        }
    }

    public void CreateDebugLines()
    {
        Vector3 offset = new Vector3(-cellSize, -cellSize) * 0.5f;

        for (int x = 0; x < gridObjectArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridObjectArray.GetLength(1); y++)
            {
                Debug.DrawLine(GetWorldPosition(new GridPosition(x, y)) + offset, GetWorldPosition(new GridPosition(x, y + 1)) + offset, Color.white, 1000f);
                Debug.DrawLine(GetWorldPosition(new GridPosition(x, y)) + offset, GetWorldPosition(new GridPosition(x + 1, y)) + offset, Color.white, 1000f);
            }
        }
        Debug.DrawLine(GetWorldPosition(new GridPosition(0, height)) + offset, GetWorldPosition(new GridPosition(width, height)) + offset, Color.white, 1000f);
        Debug.DrawLine(GetWorldPosition(new GridPosition (width, 0)) + offset, GetWorldPosition(new GridPosition (width, height)) + offset, Color.white, 1000f);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        return gridObjectArray[x, y];
    }

    public TGridObject GetGridObject(GridPosition gridPosition) {
        return gridObjectArray[gridPosition.x, gridPosition.y];
    }

    public TGridObject GetGridObject(Vector3 worldPosition) {
        GridPosition gridPosition = GetGridPosition(worldPosition);
        if (!IsValidGridPosition(gridPosition))
        {
            return default(TGridObject);
        }
        return gridObjectArray[gridPosition.x, gridPosition.y];
    }

    public void TriggerGridObjectChanged(GridPosition gridPosition) {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { gridPosition = gridPosition });
    }

    public bool IsValidGridPosition(Vector3 worldPosition) {
        return IsValidGridPosition(GetGridPosition(worldPosition));
    }

    public bool IsValidGridPosition(GridPosition gridPosition) {
        return gridPosition.x >= 0 &&
                gridPosition.y >= 0 &&
                gridPosition.x < width &&
                gridPosition.y < height;
    }

    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

}