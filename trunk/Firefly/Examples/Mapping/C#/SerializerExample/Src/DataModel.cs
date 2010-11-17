//==========================================================================
//
//  File:        DataModel.cs
//  Location:    Firefly.Examples <Visual Basic .Net>
//  Description: 数据模型
//  Version:     2010.11.17.
//  Author:      F.R.C.
//  Copyright(C) public Domain
//
//==========================================================================

using System;
using System.Collections.Generic;

//版本1的DataEntry
public class DataEntryVersion1
{
    public String Name;
    public Byte[] Data;
}

//当前版本(版本2)的DataEntry
public class DataEntry
{
    public String Name;
    public Byte[] Data;
    public String Attribute;
}

public class ImmutableDataEntry<T>
{
    public String Name
    {
        get
        {
            return NameValue;
        }
    }
    public T Data
    {
        get
        {
            return DataValue;
        }
    }

    private String NameValue;
    private T DataValue;
    public ImmutableDataEntry(String Name, T Data)
    {
        this.NameValue = Name;
        this.DataValue = Data;
    }
}

public class DataObject
{
    public Dictionary<String, DataEntry> DataEntries = new Dictionary<String, DataEntry>();
    public Dictionary<String, ImmutableDataEntry<Byte[]>> ImmutableDataEntries = new Dictionary<String, ImmutableDataEntry<Byte[]>>();
}
