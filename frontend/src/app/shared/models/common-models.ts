export interface LooseObject {
  [key: string]: any
}

export enum DataStatusEnum {
  None = 0,
  Fetching = 1,
  DataAvailable = 2,
  Error = 3
}
export interface IBaseList {
  $selected?: boolean;
  $disabled?: boolean;
}