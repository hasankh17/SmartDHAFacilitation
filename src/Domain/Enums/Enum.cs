using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHAFacilitationAPIs.Domain.Enums;
public enum CategoryType
{
}
public enum Zone
{
}
public enum Relation
{
    Father = 1,
    Mother = 2,
    Son = 3,
    Daughter = 4,
    Brother = 5,
    Sister = 6,
    Spouse = 7,
}
public enum PropertyType
{
}
public enum Phase
{
}
public enum PossessionType
{
}
/// <summary>
/// Lightweight media type enum used by SaveImageOrVideoAsync.
/// Placed in the existing Domain.Enums namespace so the FileStorageService can resolve it.
/// </summary>
public enum PanicDispatchMediaType
{
    Image = 0,
    Video = 1
}

/// <summary>
/// Alternate media-type enum used by other methods in FileStorageService.
/// Kept distinct to match existing method signatures in the file.
/// </summary>
public enum FMType
{
    Image = 0,
    Video = 1
}
