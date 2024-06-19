using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.IntegrationTests.Tools
{
    /// <summary>
    /// Methods of a Class locked with [<see cref="GPTLockClass"/>] can't be used by GPT even if you attempt to add them.
    /// </summary>
    [GPTLockClass]
    internal class NonToolMethods
    {
        public static void NonToolMethods1() { }

        public static void NonToolMethods2() { }

        public static void NonToolMethods3() { }
    }
}
