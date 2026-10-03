import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[appFocusFirstInvalidField]',
  standalone: true
})
export class FocusFirstInvalidFieldDirective {

  constructor(private el: ElementRef) { }

  @HostListener('submit')
  onFormSubmit() {
    const invalidElements = this.el.nativeElement.querySelectorAll('.ng-invalid');
    if (invalidElements.length > 0) {
      invalidElements[0].focus();
    }
  }

}
