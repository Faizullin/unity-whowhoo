using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Multiplayer
{
    public class AnimationInnerTile : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        public InnerTile FromInnerTile;
        public InnerTile ToInnerTile;
        [SerializeField] private Rigidbody2D _rb;
        public bool IsWorking = false;


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
            float moveSpeed = 5f;
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
        //            MultiplayerBoardManager.instance.OnAnimationInnerTileMovementEndServerRpc();
        //        }
        //    }
        //}
    }
}
