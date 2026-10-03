import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'pascalCaseToSpace',
  standalone: true
})
export class PascalCaseToSpacePipe implements PipeTransform {
  transform(value: string | undefined | null): string {
    if (!value) {
      return '';
    }
    // Adds a space before all caps that are followed by lowercase letters
    // Example: "GlobalAdministrator" -> "Global Administrator"
    return value.replace(/([A-Z][a-z])/g, ' $1').trim();
  }
}
