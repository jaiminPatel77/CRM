export class ConstString {
    public static readonly asc = " asc";
    public static readonly desc = " desc";

    public static readonly ContainsSplit = "=*";  // Gridify contains: field=*value*
    public static readonly EqSplit = "=";  // Gridify equal
    public static readonly GeSplit = ",ge,";
    public static readonly LeSplit = ",le,";
    public static readonly CustomSplit = ",custom,";
    public static readonly GtSplit = ",gt,";
    public static readonly LtSplit = ",lt,";
    public static readonly NeSplit = ",ne,";
    public static readonly NotContainsSplit = ",notcontains,";
    public static readonly StartsWithSplit = ",startswith,";
    public static readonly EndsWithSplit = ",endswith,";

    public static readonly AndSplit = ",and,";
    public static readonly OrSplit = ",or,";

    public static readonly FieldSeparator = ",";
    public static readonly SortOrderAsc = " asc";
    public static readonly SortOrderDesc = " desc";

    public static readonly FilterStr = "filter";
    public static readonly OrderByStr = "orderBy";
    public static readonly PageNoStr = "page";  // Gridify param
    public static readonly SizeStr = "pageSize";  // Gridify param    

    public static readonly stringType = "string";
    public static readonly numberType = "number";
    public static readonly archive = "archive";
    public static readonly searchText = "searchText";

    public static readonly customValueSep = '~';

    /**
     * Common date format const string.
     */
    public static readonly YearMonthDay = 'YYYY-MM-DD';
}