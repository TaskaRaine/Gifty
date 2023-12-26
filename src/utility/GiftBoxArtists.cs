using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gifty.Utility
{
    public static class GGiftBoxArtists
    {
        public static Dictionary<string, string> GiftboxArtists { get; private set; } = new Dictionary<string, string>()
        {
            { "plain", "Taska" },
            { "christmas_meatfest", "CaptainOats" },
            { "christmas_orangejam", "CaptainOats" },
            { "christmas_purplejam", "CaptainOats" },
            { "christmas_redjam", "CaptainOats" },
            { "halloween_cloth", "CaptainOats" },
            { "halloween_pixelportal", "CaptainOats" },
            { "halloween_batmoon", "CaptainOats' Brother" }
        };

        public static string GetGiftBoxArtist(string key)
        {
            return GiftboxArtists[key];
        }
    }
}
