namespace OpenAI.ChatGPT.Net.IntegrtionTests.Tools
{
    ///// <summary>
    ///// This class has the Attribute [<see cref="GPTAttributeFiltered"/>] 
    ///// which will filter out all methods that do not have the [<see cref="GPTTool"/>] 
    ///// or [<see cref="GPTParemeters"/>] attribute.
    ///// <para/>Class containing tools must be public
    ///// </summary>
    //[GPTAttributeFiltered]
    public class MyToolClassWithAttributes
    {
        /// <summary>
        /// This Method will not be added by the 
        /// <code>.AddToolClass&lt;<see cref="MyToolClassWithAttributes"/>&gt;();</code>
        /// because this class has the [<see cref="GPTAttributeFiltered"/>] Attribute which filters this method out.
        /// </summary>
        public static string MethodWithoutAttribute() => "This shoul not be included in the tools";

        ///// <summary>
        ///// [<see cref="GPTTool"/>] Provides a Description for GPT of this method
        ///// <code>
        ///// [GPTTool("This is the Tool1")]
        ///// public static string Tool1() => "Tool1";
        ///// </code>
        ///// </summary>
        //[GPTTool("This is the Tool1")]
        //public static string Tool1() => "Tool1";

        ///// <summary>
        ///// [<see cref="GPTParameters"/>] Provides a Description of the parameters for GPT of this method
        ///// Note that this code:
        ///// <code>[GPTParameters(parameter1: "This is a number", parameter2: "This is a useless parameter description that will be ignored")]
        ///// public static string Tool2(int size) => "Tool2";
        ///// </code>
        ///// will not produce an error even tough a parameter is decribed that isn't there.
        ///// This description will just be ignored.
        ///// </summary>
        //[GPTParameters(parameter1: "This is a number", parameter2: "This is a useless parameter description that will be ignored")]
        //public static string Tool2(int size) => "Tool2";

        ///// <summary>
        ///// Full example with [<see cref="GPTTool"/>] and [<see cref="GPTParameters"/>]
        ///// <code>
        ///// [GPTTool("This is the Tool3")]
        ///// [GPTParameters(parameter1: "This is a number")]
        ///// public static string Tool3(int number) => "Tool3";
        ///// </code>
        ///// </summary>
        //[GPTTool("This is the Tool3")]
        //[GPTParameters(parameter1: "This is a number")]
        //public static string Tool3(int number) => "Tool3";

        /// <summary>
        /// Non static Methods can't be used by GPT and can't have GPT Attributes
        /// </summary>
        public void NonStaticMethod() { }

        /// <summary>
        /// Non public Methods can't be used by GPT and can't have GPT Attribute
        /// </summary>
        private static void PrivateMethod() { }
    }
}
