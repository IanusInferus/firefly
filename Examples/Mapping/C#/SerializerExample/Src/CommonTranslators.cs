//==========================================================================
//
//  File:        CommonTranslators.cs
//  Location:    Firefly.Examples <Visual Basic .Net>
//  Description: 公共适配器
//  Version:     2010.11.17.
//  Author:      F.R.C.
//  Copyright(C) public Domain
//
//==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using Firefly;
using Firefly.TextEncoding;
using Firefly.Mapping;

//用于适配DataEntry的版本1和版本2
public class DataEntryVersion1To2Translator :
    IProjectorToProjectorRangeTranslator<DataEntry, DataEntryVersion1>, //Reader
    IProjectorToProjectorDomainTranslator<DataEntry, DataEntryVersion1> //Writer
{
    public Func<D, DataEntry> TranslateProjectorToProjectorRange<D>(Func<D, DataEntryVersion1> Projector)
    {
        return DomainValue =>
        {
            var v1 = Projector(DomainValue);
            return new DataEntry { Name = v1.Name, Data = v1.Data, Attribute = "" };
        };
    }
    public Func<DataEntry, R> TranslateProjectorToProjectorDomain<R>(Func<DataEntryVersion1, R> Projector)
    {
        return v2 =>
        {
            var v1 = new DataEntryVersion1 { Name = v2.Name, Data = v2.Data };
            return Projector(v1);
        };
    }
}
