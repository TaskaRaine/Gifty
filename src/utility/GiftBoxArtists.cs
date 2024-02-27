using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Config;

namespace Gifty.Utility
{
    public static class GGiftBoxArtists
    {
        public static Dictionary<string, string> GiftboxArtists { get; private set; } = new Dictionary<string, string>()
        {
            { "unknown", Lang.Get("gifty:blockinfo-unknownartist") },
            { "plain", "Taska" },
            { "christmas_ohdeer", "Taska" },
            { "birthday_pawparty", "Taska" },
            { "misc_starrynight", "Taska" },
            { "halloween_pumpgrin", "Taska" },
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
            if(GiftboxArtists.ContainsKey(key))
                return GiftboxArtists[key];
            else return GiftboxArtists["unknown"];
        }
    }
}
