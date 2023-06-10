using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Singleplayer.Tiles;
using Singleplayer.Player;

namespace Singleplayer
{
    public class AnimationInnerTileControlBoardManager : MonoBehaviour
    {
        [SerializeField]
        private AnimationInnerTile m_animationInnerTilePrefab;

        private List<AnimationInnerTile> m_currentAnimationInnerTiles = new();
        private Coroutine m_onEndAnimInnerTileMovementCoroutine;

        public AnimationInnerTile GetAndAddAnimationInnerTile(InnerTile innerTileFrom, InnerTile innerTileTo, PlayerState currentPlayerState)
        {
            AnimationInnerTile animInnerTile = GetAnimationInnerTile(innerTileFrom, innerTileTo, currentPlayerState);
            m_currentAnimationInnerTiles.Add(animInnerTile);
            return animInnerTile;
        }
        public AnimationInnerTile GetAnimationInnerTile(InnerTile innerTileFrom, InnerTile innerTileTo, PlayerState currentPlayerState)
        {
            AnimationInnerTile animInnerTile = Instantiate(m_animationInnerTilePrefab);
            animInnerTile.FromInnerTile = innerTileFrom;
            animInnerTile.ToInnerTile = innerTileTo;
            animInnerTile.MoveDirection = Directions.GetVector2FromDirection(innerTileFrom.direction);
            animInnerTile.transform.position = innerTileFrom.transform.position;
            animInnerTile.SelectColor = currentPlayerState.PlayerColor;
            animInnerTile.PlayerId = currentPlayerState.ClientId;
            return animInnerTile;
        }

        public List<AnimationInnerTile> GetAnimationInnerTilesList()
        {
            return m_currentAnimationInnerTiles;
        }

        public void StartMoveAll()
        {
            foreach (AnimationInnerTile animInnerTile in m_currentAnimationInnerTiles)
            {
                animInnerTile.StartMove();
            }
            if (m_onEndAnimInnerTileMovementCoroutine != null)
            {
                Debug.Log($"Warning: StartMoveAll: m_onEndAnimInnerTileMovementCoroutine is not null");
                StopCoroutine(m_onEndAnimInnerTileMovementCoroutine);
            }
            m_onEndAnimInnerTileMovementCoroutine = StartCoroutine(TileMovementCoroutine());
        }
        private IEnumerator TileMovementCoroutine()
        {
            float moveTime = 2.0f;
            yield return new WaitForSeconds(moveTime);
            OnAnimationInnerTileMovementEnd();
        }
        private void OnAnimationInnerTileMovementEnd()
        {
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