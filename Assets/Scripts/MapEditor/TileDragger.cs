using UnityEngine;
using UnityEngine.EventSystems;
//using Singleplayer;

namespace TileMapEditorScene
{
    public class TileDragger : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public Tile _tilePrefab;
        public Camera mapCamera; // The camera that will be used to convert screen coordinates to world coordinates
        public float tilePreviewSize = 10f; // The size of the tile preview that follows the mouse cursor

        private Tile tilePreview; // The tile preview that follows the mouse cursor while the user is dragging a button
        private Transform tilesParent; // The parent object that will hold all of the tiles on the map

        public float gridSize = 4.0f;

        void Start()
        {
            tilesParent = new GameObject("Tiles Parent").transform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            tilePreview = Instantiate<Tile>(_tilePrefab);
            Vector3 worldPos = mapCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
            tilePreview.transform.position = worldPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 worldPos = mapCamera.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;
            tilePreview.transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector3 worldPos = mapCamera.ScreenToWorldPoint(eventData.position);
            Vector3 normalizedPosition = NormalizePostion(worldPos);
            
            Destroy(tilePreview.gameObject);
            if (TileMapEditor.instance.HasTileInPosition(normalizedPosition))
            {
                return;
            }
            var newTile = Instantiate<Tile>(_tilePrefab, normalizedPosition, Quaternion.identity, tilesParent);
            TileMapEditor.instance.NormalizeTilesInBoard(newTile);
        }
        public Vector3 NormalizePostion(Vector3 oldPosition)
        {
            Vector3 normalizedPosition = new Vector2();
            normalizedPosition.x = Mathf.Round(oldPosition.x / gridSize) * gridSize;
            normalizedPosition.y = Mathf.Round(oldPosition.y / gridSize) * gridSize;
            normalizedPosition.z = 0;
            return normalizedPosition;
        }
    }
}