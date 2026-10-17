using UnityEngine;

namespace Watermelon
{
    public class CampaignController : MonoBehaviour
    {
        [SerializeField] CampaignDatabase database;
        public CampaignDatabase Database => database;

        public void Init()
        {
            Campaign.Init(database);
        }
    }
}
