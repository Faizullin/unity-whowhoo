using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Singleplayer
{
    public class AnimationInnerTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Rigidbody2D _rb;
        public InnerTile FromInnerTile;
        public InnerTile ToInnerTile;

        public float TileSize = 2f;
        public Color SelectColor;
        public Vector3 direction;
        public Vector3 startPosition;

        public int PlayerId;



        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _rb = GetComponent<Rigidbody2D>();
        }

        public void StartMove()
        {
            float moveSpeed = 1f;
            _spriteRenderer.color = SelectColor;
            _rb.velocity = direction * moveSpeed;
        }

        //private void Update()
        //{
        //    if (IsWorking)
        //    {

        //        float elapsedTime = Time.time - startTime;
        //        float percentageComplete = elapsedTime / animationDuration;

        //        transform.position = Vector3.Lerp(startPosition, endPosition, percentageComplete);

        //        if (percentageComplete >= 1.0f)
        //        {
        //            IsWorking = false;
        //            BoardManager boardManager = GameObject.FindObjectOfType<BoardManager>();
        //            boardManager.OnAnimationInnerTileMovementEnd();
        //        }
        //    }
        //}
    }
}

