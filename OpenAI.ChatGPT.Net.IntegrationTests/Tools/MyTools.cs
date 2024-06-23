namespace OpenAI.ChatGPT.Net.IntegrationTests.Tools
{
    /// <summary>
    /// Class containing tools must be public
    /// </summary>
    public class MyTools
    {
        /// <summary>
        /// Example of a simple tool that returns the current time in a specific timezone.
        /// </summary>
        /// <param name="timeZoneOffset"></param>
        /// <param name="use24h"></param>
        /// <returns></returns>
        public static string GetTime(int timeZoneOffset, bool use24h)
        {
            var time = DateTime.UtcNow.AddHours(timeZoneOffset);
            return use24h ? time.ToString("HH:mm") : time.ToString("hh:mm tt");
        }        
        
        // Overload with different JSON types
        public static string GetTime2(float timeZoneOffset, bool use24h)
        {
            var time = DateTime.UtcNow.AddHours(timeZoneOffset);
            return use24h ? time.ToString("HH:mm") : time.ToString("hh:mm tt");
        }        
        
        // Overload with same JSON types
        public static string GetTime2(double timeZoneOffset, bool use24h)
        {
            var time = DateTime.UtcNow.AddHours(timeZoneOffset);
            return use24h ? time.ToString("HH:mm") : time.ToString("hh:mm tt");
        }
    }
}
