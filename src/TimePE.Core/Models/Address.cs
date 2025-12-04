using DevExpress.Xpo;

namespace TimePE.Core.Models;

public class Address : BaseEntity
{
    public Address(Session session) : base(session) { }

    string _street1 = string.Empty;
    [Persistent]
    [Size(200)]
    public string StreetLine1
    {
        get => _street1;
        set => SetPropertyValue(nameof(StreetLine1), ref _street1, value);
    }
    
    string _street2 = string.Empty;
    [Persistent]
    [Size(200)]
    public string StreetLine2
    {
        get => _street2;
        set => SetPropertyValue(nameof(StreetLine2), ref _street2, value);
    }

    string _city = string.Empty;
    [Persistent]
    [Size(100)]
    public string City
    {
        get => _city;
        set => SetPropertyValue(nameof(City), ref _city, value);
    }

    StateProvince _stateProvince = StateProvince.Unknown;
    [Persistent]
    public StateProvince StateProvince
    {
        get => _stateProvince;
        set => SetPropertyValue(nameof(StateProvince), ref _stateProvince, value);
    }

    string _postalCode = string.Empty;
    [Persistent]
    [Size(20)]
    public string PostalCode
    {
        get => _postalCode;
        set => SetPropertyValue(nameof(PostalCode), ref _postalCode, value);
    }

    AddressType _addressType = AddressType.Unknown;
    [Persistent]
    public AddressType AddressType
    {
        get => _addressType;
        set => SetPropertyValue(nameof(AddressType), ref _addressType, value);
    }
}

public enum AddressType
{
    Unknown,
    Residential,
    Commercial,
    Rental
}

public enum StateProvince
{
    Unknown,
    AL,
    AK,
    AZ,
    AR,
    CA,
    CO,
    CT,
    DE,
    FL,
    GA,
    HI,
    ID,
    IL,
    IN,
    IA,
    KS,
    KY,
    LA,
    ME,
    MD,
    MA,
    MI,
    MN,
    MS,
    MO,
    MT,
    NE,
    NV,
    NH,
    NJ,
    NM,
    NY,
    NC,
    ND,
    OH,
    OK,
    OR,
    PA,
    RI,
    SC,
    SD,
    TN,
    TX,
    UT,
    VT,
    VA,
    WA,
    WV,
    WI,
    WY
}   