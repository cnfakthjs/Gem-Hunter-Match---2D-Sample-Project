using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    /// <summary>
    /// �~�P�D�� - �H�����s�ƦC�ѽL�W�Ҧ��_�۪���m
    /// Shuffle Bonus Item - Randomly rearranges all gems on the board
    /// </summary>
    [CreateAssetMenu(fileName = "Shuffle Bonus Item", menuName = "2D Match/Bonus Items/Shuffle Item")]
    public class ShuffleBonusItem : BonusItem
    {
        [Header("�~�P�]�w / Shuffle Settings")]
        public AudioClip shuffleSound;  // �~�P����

        public override void Use(Vector3Int target)
        {
            // �B�J1�G�����Ҧ��i�H���ʪ��_��
            List<Gem> allGems = new List<Gem>();
            List<Vector3Int> allPositions = new List<Vector3Int>();

            foreach (var kvp in GameManager.Instance.Board.CellContent)
            {
                Vector3Int position = kvp.Key;
                BoardCell cell = kvp.Value;

                // �u�B�z���_�ۥB�i�H���ʪ���l
                if (cell.ContainingGem != null && cell.CanBeMoved)
                {
                    allGems.Add(cell.ContainingGem);
                    allPositions.Add(position);
                }
            }

            // �B�J2�G�p�G�_�ۼƶq�Ӥ֡A�N������~�P
            if (allGems.Count < 2)
            {
                Debug.Log("�_�ۼƶq�Ӥ֡A�L�k�~�P / Not enough gems to shuffle");
                return;
            }

            // �B�J3�G�ϥ� Fisher-Yates �t��k�~�P��m�}�C
            ShufflePositions(allPositions);

            // �B�J4�G�N�_�ۭ��s���t��s��m
            for (int i = 0; i < allGems.Count; i++)
            {
                Gem gem = allGems[i];
                Vector3Int newPosition = allPositions[i];

                // �M���¦�m���_��
                GameManager.Instance.Board.CellContent[gem.CurrentIndex].ContainingGem = null;

                // �]�w�s��m���_��
                GameManager.Instance.Board.CellContent[newPosition].ContainingGem = gem;

                // ��s�_�۪���m��T�M��ڮy��
                gem.MoveTo(newPosition);
                gem.transform.position = GameManager.Instance.Board.GetCellCenter(newPosition);
            }

            // �B�J5�G���񭵮�
            if (shuffleSound != null)
            {
                GameManager.Instance.PlaySFX(shuffleSound);
            }

            Debug.Log($"�~�P�����I���s�ƦC�F {allGems.Count} ���_�� / Shuffle completed! Rearranged {allGems.Count} gems");
        }

        /// <summary>
        /// Fisher-Yates �~�P�t��k - �T�O�u�����H������
        /// Fisher-Yates shuffle algorithm - ensures truly random distribution
        /// </summary>
        private void ShufflePositions(List<Vector3Int> positions)
        {
            for (int i = positions.Count - 1; i > 0; i--)
            {
                // �q 0 �� i ���H����ܤ@�ӯ���
                int randomIndex = Random.Range(0, i + 1);

                // �洫��m
                Vector3Int temp = positions[i];
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }
        }
    }
}