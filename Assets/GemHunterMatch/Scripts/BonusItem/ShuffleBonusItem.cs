using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "Shuffle Bonus Item", menuName = "2D Match/Bonus Items/Shuffle Item")]
    public class ShuffleBonusItem : BonusItem
    {
        [Header("洗牌設定 / Shuffle Settings")]
        public AudioClip shuffleSound;

        public override void Use(Vector3Int target)
        {
            // 步驟1：收集所有可以移動的寶石和位置
            List<Gem> allGems = new List<Gem>();
            List<Vector3Int> allPositions = new List<Vector3Int>();

            foreach (var kvp in GameManager.Instance.Board.CellContent)
            {
                Vector3Int position = kvp.Key;
                BoardCell cell = kvp.Value;

                // 只處理有寶石且可以移動的格子，且不在 match 中
                if (cell.ContainingGem != null && cell.CanBeMoved &&
                    cell.ContainingGem.CurrentMatch == null)
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

            // 步驟4a：先清除所有舊位置的寶石引用
            for (int i = 0; i < allGems.Count; i++)
            {
                Gem gem = allGems[i];
                GameManager.Instance.Board.CellContent[gem.CurrentIndex].ContainingGem = null;
            }

            // 步驟4b：再設定所有新位置
            for (int i = 0; i < allGems.Count; i++)
            {
                Gem gem = allGems[i];
                Vector3Int newPosition = allPositions[i];

                // 設定新位置的寶石
                GameManager.Instance.Board.CellContent[newPosition].ContainingGem = gem;

                // 更新寶石的位置資訊和實際座標
                gem.MoveTo(newPosition);
                gem.transform.position = GameManager.Instance.Board.GetCellCenter(newPosition);

                // 重要：確保寶石狀態正確
                gem.StopBouncing();
                if (gem.CurrentState == Gem.State.Falling)
                {
                    gem.StopFalling();
                }
            }

            // 步驟5：播放音效
            if (shuffleSound != null)
            {
                GameManager.Instance.PlaySFX(shuffleSound);
            }

            Debug.Log($"洗牌完成！重新排列了 {allGems.Count} 個寶石 / Shuffle completed! Rearranged {allGems.Count} gems");
        }

        private void ShufflePositions(List<Vector3Int> positions)
        {
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                Vector3Int temp = positions[i];
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }
        }
    }
}