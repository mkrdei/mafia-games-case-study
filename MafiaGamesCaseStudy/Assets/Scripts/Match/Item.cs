using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Managers;
using Util;
namespace MatchThreeEngine
{
    public class Item : MonoBehaviour
    {
        public ItemTypeAsset itemType;
        private SpriteRenderer spriteRenderer;
        private bool _picked;
        private Tile triggeredTile;
        private Tile currentTile;
        private void Awake() 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        private void Update() 
        {
            spriteRenderer.sprite = itemType.sprite;
            if (_picked)
                DragItem();
        }
        private void OnEnable() 
        {
            InputManager.OnSelect += OnSelect;
            InputManager.OnRelease += OnRelease;
        }
        private void OnDisable() 
        {       
            InputManager.OnSelect -= OnSelect;
            InputManager.OnRelease -= OnRelease;
        }
        public void Reset()
        {
            itemType = null;
            spriteRenderer.sprite = null;
            _picked = false;
            triggeredTile = null;
            currentTile = null;
            transform.parent = null;
        }
        private void DragItem()
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition).With(z: 0);
        }
        private void OnSelect(Transform item)
        {
            if (item == transform)
            {
                if (transform.parent.TryGetComponent<Tile>(out Tile tile))
                    currentTile = tile;
                _picked = true;
            }
        }
        private void OnRelease(Transform item)
        {
            if (item == transform && currentTile != null)
            {
                _picked = false;
                currentTile.Reset();
                currentTile = triggeredTile;
                currentTile.Type = itemType;
                currentTile.item = this;
                transform.parent = currentTile.transform;
                transform.localPosition = Vector3.zero;
            }
        }
        private void OnTriggerEnter(Collider other) 
        {
            if (other.tag == "Tile" && (other.transform.childCount == 0 || other.transform == transform.parent))
            {
                triggeredTile = other.GetComponent<Tile>();
            }
            else
            {
                triggeredTile = transform.parent.GetComponent<Tile>();
            }
        }
    }
}
