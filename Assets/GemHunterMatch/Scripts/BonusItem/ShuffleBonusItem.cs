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

            CheckForMatchesAfterShuffle(allPositions);

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

        // 檢查 shuffle 後的 matches
        private void CheckForMatchesAfterShuffle(List<Vector3Int> shuffledPositions)
        {
            // 使用 Coroutine 延遲檢查，讓 UI 先穩定
            GameManager.Instance.StartCoroutine(DelayedMatchCheck(shuffledPositions));
        }

        // 延遲 match 檢查
        private System.Collections.IEnumerator DelayedMatchCheck(List<Vector3Int> positions)
        {
            // 等待一個 frame 讓位置穩定
            yield return null;

            // 檢查所有 shuffled 位置是否有 match
            foreach (var position in positions)
            {
                if (GameManager.Instance.Board.CellContent.TryGetValue(position, out var cell) &&
                    cell.ContainingGem != null &&
                    cell.ContainingGem.CurrentMatch == null)
                {
                    // 使用 Board 的私有方法檢查 match（我們需要通過反射或其他方式）
                    // 或者直接添加到 match check queue
                    var board = GameManager.Instance.Board;

                    // 使用反射調用私有的 DoCheck 方法
                    var methodInfo = board.GetType().GetMethod("DoCheck",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (methodInfo != null)
                    {
                        methodInfo.Invoke(board, new object[] { position, true });
                    }
                }
            }
        }
    }
}