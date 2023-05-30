using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TileMapEditor.Tiles;

namespace TileMapEditor
{
    public class TileDragger : MonoBehaviour //, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [SerializeField]
        private GameObject m_previewTilePrefab;
        [SerializeField]
        private Camera m_mapCamera;
        [SerializeField]
        private GameObject m_tilesListBackground;
        [SerializeField]
        private GameObject m_sidebarUI;
        
        private GameObject m_tilePreview;
        private float m_gridSize = 4.0f;
        private bool m_isDragging = false;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }

            if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }

            if (m_isDragging)
            {
                DragTile();
            }

            if (Input.GetMouseButtonUp(0))
            {
                PasteTile();
            }
        }

        private void HandleLeftClick()
        {
            // Vector2 mousePosition = Input.mousePosition;
            //if (RectTransformUtility.RectangleContainsScreenPoint(m_sidebarUI.GetComponent<RectTransform>(), mousePosition))
            //{
            //    return;
            //}

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                var tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    MoveTile(tile);
                    return;
                }
            }
            
            SpawnTile();
        }

        private void MoveTile(Tile tile)
        {
            DestroyTile(tile);
            SpawnTile();
        }
        private void DragTile()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            m_tilePreview.transform.position = mousePosition;
        }

        private void DestroyTile(Tile tile)
        {
            TileMapEditorManager.Instance.tileList.Remove(tile);
            TileMapEditorManager.Instance.NormalizeTilesInBoard(tile, true);
            Destroy(tile.gameObject);
        }

        private void HandleRightClick()
        {
            //Vector2 mousePosition = Input.mousePosition;
            //if (RectTransformUtility.RectangleContainsScreenPoint(m_sidebarUI.GetComponent<RectTransform>(), mousePosition))
            //{
            //    return;
            //}
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                var tile = hit.collider.GetComponent<Tile>();
                if (tile != null)
                {
                    DestroyTile(tile);
                }
            }
        }

        private void SpawnTile()
        {
            if (m_previewTilePrefab == null)
            {
                Debug.Log($"Warning: SpawnTile: No m_previewTilePrefab");
                return;
            }
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //if (m_tilePreview != null)
            //{
            //    Destroy(m_tilePreview.gameObject);
            //}
            m_tilePreview = Instantiate(m_previewTilePrefab, mousePosition, Quaternion.identity);
            m_isDragging = true;
        }

        private void PasteTile()
        {
            m_isDragging = false;
            // Vector3 worldPos = m_mapCamera.ScreenToWorldPoint(eventData.position);
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 normalizedPosition = NormalizePosition(mousePosition);
            Destroy(m_tilePreview);
            if (TileMapEditorManager.Instance.HasTileAtPosition(normalizedPosition))
            {
                return;
            }
            TileMapEditorManager.Instance.SpawnInBoard(normalizedPosition);
        }

        private Vector3 NormalizePosition(Vector3 oldPosition)
        {
            Vector3 normalizedPosition = new Vector3();
            normalizedPosition.x = Mathf.Round(oldPosition.x / m_gridSize) * m_gridSize;
            normalizedPosition.y = Mathf.Round(oldPosition.y / m_gridSize) * m_gridSize;
            normalizedPosition.z = 91;
            return normalizedPosition;
        }
    }
}