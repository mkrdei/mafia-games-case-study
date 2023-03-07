using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Managers;
using TMPro;
using Util;
namespace MatchThreeEngine
{
	public sealed class Board : MonoBehaviour
	{
		[SerializeField] private ItemTypeAsset[] itemTypes;

		[SerializeField] private Row[] rows;

		[SerializeField] private float tweenDuration;

		[SerializeField] private bool ensureNoStartingMatches;
		
		[SerializeField] private ObjectPool objectPool;

		private List<Tile> matchedNeighbourTiles = new List<Tile>();

		private readonly List<Tile> _selection = new List<Tile>();

		private TileData[,] Matrix
		{
			get
			{
				var width = rows.Max(row => row.tiles.Length);
				var height = rows.Length;

				var data = new TileData[width, height];

				for (var y = 0; y < height; y++)
					for (var x = 0; x < width; x++)
						data[x, y] = GetTile(x, y).Data;

				return data;
			}
		}
		private void Awake()
		{
		}
		private void Start()
		{
			for (var y = 0; y < rows.Length; y++)
			{
				for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
				{
					var tile = GetTile(x, y);

					tile.x = x;
					tile.y = y;
				}
			}
			SpawnRandomItems(7);
		}
		private void OnEnable() 
		{
			InputManager.OnRelease += StartFlood;
		}
		private void OnDisable()
		{
			InputManager.OnRelease -= StartFlood;
		}

		private Tile GetTile(int x, int y) => rows[y].tiles[x];

		private Tile[] GetTiles(IList<TileData> tileData)
		{
			var length = tileData.Count;

			var tiles = new Tile[length];

			for (var i = 0; i < length; i++) tiles[i] = GetTile(tileData[i].X, tileData[i].Y);

			return tiles;
		}
		private async void SetTileItem(Tile tile, ItemTypeAsset itemType)
		{
			tile.Reset();
			GameObject itemObject = objectPool.GetPooledObject(0);
			if (itemObject != null)
			{
				itemObject.transform.parent = tile.transform;
				itemObject.transform.position = tile.transform.position.With(y:3);
				var sequence = DOTween.Sequence();
				sequence.Append(itemObject.transform.DOLocalMove(Vector3.zero, tweenDuration));
				Item item = itemObject.GetComponent<Item>();
				item.itemType = itemType;
				tile.item = item;
				tile.Type = itemType;
				await sequence.Play().AsyncWaitForCompletion();
			}
			InputManager.instance.EnableInput();
			
		}
		private List<Tile> GetRandomTiles()
		{
			var width = rows.Max(row => row.tiles.Length);
			var height = rows.Length;
			Shuffler shuffler = new Shuffler();
			List<Tile> randomTiles;
			randomTiles = GetEmptyTiles();
			shuffler.Shuffle(randomTiles);
			return randomTiles;
		}
		private List<Tile> GetEmptyTiles()
		{
			List<Tile> emptyTiles = new List<Tile>();
			for (var y = 0; y < rows.Length; y++)
			{
				for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
				{
					Tile tile = rows[y].tiles[x];
					if (tile.transform.childCount == 0)
						emptyTiles.Add(tile);
				}
			}
			return emptyTiles;
		}
		private List<ItemTypeAsset> GetActiveItemTypes()
		{
			List<ItemTypeAsset> activeItemTypes = new List<ItemTypeAsset>();
			for (var y = 0; y < rows.Length; y++)
			{
				for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
				{
					Tile tile = rows[y].tiles[x];
					if (tile.transform.childCount > 0)
						activeItemTypes.Add(tile.item.itemType);
				}
			}
			return activeItemTypes;
		}
		private IEnumerator Flood(int x, int y, Item oldItem, Vector3 oldPos)
		{
			WaitForSeconds wait = new WaitForSeconds(0.001f);
			if (x >= 0 && x < 5 && y >= 0 && y < 5)
			{
				yield return wait;
				Tile tile = rows[y].tiles[x];
				Item item = tile.item;
				if(item != null)
					if (item.itemType.id == oldItem.itemType.id)
					{
						matchedNeighbourTiles.Add(tile);
						if (x + 1 < 5 && oldPos.x <= x)
							StartCoroutine(Flood(x + 1, y, item, oldPos)); 
						if (x - 1 >= 0 && oldPos.x >= x)
							StartCoroutine(Flood(x - 1, y, item, oldPos));
						if (y + 1 < 5 && oldPos.y <= y)
							StartCoroutine(Flood(x, y + 1, item, oldPos));
						if (y - 1 >= 0 && oldPos.y >= y)
							StartCoroutine(Flood(x, y - 1, item, oldPos));
						oldPos = new Vector2(x,y);
					}
				else
					yield break;
			}
		}

		private async void StartFlood(Transform itemTransform)
		{
			await Task.Delay(100);
			if (itemTransform != null)
				if (itemTransform.tag == "Item")
				{
					Item item = itemTransform.GetComponent<Item>();
					Transform tileTransform = itemTransform.parent;
					if (tileTransform != null)
					{
						Tile tile = tileTransform.GetComponent<Tile>();
						StartCoroutine(Flood(tile.x, tile.y, item, new Vector2(tile.x, tile.y)));
						await MatchAsync();
					}
				}
		}
		private async Task MatchAsync()
		{
			await Task.Delay(100);
			int matchedCount = matchedNeighbourTiles.Count;
			if (matchedCount > 2)
			{
				InputManager.instance.DisableInput();
				int itemId = matchedNeighbourTiles[0].item.itemType.id;
				var sequence = DOTween.Sequence();;
				foreach(Tile matched in matchedNeighbourTiles)
				{
					sequence = DOTween.Sequence();
					Transform itemTransform = matched.item.transform;
					sequence.Append(itemTransform.DOJump(itemTransform.position, 1f, 3, tweenDuration));
					sequence.Append(itemTransform.DOMove(ScoreManager.instance.GetScoreBoardPosition(itemId),tweenDuration)).AppendCallback(() => 
					{
						matched.item.Reset();
						matched.Reset();
						objectPool.AddPooledObject(itemTransform.gameObject);
					});
					
				}
				await sequence.Play().AsyncWaitForCompletion();		
				ScoreManager.instance.IncreaseScore(itemId);
				SpawnRandomItems(matchedCount);
			}
			matchedNeighbourTiles = new List<Tile>();
			
		}
		private void SpawnRandomItems(int amount)
		{
			List<ItemTypeAsset> activeItemTypes;
			foreach (Tile randomTile in GetRandomTiles().Take(amount))
			{
				bool isConnected = false;
				
				Tile tile = randomTile;

				// Spawn the most frequent item if it's frequency is lower than 3.
				activeItemTypes = GetActiveItemTypes();
				ItemTypeAsset itemType;
				if (activeItemTypes.Count > 0)
				{
					var result = GetMostFrequentPair(activeItemTypes);
					
					if (result.Values.First() < 3)
					{
						itemType = result.Keys.First();
					}
					else
					{
						itemType = itemTypes[Random.Range(0, itemTypes.Length)];
					}
				}
				else
				{
					itemType = itemTypes[Random.Range(0, itemTypes.Length)];
				}

				// Making sure to spawn at a non-matchable tile.
				for (var y = 0; y < rows.Length; y++)
				{
					for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
					{
						Tile neighbourTile = null;
						if (tile.item == null)
						{
							if (tile.x+1 < 5)
							{
								neighbourTile = GetTile(tile.x+1, tile.y);
								if (neighbourTile.item != null)
									if (neighbourTile.item.itemType.id == itemType.id)
										isConnected = true;
							}
							if (tile.x-1 >= 0)
							{
								neighbourTile = GetTile(tile.x-1, tile.y);
								if (neighbourTile.item != null)
									if (neighbourTile.item.itemType.id == itemType.id)
										isConnected = true;
							}
							if (tile.y+1 < 5)
							{
								neighbourTile = GetTile(tile.x, tile.y+1);
								if (neighbourTile.item != null)
									if (neighbourTile.item.itemType.id == itemType.id)
										isConnected = true;
							}
							if (tile.y-1 >= 0)
							{
								neighbourTile = GetTile(tile.x, tile.y-1);
								if (neighbourTile.item != null)
									if (neighbourTile.item.itemType.id == itemType.id)
										isConnected = true;
							}
						}
						else
						{
							isConnected = true;
						}
						if (!isConnected)
							break;
						if (tile.x+1 < 5 && tile.y+1 < 5)
							tile = GetTile(tile.x+1, tile.y);
						else if (tile.x+1 >= 5 && tile.y+1 < 5)
							tile = GetTile(0, tile.y+1);
						else if ((tile.x+1 >= 5 && tile.x+1 >= 5))
							tile = GetTile(0, 0);
					}
					if (!isConnected)
							break;
				}
				if (tile.item != null)
					tile = GetRandomTiles()[0];
				
				SetTileItem(tile, itemType);					
			}
		}
		static Dictionary<ItemTypeAsset, int> GetMostFrequentPair(List<ItemTypeAsset> values)
		{
			var result = new Dictionary<ItemTypeAsset, int>();
			foreach (ItemTypeAsset value in values)
			{
				if (result.TryGetValue(value, out int count))
				{
					result[value] = count + 1;
				}
				else
				{
					result.Add(value, 1);
				}
			}
			var sorted = from pair in result
						orderby pair.Value descending
						select pair;
			result = new Dictionary<ItemTypeAsset, int>();
			result.Add(sorted.First().Key, sorted.First().Value);
			return result;
		}
	}
}
