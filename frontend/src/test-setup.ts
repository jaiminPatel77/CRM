import '@angular/compiler';
if (typeof (globalThis as any).$localize === 'undefined') {
  (globalThis as any).$localize = (strings: any, ...values: any[]) => Array.isArray(strings) ? (strings.raw ? strings.raw[0] : strings[0]) : strings;
}
import '@angular/localize/init';
if (typeof (globalThis as any).$localize === 'undefined') {
  (globalThis as any).$localize = (strings: any, ...values: any[]) => Array.isArray(strings) ? (strings.raw ? strings.raw[0] : strings[0]) : strings;
}
import { getTestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';

getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting()
);
