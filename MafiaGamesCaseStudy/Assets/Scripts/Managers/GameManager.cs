using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        [SerializeField] private Canvas onLevelPassedCanvas;
        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this);
        }
        private void OnEnable() 
        {
            ScoreManager.ScoredAllEvent += OnLevelPassed;
        }
        private void OnDisable() 
        {
            ScoreManager.ScoredAllEvent -= OnLevelPassed;
        }
        private void OnLevelPassed()
        {
            onLevelPassedCanvas.gameObject.SetActive(true);
        }
        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}