
namespace ConsoleCord
{
    public enum RegistryType
    {
        /// <summary>
        /// ADIS Command Registry.
        /// </summary>
        ADISCR,
        /// <summary>
        /// ConsoleCord Client Command Registry.
        /// </summary>
        CCCCR,
        /// <summary>
        /// ConsoleCord Server Command Registry.
        /// </summary>
        CCSCR
    }

    public enum CCInstruction
    {
        /// <summary>
        /// Null
        /// </summary>
        nul,
        /// <summary>
        /// End Of Transmission
        /// </summary>
        eot,
        /// <summary>
        /// Echo all arguments
        /// </summary>
        echo,
        /// <summary>
        /// Request to cut connection.
        /// </summary>
        reqCutCom,
        /// <summary>
        /// Advise that the connection is being cut.
        /// </summary>
        cutCom,
    }

    /// <summary>
    /// ConsoleCord Command
    /// </summary>
    public class CCCommand
    {
        public CCInstruction instruction;
        public string[]? args;

        public CCCommand(CCInstruction instruction) : this(instruction, null) { }

        public CCCommand(string[] args) : this(CCInstruction.nul, args){ }

        public CCCommand(CCInstruction instruction = CCInstruction.nul, string[]? args = null)
        {
            this.instruction = instruction;
            this.args = args;
        }

        public override string ToString()
        {
            bool argsIsNull = args is null;
            string toString = $"ConsoleCord.CCCommand Instruction: '{instruction}' {(argsIsNull ? "There are no arguments." : "Args: ")}";
            if (!argsIsNull)
            {
                toString += $"Size: {args!.Length} {{ ";
                foreach (var a in args!)
                    toString += $"{a}, ";
                toString = string.Concat(toString.AsSpan(0, toString.Length - 2), " }");
            }
            return toString;
        }


    }

}
