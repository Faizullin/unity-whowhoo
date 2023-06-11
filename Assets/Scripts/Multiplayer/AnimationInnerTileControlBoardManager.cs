using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Multiplayer.Tiles;
using Multiplayer.Player;

namespace Multiplayer
{
    public class AnimationInnerTileControlBoardManager : MonoBehaviour
    {
        [SerializeField] 
        private AnimationInnerTile m_animationInnerTilePrefab;

        private List<AnimationInnerTile> m_currentAnimationInnerTiles = new();
        private Coroutine m_onEndAnimInnerTileMovementCoroutine;

        public AnimationInnerTile GetAnimationInnerTile(AnimationInnerTileData animInnerTileData, InnerTile fromInnerTile, InnerTile toInnerTile)
        {
            AnimationInnerTile animInnerTile = Instantiate(m_animationInnerTilePrefab);
            animInnerTile.FromInnerTile = fromInnerTile;
            animInnerTile.ToInnerTile = toInnerTile;
            animInnerTile.MoveDirection = animInnerTileData.MoveDirection;
            animInnerTile.transform.position = animInnerTile.FromInnerTile.transform.position;
            animInnerTile.SelectColor = animInnerTileData.SelectColor;
            animInnerTile.PlayerId = animInnerTileData.PlayerId;
            return animInnerTile;
        }

        public AnimationInnerTile GetAndAddAnimationInnerTile(AnimationInnerTileData animInnerTileData, InnerTile fromInnerTile, InnerTile toInnerTile)
        {
            AnimationInnerTile animInnerTile = GetAnimationInnerTile(animInnerTileData, fromInnerTile, toInnerTile);
            m_currentAnimationInnerTiles.Add(animInnerTile);
            return animInnerTile;
        }
        //public AnimationInnerTile GetAnimationInnerTile(Tile tileFrom, InnerTile innerTileFrom, InnerTile innerTileTo)
        //{
        //    PlayerState currentPlayer = (PlayerState)GetCurrentPlayer();
        //    AnimationInnerTile animInnerTile = Instantiate(m_animationInnerTilePrefab);
        //    animInnerTile.FromInnerTile = innerTileFrom;
        //    animInnerTile.ToInnerTile = innerTileTo;
        //    animInnerTile.MoveDirection = Directions.GetVector2FromDirection(innerTileFrom.direction);
        //    animInnerTile.transform.position = innerTileFrom.transform.position;
        //    animInnerTile.SelectColor = currentPlayer.PlayerColor;
        //    animInnerTile.PlayerId = currentPlayer.ClientId;
        //    return animInnerTile;
        //}

        public AnimationInnerTileData[] GetAnimationInnerTileDataList()
        {
            List<AnimationInnerTileData> animationInnerTileDataList = new();
            PlayerState currentPlayer = (PlayerState)BoardManager.Instance.GetCurrentPlayer();
            foreach (Tile tile in BoardManager.Instance.CurrentWaveSessionTiles)
            {
                var innerTiles = tile.GetInnerTiles();
                var neighbourTiles = tile.GetNeighbourTiles();
                foreach (var innerTile in innerTiles)
                {
                    animationInnerTileDataList.Add(new AnimationInnerTileData(
                        innerTile.Value, neighbourTiles[innerTile.Key].GetInnerTileByDirection(Directions.GetOppositeDirection(innerTile.Key)), currentPlayer.PlayerColor,
                        Directions.GetVector2FromDirection(innerTile.Value.direction), (long)currentPlayer.ClientId
                    ));
                }
            }
            return animationInnerTileDataList.ToArray();
        }

        public void StartMoveAll()
        {
            foreach (AnimationInnerTile animInnerTile in m_currentAnimationInnerTiles)
            {
                animInnerTile.StartMove();
            }
            // Debug.Log($"Created anim inner tiles{m_currentAnimationInnerTiles.Count}");
            if (m_onEndAnimInnerTileMovementCoroutine != null)
            {
                Debug.Log($"Warning: StartMoveAll: m_onEndAnimInnerTileMovementCoroutine is not null");
            }
            //m_onEndAnimInnerTileMovementCoroutine = StartCoroutine(TileMovementCoroutine());
            StartCoroutine(TileMovementCoroutine());
        }
        private IEnumerator TileMovementCoroutine()
        {
            float moveTime = 2.0f;
            yield return new WaitForSeconds(moveTime);
            BoardManager.Instance.OnAnimationInnerTileMovementEnd();
        }

        public void StopMoveAll()
        {

            foreach (AnimationInnerTile animTile in m_currentAnimationInnerTiles)
            {
                animTile.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }

        public void DestroyAll()
        {
            foreach (AnimationInnerTile animTile in m_currentAnimationInnerTiles)
            {
                Destroy(animTile.gameObject);
            }
            m_currentAnimationInnerTiles.Clear();
        }

        public void DestroyAllWithCoroutine()
        {
            if (m_onEndAnimInnerTileMovementCoroutine != null)
            {
                StopCoroutine(m_onEndAnimInnerTileMovementCoroutine);
            }
            if (m_currentAnimationInnerTiles.Count > 0)
            {
                DestroyAll();
            }
        }

        public List<AnimationInnerTile> GetUniqueAnimationInnerTiles()
        {
            List<AnimationInnerTile> activeAnimeInnertTiles = new();
            List<InnerTile> activeInnertTiles = new();
            foreach (AnimationInnerTile animTile in m_currentAnimationInnerTiles)
            {
                if (!activeInnertTiles.Contains(animTile.ToInnerTile))
                {
                    activeInnertTiles.Add(animTile.ToInnerTile);
                    activeAnimeInnertTiles.Add(animTile);
                }
            }
            return activeAnimeInnertTiles;
        }

    }
}