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
        public string fileName = "UserConsent";

        void Start()
        {
            var filePath = Path.Combine(Application.dataPath, "Resources", fileName + ".txt");

            if (File.Exists(filePath))
            {
                var content = File.ReadAllText(filePath);
                textMesh.text = content;
            }
        }
    }
}
