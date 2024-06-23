using OpenAI.ChatGPT.Net.Tools;

namespace OpenAI.ChatGPT.Net.IntegrationTests.Tools
{
    /// <summary>
    /// Methods of a Class locked with [<see cref="GPT_Locked"/>] can't be used by GPT even if you attempt to add them.
    /// </summary>
    [GPT_Locked]
    internal class LockedClass
    {
        public static void NonToolMethods1() { }

        public static void NonToolMethods2() { }

        public static void NonToolMethods3() { }
    }

    internal class AllLockedClass
    {
        [GPT_Locked]
        public static void NonToolMethods1() { }

        [GPT_Locked]
        public static void NonToolMethods2() { }

        [GPT_Locked]
        public static void NonToolMethods3() { }
    }
}
