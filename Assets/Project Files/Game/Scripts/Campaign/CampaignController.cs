using UnityEngine;

namespace SquadCommand
{
    public class CampaignController : MonoBehaviour
    {
        [SerializeField] CampaignDatabase database;
        public CampaignDatabase Database => database;

        [SerializeField] DustwaterUIStyle uiStyle;

        public void Init()
        {
            Campaign.Init(database);

            if (uiStyle != null)
            {
                UIMainMenu.IsCampaignHubActive = true;
                DustwaterUI.Create(uiStyle);
            }
        }
    }
}
