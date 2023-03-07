using UnityEngine;

namespace MatchThreeEngine
{
	[CreateAssetMenu(menuName = "Match 3 Engine/Item Type Asset")]
	public sealed class ItemTypeAsset : ScriptableObject
	{
		public int id;
		public Sprite sprite;
	}
}
