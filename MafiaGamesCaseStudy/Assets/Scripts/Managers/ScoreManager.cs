using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
namespace Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager instance;
        [SerializeField] private Transform[] scoreBoards;
        private int[] _scores;
        public static event Action ScoredAllEvent;
        
        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
            _scores = new int[scoreBoards.Length];
        }
        public void IncreaseScore(int index)
        {
            _scores[index] += 1;
            scoreBoards[index].GetComponentInChildren<TextMeshProUGUI>().text = _scores[index] + "/3";
            CheckScore();
        }
        public bool CheckScore()
        {
            bool scoredAll = true;
            foreach (int score in _scores)
            {
                if (score < 3)
                    scoredAll = false;;
            }
            if (scoredAll)
                ScoredAllEvent?.Invoke();
            return scoredAll;
        }
        public Vector2 GetScoreBoardPosition(int index)
        {
            return scoreBoards[index].position;
        }
    }
}