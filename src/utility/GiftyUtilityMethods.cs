namespace Gifty.Utility
{
    public static class GiftyUtilityMethods
    {
        public static bool PlayerCanInteractAs(string playerName, string testPart)
        {
            if(playerName == null || testPart == null) 
                return true;

            string[] parts = testPart.Split(' ');

            foreach (string part in parts)
            {
                if (playerName == part)
                    return true;
            }

            return false;
        }
    }
}
