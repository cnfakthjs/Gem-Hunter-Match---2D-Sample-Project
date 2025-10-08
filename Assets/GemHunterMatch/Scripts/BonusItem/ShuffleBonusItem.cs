using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "Shuffle Bonus Item", menuName = "2D Match/Bonus Items/Shuffle Item")]
    public class ShuffleBonusItem : BonusItem
    {
        [Header("洗牌設定 / Shuffle Settings")]
        public AudioClip shuffleSound;

        [Header("動畫設定 / Animation Settings")]
        [Tooltip("寶石飛起的高度")]
        public float liftHeight = 3f;
        [Tooltip("飛起所需時間（秒）")]
        public float liftDuration = 0.3f;
        [Tooltip("落下所需時間（秒）")]
        public float fallDuration = 0.6f;
        [Tooltip("飛起的動畫曲線")]
        public AnimationCurve liftCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("落下的動畫曲線")]
        public AnimationCurve fallCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public override void Use(Vector3Int target)
        {
            GameManager.Instance.StartCoroutine(ShuffleWithAnimation());
        }

        private IEnumerator ShuffleWithAnimation()
        {
            GameManager.Instance.Board.ToggleInput(false);

            if (shuffleSound != null)
            {
                GameManager.Instance.PlaySFX(shuffleSound);
            }

            // === 步驟1：收集寶石 ===
            List<Gem> allGems = new List<Gem>();
            List<Vector3Int> allPositions = new List<Vector3Int>();
            Dictionary<Gem, Vector3> originalPositions = new Dictionary<Gem, Vector3>();

            foreach (var kvp in GameManager.Instance.Board.CellContent)
            {
                Vector3Int position = kvp.Key;
                BoardCell cell = kvp.Value;

                if (cell.ContainingGem != null && cell.CanBeMoved)
                {
                    Gem gem = cell.ContainingGem;
                    allGems.Add(gem);
                    allPositions.Add(position);
                    originalPositions[gem] = gem.transform.position;
                }
            }

            if (allGems.Count < 2)
            {
                Debug.Log("寶石數量太少，無法洗牌");
                GameManager.Instance.Board.ToggleInput(true);
                yield break;
            }

            // === 步驟2：飛起動畫 ===
            float elapsedTime = 0f;

            while (elapsedTime < liftDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / liftDuration);
                float curveValue = (liftCurve != null && liftCurve.length > 0)
                    ? liftCurve.Evaluate(t)
                    : t;

                foreach (var gem in allGems)
                {
                    if (gem != null && originalPositions.ContainsKey(gem))
                    {
                        Vector3 startPos = originalPositions[gem];
                        Vector3 targetPos = startPos + Vector3.up * liftHeight;
                        gem.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
                        gem.transform.rotation = Quaternion.Euler(0, 0, curveValue * 180f);
                    }
                }

                yield return null;
            }

            // === 步驟3：洗牌並重新分配 ===
            ShufflePositions(allPositions);

            // 先清除所有舊位置
            foreach (var gem in allGems)
            {
                GameManager.Instance.Board.CellContent[gem.CurrentIndex].ContainingGem = null;
            }

            // 再設定新位置
            for (int i = 0; i < allGems.Count; i++)
            {
                Gem gem = allGems[i];
                Vector3Int newPosition = allPositions[i];

                GameManager.Instance.Board.CellContent[newPosition].ContainingGem = gem;
                gem.MoveTo(newPosition);
            }

            // === 步驟4：落下動畫 ===
            elapsedTime = 0f;
            Dictionary<Gem, Vector3> liftedPositions = new Dictionary<Gem, Vector3>();
            Dictionary<Gem, Vector3> targetPositions = new Dictionary<Gem, Vector3>();

            foreach (var gem in allGems)
            {
                if (gem != null)
                {
                    liftedPositions[gem] = gem.transform.position;
                    targetPositions[gem] = GameManager.Instance.Board.GetCellCenter(gem.CurrentIndex);
                }
            }

            while (elapsedTime < fallDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fallDuration);
                float curveValue = (fallCurve != null && fallCurve.length > 0)
                    ? fallCurve.Evaluate(t)
                    : t;

                foreach (var gem in allGems)
                {
                    if (gem != null && liftedPositions.ContainsKey(gem) && targetPositions.ContainsKey(gem))
                    {
                        gem.transform.position = Vector3.Lerp(liftedPositions[gem], targetPositions[gem], curveValue);
                        gem.transform.rotation = Quaternion.Lerp(gem.transform.rotation, Quaternion.identity, curveValue);
                    }
                }

                yield return null;
            }

            // === 步驟5：確保到位並觸發匹配檢查 ===
            foreach (var gem in allGems)
            {
                if (gem != null && targetPositions.ContainsKey(gem))
                {
                    gem.transform.position = targetPositions[gem];
                    gem.transform.rotation = Quaternion.identity;
                }
            }

            // 觸發所有位置的匹配檢查
            foreach (var gem in allGems)
            {
                if (gem != null)
                {
                    GameManager.Instance.Board.CheckMatchAtPosition(gem.CurrentIndex);
                }
            }

            // 等一幀讓匹配系統處理
            yield return null;

            GameManager.Instance.Board.ToggleInput(true);
            Debug.Log($"洗牌完成！重新排列了 {allGems.Count} 個寶石");
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