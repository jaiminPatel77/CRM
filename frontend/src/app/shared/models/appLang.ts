
/**
 * Model for application lang. list
 */
export class AppLang {
    name?: string;
    key: string;
    nameKey: string;

    constructor(key: string, nameKey: string) {
        this.key = key;
        this.nameKey = nameKey;
    }
}