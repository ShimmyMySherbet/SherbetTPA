using Rocket.API;

namespace SherbetTPA.Models
{
    public class TPAConfig : IRocketPluginConfiguration
    {
        public int TPADeleySec = 10;
        public int TPACooldownSec = 10;
        public int TPATimeout = 10;
        public void LoadDefaults()
        {
        }
    }
}