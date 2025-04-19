using UnityEngine;

namespace curry.InGame
{
    public class InGameBirdsController : MonoBehaviour
    {
        [SerializeField]
        private InGameBirdController[] m_Birds;

        public void Play()
        {
            foreach (var bird in m_Birds)
            {
                bird.Play();
            }
        }
    }
}
