using System.IO;
using TMPro;
using UnityEngine;

namespace Whisperer.Scenes.Level_0
{
    /// <summary>
    /// Manages the User Consent content
    /// If developer wants to share this app, they need to update the content of the consent by pointing
    /// To a consent file
    /// </summary>
    public class ConsentContentManager : MonoBehaviour
    {
        public TextMeshProUGUI textMesh;
        public string fileName = "UserConsent.txt";

        void Start()
        {
            var textAsset = Resources.Load<TextAsset>(fileName);

            if (textAsset != null)
            {
                textMesh.text = textAsset.text;
            }
        }
    }
}
