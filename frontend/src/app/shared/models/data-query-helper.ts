export class DataQueryResult<T> {

    // Filter records return to client. Based on current page and page size.
    items?: T[];

    // Total records in database which matching passed filter and sorting information.
    totalRecords?: number;

    // Items for Page number
    pageNo?: number;

    // Number of records per page setting.
    size?: number;
}