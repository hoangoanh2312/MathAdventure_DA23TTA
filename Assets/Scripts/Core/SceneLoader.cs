using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MathAdventure.Core
{
    public sealed class SceneLoader : MonoBehaviour
    {
        public void Load(string sceneName) => StartCoroutine(LoadAsync(sceneName));
        private static IEnumerator LoadAsync(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone) yield return null;
        }
    }
}
