﻿using System.IO;

namespace unforge;
public abstract class DataForgeSerializable
{
    internal DataForgeIndex DocumentRoot { get; private set; }
    internal BinaryReader Br { get { return DocumentRoot.Br; } }

    public DataForgeSerializable(DataForgeIndex documentRoot)
    {
        DocumentRoot = documentRoot;
    }
}