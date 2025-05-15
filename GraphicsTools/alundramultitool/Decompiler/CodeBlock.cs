namespace alundramultitool.Decompiler;

public class CodeBlock<T> where T : IsInstruction
{
    public List<T> Instructions = new();

    public BlockType BlockType;
    public List<CodeBlock<T>> InEdges = new();
    public List<CodeBlock<T>> OutEdges = new();
    public List<uint> OutAddresses = new();


    public uint Address
    {
        get
        {
            return Instructions.FirstOrDefault().Address;
        }
    }

    public uint EndAddress
    {
        get
        {
            return Instructions.LastOrDefault().Address + 4;
        }
    }

    public bool IsJumpTarget
    {
        get
        {
            foreach (var edge in InEdges)
            {
                if (edge.EndAddress != Address)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool BeginsLoop
    {
        get
        {
            foreach (var edge in InEdges)
            {
                if (edge.Address >= Address)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public bool EndsLoop
    {
        get
        {
            foreach (var edge in OutEdges)
            {
                if (edge != null && edge.Address <= Address)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public BranchOperation<T> BranchOperation
    {
        get
        {
            if (BlockType == BlockType.TwoWay && Instructions[Instructions.Count - 2].IsBranch)
            {
                return new BranchOperation<T>(Instructions[Instructions.Count - 2], this);
            }
            return null;
        }
    }



}