using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// 洗牌道具 - 隨機重新排列棋盤上所有寶石的位置
    /// Shuffle Bonus Item - Randomly rearranges all gems on the board
    /// </summary>
    [CreateAssetMenu(fileName = "Shuffle Bonus Item", menuName = "2D Match/Bonus Items/Shuffle Item")]
    public class ShuffleBonusItem : BonusItem
    {
        [Header("洗牌設定 / Shuffle Settings")]
        public AudioClip shuffleSound;  // 洗牌音效

        public override void Use(Vector3Int target)
        {
            // 步驟1：收集所有可以移動的寶石
            List<Gem> allGems = new List<Gem>();
            List<Vector3Int> allPositions = new List<Vector3Int>();

            foreach (var kvp in GameManager.Instance.Board.CellContent)
            {
                Vector3Int position = kvp.Key;
                BoardCell cell = kvp.Value;

                // 只處理有寶石且可以移動的格子
                if (cell.ContainingGem != null && cell.CanBeMoved)
                {
                    allGems.Add(cell.ContainingGem);
                    allPositions.Add(position);
                }
            }

            // 步驟2：如果寶石數量太少，就不執行洗牌
            if (allGems.Count < 2)
            {
                Debug.Log("寶石數量太少，無法洗牌 / Not enough gems to shuffle");
                return;
            }

            // 步驟3：使用 Fisher-Yates 演算法洗牌位置陣列
            ShufflePositions(allPositions);

            // 步驟4：將寶石重新分配到新位置
            for (int i = 0; i < allGems.Count; i++)
            {
                Gem gem = allGems[i];
                Vector3Int newPosition = allPositions[i];

                // 清除舊位置的寶石
                GameManager.Instance.Board.CellContent[gem.CurrentIndex].ContainingGem = null;

                // 設定新位置的寶石
                GameManager.Instance.Board.CellContent[newPosition].ContainingGem = gem;

                // 更新寶石的位置資訊和實際座標
                gem.MoveTo(newPosition);
                gem.transform.position = GameManager.Instance.Board.GetCellCenter(newPosition);
            }

            // 步驟5：播放音效
            if (shuffleSound != null)
            {
                GameManager.Instance.PlaySFX(shuffleSound);
            }

            Debug.Log($"洗牌完成！重新排列了 {allGems.Count} 個寶石 / Shuffle completed! Rearranged {allGems.Count} gems");
        }

        /// <summary>
        /// Fisher-Yates 洗牌演算法 - 確保真正的隨機分布
        /// Fisher-Yates shuffle algorithm - ensures truly random distribution
        /// </summary>
        private void ShufflePositions(List<Vector3Int> positions)
        {
            for (int i = positions.Count - 1; i > 0; i--)
            {
                // 從 0 到 i 中隨機選擇一個索引
                int randomIndex = Random.Range(0, i + 1);

                // 交換位置
                Vector3Int temp = positions[i];
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }
        }
    }
}