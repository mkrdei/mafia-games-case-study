using UnityEngine;
using UnityEngine.UI;

namespace MatchThreeEngine
{
	public sealed class Tile : MonoBehaviour
	{
		public int x;
		public int y;
		[SerializeField] public Item item;

		[SerializeField] private ItemTypeAsset _type;

		public ItemTypeAsset Type
		{
			get => _type;

			set
			{
				if (_type == value) return;

				_type = value;
			}
		}
		public void Reset()
		{
			item = null;
			_type = null;
		}
		
			public TileData Data => new TileData(x, y, _type != null ? _type.id : -100);
	}
}
